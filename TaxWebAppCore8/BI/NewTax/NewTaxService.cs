using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NewTaxSdk.Dto.Content;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using TaxWebAppCore8.Models.SharedDTO;
using LegacyInquiry = TaxCollectData.Library.Models.InquiryResultModel;

namespace TaxWebAppCore8.BI.NewTax;

public sealed class NewTaxService
{
    private readonly IContext context;
    private readonly DTOServerInfo settings;
    private readonly INewTaxClient client;
    private readonly INewTaxStateStore stateStore;
    private readonly SemaphoreSlim sendGate = new(1, 1);

    public NewTaxService(IContext context, DTOServerInfo settings,
        INewTaxClient? client = null, INewTaxStateStore? stateStore = null)
    {
        this.context = context;
        this.settings = settings;
        this.client = client ?? new NewTaxClient(settings);
        this.stateStore = stateStore ?? new NewTaxStateStore();
    }

    public Task Init() => client.InitializeAsync();
    public Task<bool> CheckConnectionAsync() => client.CheckConnectionAsync();
    public Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search) => context.GetAllInvoice(search);

    public async Task<ResultModel> SendInvoiceAsync(DTOInvoiceLimitParameter invoice, bool isCancel = false)
    {
        await sendGate.WaitAsync();
        try
        {
            if (!Guid.TryParse(invoice.InvoiceID, out var invoiceId))
                return new() { Success = false, Message = "شناسه فاکتور معتبر نیست." };
            // Read and validate before making any network request.
            var source = context.GetLastInvoiceForSend((short)(isCancel ? 3 : 1), invoice, 1);
            var payload = NewTaxInvoiceMapper.Map(source, settings, isCancel);
            invoice.TaxGuid = payload.Header.Taxid;
            var packet = await client.SendAsync(payload);
            try
            {
                await stateStore.SaveSentAsync(invoiceId, packet, payload, isCancel);
            }
            catch
            {
                return new() { Success = false, Status = "SENT_SAVE_FAILED", Data = packet,
                    Message = $"فاکتور ارسال شد ولی ذخیره نتیجه کامل نشد. ارسال مجدد نکنید؛ شماره پیگیری: {packet.ReferenceNumber}" };
            }
            return new() { Success = true, Status = "Sent", Data = packet, Count = 1,
                Message = "فاکتور ارسال شد؛ تأیید نهایی پس از استعلام مشخص می‌شود." };
        }
        catch (Exception ex)
        {
            return new() { Success = false, Message = ex.Message };
        }
        finally { sendGate.Release(); }
    }

    public async Task<ResultModel> SendGroupInvoicesForGroupingAsync(List<DTOInvoiceLimitParameter> invoices)
    {
        var results = new List<object>();
        var sent = 0;
        foreach (var invoice in invoices.DistinctBy(x => x.InvoiceID))
        {
            var result = await SendInvoiceAsync(invoice);
            if (result.Success) sent++;
            results.Add(new { invoice.InvoiceID, result.Success, result.Status, result.Message, result.Data });
            // Do not proceed with more submissions after a local persistence failure.
            if (result.Status == "SENT_SAVE_FAILED") break;
        }
        var total = invoices.DistinctBy(x => x.InvoiceID).Count();
        return new() { Success = total > 0 && sent == total, Count = sent, Data = results,
            Message = $"{sent} از {total} فاکتور ارسال شد؛ نتیجه هر فاکتور در جزئیات موجود است." };
    }

    public async Task<ResultModel> GetInvoiceStatusByRefrenceAsync(string invoiceId)
    {
        var history = context.GetTaxHistory(Guid.Parse(invoiceId));
        if (string.IsNullOrWhiteSpace(history?.ReferenceNumber))
            return new() { Success = false, Message = "شماره پیگیری برای این فاکتور موجود نیست." };
        return await InquiryAndSaveAsync(invoiceId, history.ReferenceNumber);
    }

    private async Task<ResultModel> InquiryAndSaveAsync(string invoiceId, string reference)
    {
        var responses = await client.InquiryAsync(reference);
        var matched = responses.Where(x => string.Equals(x.ReferenceNumber, reference, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matched.Count != 1)
            return new() { Success = false, Message = "پاسخ استعلام منطبق با شماره پیگیری دریافت نشد." };
        var response = matched[0];
        var status = response.Status?.ToUpperInvariant() ?? "";
        var data = response.Data is System.Text.Json.JsonElement element
            ? JToken.Parse(element.GetRawText()) : response.Data == null ? new JObject() : JToken.FromObject(response.Data);
        var errors = data is JObject ? data["error"] ?? data["Error"] : null;
        var message = status switch
        {
            "SUCCESS" => "صورتحساب با موفقیت در کارپوشه ثبت شد.",
            "FAILED" => string.Join("؛ ", (errors as JArray ?? new JArray()).Select(x => (string?)(x["message"] ?? x["msg"] ?? x["Message"] ?? x["Msg"]) ?? "خطای اعتبارسنجی")),
            "PENDING" or "IN_PROGRESS" => "صورتحساب در حال بررسی است.",
            "NOT_FOUND" => "صورتحساب پیدا نشد؛ پیش از ارسال مجدد بررسی شود.",
            "TIMEOUT" => "پردازش صورتحساب به پایان نرسیده است؛ مجدداً استعلام کنید.",
            _ => "وضعیت ناشناخته از NewTax دریافت شد."
        };
        var state = status switch { "SUCCESS" => 4, "FAILED" => 3, _ => 2 };
        if (string.IsNullOrWhiteSpace(message)) message = "صورتحساب رد شد؛ جزئیات خطا در پاسخ موجود نیست.";
        var codes = string.Join("؛ ", (errors as JArray ?? new JArray()).Select(x => (string?)(x["code"] ?? x["Code"])));
        await stateStore.SaveInquiryAsync(Guid.Parse(invoiceId), response, state, message, codes);
        return new() { Success = status is "SUCCESS" or "FAILED" or "PENDING" or "IN_PROGRESS" or "NOT_FOUND" or "TIMEOUT",
            Status = status, Message = message, Data = response };
    }

    public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoiceByInqFromServer(SearchParameters search)
    {
        var models = await context.GetAllInvoice(search);
        foreach (var model in models)
        {
            var reference = new[] { model.fTaxHistoryReferenceNumber, model.taxReferenceNumber, model.ReferenceNumber }
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            if (reference == null) continue;
            try
            {
                var result = await InquiryAndSaveAsync(model.InvoiceId, reference);
                model.WasQueriedFromTaxService = result.Success;
                model.InquiryFailed = !result.Success;
                model.InqMessage = result.Message;
                if (result.Success)
                {
                    model.fSendState = result.Status switch { "SUCCESS" => 4, "FAILED" => 3, _ => 2 };
                    model.fListOfErrorCaption = result.Message;
                }
            }
            catch (Exception ex) { model.InquiryFailed = true; model.InqMessage = ex.Message; }
        }
        return models;
    }

    public async Task<List<LegacyInquiry>> GetInvoiceStatusByDateRange(string from, string to)
    {
        static string Normalize(string value)
        {
            var date = MD.PersianDateTime.PersianDateTime.Parse(value).ToDateTime();
            var calendar = new PersianCalendar();
            return $"{calendar.GetYear(date):0000}{calendar.GetMonth(date):00}{calendar.GetDayOfMonth(date):00}";
        }
        var responses = await client.InquiryByDateAsync(Normalize(from), Normalize(to));
        return responses.Select(ToLegacyInquiry).ToList();
    }

    public static LegacyInquiry ToLegacyInquiry(InquiryResultModel response)
    {
        var json = JObject.Parse(JsonConvert.SerializeObject(response));
        if (response.Data is System.Text.Json.JsonElement element) json["Data"] = JToken.Parse(element.GetRawText());
        if (json["Data"] is JObject data)
        {
            foreach (var array in data.Properties().Where(x => x.Value is JArray).Select(x => (JArray)x.Value))
                foreach (var item in array.OfType<JObject>())
                    if (item["msg"] != null) item["message"] = item["msg"];
        }
        return json.ToObject<LegacyInquiry>()!;
    }

    public async Task GetNotFinishedLastRefrenceIdAsync()
    {
        var invoiceId = await context.GetNotFinishedLastRefrenceId();
        if (!string.IsNullOrWhiteSpace(invoiceId)) await GetInvoiceStatusByRefrenceAsync(invoiceId);
    }

    public Task<ResultModel> GetInvoiceByID(string invoiceId) => GetInvoiceStatusByRefrenceAsync(invoiceId);
}
