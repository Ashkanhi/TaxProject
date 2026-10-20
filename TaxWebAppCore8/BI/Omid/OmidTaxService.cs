using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxWebAppCore8.Models.SharedDTO;

namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidTaxService
    {
        private readonly IContext _context;

        // تنظیمات همین مشتری؛ MEMORY_ID و TINS از اینجا خوانده می‌شوند.
        private readonly DTOServerInfo _serverInfo;

        public OmidTaxService(IContext context, DTOServerInfo serverInfo)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));

            _serverInfo = serverInfo
                ?? throw new ArgumentNullException(nameof(serverInfo));
        }

        /// <summary>
        /// یک فاکتور را از دیتابیس می‌خواند و درخواست امید را می‌سازد.
        /// این متد هیچ درخواست HTTP ارسال نمی‌کند.
        /// </summary>
        public OmidSendInvoiceRequest BuildRequest(
            DTOInvoiceLimitParameter invoice)
        {
            if (invoice == null ||
                !Guid.TryParse(invoice.InvoiceID, out _) ||
                !invoice.StoreID.HasValue)
            {
                throw new InvalidOperationException(
                    "InvoiceID معتبر و StoreID برای ارسال به امید لازم است.");
            }

            // همان شاخهٔ خواندن دیتابیس که RimaCalculate استفاده می‌کند.
            var source = _context.GetLastInvoiceForSend(
                invoicetype: 1,
                invoice: invoice,
                isForTis: 2);

            if (source?.Header == null)
                throw new InvalidOperationException(
                    "سربرگ فاکتور از دیتابیس برنگشت.");

            if (source.Rows == null || source.Rows.Count == 0)
                throw new InvalidOperationException(
                    "ردیف‌های فاکتور از دیتابیس برنگشتند.");

            // MEMORY_ID همین مشتری، FiscalId امید است.
            string fiscalId = _serverInfo.MEMORY_ID?.Trim();

            if (string.IsNullOrWhiteSpace(fiscalId))
                throw new InvalidOperationException(
                    "MEMORY_ID در CustomerInformation.xml خالی است.");

            return OmidInvoiceMapper.Map(
                source,
                fiscalId,
                _serverInfo.TINS);
        }

        /// <summary>
        /// چند فاکتور را از دیتابیس می‌خواند و در یک درخواست امید قرار می‌دهد.
        /// این متد نه HTTP ارسال می‌کند و نه رکوردی در جدول وضعیت ثبت می‌کند.
        /// </summary>
        public OmidSendInvoiceRequest BuildBatchRequest(
            IReadOnlyCollection<DTOInvoiceLimitParameter> selectedInvoices)
        {
            if (selectedInvoices == null || selectedInvoices.Count == 0)
                throw new InvalidOperationException(
                    "حداقل یک فاکتور برای ارسال انتخاب کنید.");

            var batch = new OmidSendInvoiceRequest();
            var invoiceIds = new HashSet<Guid>();
            var invoiceNumbers = new HashSet<long>();

            int index = 0;

            foreach (var selected in selectedInvoices)
            {
                index++;

                if (selected == null ||
                    !Guid.TryParse(selected.InvoiceID, out Guid invoiceId) ||
                    invoiceId == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        $"InvoiceID فاکتور شمارهٔ {index} معتبر نیست.");
                }

                if (!invoiceIds.Add(invoiceId))
                    throw new InvalidOperationException(
                        $"فاکتور با InvoiceID برابر {invoiceId} دوبار انتخاب شده است.");

                // هر فاکتور با همان روش ارسال تکی از دیتابیس خوانده می‌شود.
                OmidSendInvoiceRequest single = BuildRequest(selected);

                if (single.Invoices == null || single.Invoices.Count != 1)
                    throw new InvalidOperationException(
                        $"برای فاکتور شمارهٔ {index} دقیقاً یک صورتحساب ساخته نشد.");

                // تمام اعضای یک درخواست باید fiscalId یکسان داشته باشند.
                if (index == 1)
                {
                    batch.FiscalId = single.FiscalId;
                }
                else if (!string.Equals(
                    batch.FiscalId,
                    single.FiscalId,
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "فاکتورهای انتخاب‌شده fiscalId یکسان ندارند.");
                }

                OmidInvoice omidInvoice = single.Invoices[0];

                if (omidInvoice.Header?.Inno is not long inno)
                    throw new InvalidOperationException(
                        $"inno فاکتور شمارهٔ {index} معتبر نیست.");

                // رسید امید InvoiceID محلی ندارد و با inno تطبیق داده می‌شود.
                if (!invoiceNumbers.Add(inno))
                    throw new InvalidOperationException(
                        $"شمارهٔ فاکتور {inno} در دسته تکراری است.");

                batch.Invoices.Add(omidInvoice);
            }

            ValidateForSend(batch);
            return batch;
        }

        /// <summary>
        /// یک فاکتور را به امید ارسال می‌کند و رسید آن را جداگانه ثبت می‌کند.
        /// Pending یعنی امید درخواست را پذیرفته؛ تأیید نهایی هنوز معلوم نیست.
        /// </summary>
        public async Task<(string LocalInvoiceId, OmidSendReceipt Receipt)>
            SubmitSingleInvoiceAsync(
                DTOInvoiceLimitParameter invoice,
                OmidApiClient client,
                CancellationToken cancellationToken = default)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            var payload = BuildRequest(invoice);
            ValidateForSend(payload);

            if (!Guid.TryParse(invoice.InvoiceID, out Guid localInvoiceId))
                throw new InvalidOperationException(
                    "InvoiceID فاکتور معتبر نیست.");

            var store = new OmidSubmissionStore(_serverInfo);

            // ابتدا Login می‌کنیم؛ اگر شبکه یا سرویس ورود مشکل داشته باشد،
            // هنوز هیچ رکوردی Sending نمی‌شود و فاکتور قفل نخواهد شد.
            string token;
            try
            {
                token = await client.LoginAsync(cancellationToken);
            }
            catch (Exception loginError)
            {
                await TryLogAttemptErrorAsync(
                    store,
                    localInvoiceId,
                    "Login",
                    "/api/Auth/login",
                    loginError,
                    "LoginFailed");

                // خطای ورود پیش از ارسال رخ می‌دهد؛ درخواست صورتحساب به امید
                // نرسیده و کاربر می‌تواند پس از رفع ارتباط دوباره تلاش کند.
                throw new InvalidOperationException(
                    "ورود به امید ناموفق بود؛ درخواست ارسال صورتحساب هنوز به امید ارسال نشده است. " +
                    "وضعیت قبلی فاکتور تغییر نکرده و پس از رفع مشکل ارتباط می‌توانید دوباره ارسال کنید.",
                    loginError);
            }

            bool saved = await store.TrySaveOmidInvoiceSubmissionAsync(
                localInvoiceId,
                cancellationToken);

            if (!saved)
                throw new InvalidOperationException(
                    "این فاکتور قبلاً برای ارسال به امید ثبت شده است؛ " +
                    "قبل از هر اقدام، وضعیت آن را در OmidInvoiceSubmission بررسی کنید.");

            await store.MarkSendingAsync(
                localInvoiceId,
                cancellationToken);

            try
            {
                OmidSendResponse response = await client.SendInvoiceAsync(
                    payload,
                    token,
                    cancellationToken);

                OmidSendReceipt receipt =
                    OmidSendResponseParser.RequireSingleAccepted(response);

                // حتی اگر مرورگر درخواست را لغو کند، رسید باید ذخیره شود.
                await store.MarkPendingAsync(
                    localInvoiceId,
                    receipt,
                    CancellationToken.None);

                return (
                    LocalInvoiceId: invoice.InvoiceID,
                    Receipt: receipt);
            }
            catch (Exception sendError)
            {
                if (TryGetDefinitiveRejection(sendError, out string rejection))
                {
                    await TryLogAttemptErrorAsync(
                        store,
                        localInvoiceId,
                        "SendInvoice",
                        "/api/Invoice/SendInvoice",
                        sendError,
                        "Rejected",
                        rejection);

                    try
                    {
                        await store.MarkFailedAsync(
                            localInvoiceId,
                            rejection,
                            CancellationToken.None);
                    }
                    catch (Exception stateError)
                    {
                        throw new AggregateException(
                            "امید صورتحساب را رد کرد، اما ثبت وضعیت ردشده در دیتابیس ناموفق بود.",
                            sendError,
                            stateError);
                    }

                    throw new InvalidOperationException(
                        $"درخواست به امید رسید، اما صورتحساب پذیرفته نشد. علت: {rejection} " +
                        "پس از اصلاح اطلاعات، امکان ارسال دوباره وجود دارد.",
                        sendError);
                }

                await TryLogAttemptErrorAsync(
                    store,
                    localInvoiceId,
                    "SendInvoice",
                    "/api/Invoice/SendInvoice",
                    sendError,
                    "Unknown");

                // قطع ارتباط پس از شروع ارسال ثابت نمی‌کند که امید
                // فاکتور را دریافت نکرده است؛ ارسال خودکار مجدد ممنوع است.
                try
                {
                    await store.MarkUnknownAsync(
                        localInvoiceId,
                        sendError.Message,
                        CancellationToken.None);
                }
                catch (Exception stateError)
                {
                    throw new AggregateException(
                        "نتیجهٔ ارسال و ثبت وضعیت آن نامشخص است. " +
                        "پیش از ارسال دوباره، رکورد فاکتور را بررسی کنید.",
                        sendError,
                        stateError);
                }

                throw new InvalidOperationException(
                    "نتیجهٔ ارسال به امید نامشخص است و وضعیت Unknown ثبت شد. " +
                    "این فاکتور را خودکار دوباره ارسال نکنید.",
                    sendError);
            }
        }

        /// <summary>
        /// چند فاکتور را در یک درخواست HTTP به امید می‌فرستد.
        /// رسید هر فاکتور با inno به InvoiceID محلی مربوط می‌شود.
        /// </summary>
        public async Task<OmidBatchSendResult> SubmitBatchInvoicesAsync(
            IReadOnlyCollection<DTOInvoiceLimitParameter> selectedInvoices,
            OmidApiClient client,
            CancellationToken cancellationToken = default)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (selectedInvoices == null)
                throw new ArgumentNullException(nameof(selectedInvoices));

            var selected = new List<DTOInvoiceLimitParameter>(selectedInvoices);

            // یک فاکتور از مسیر ارسال تکی عبور می‌کند.
            // دسته‌های بزرگ‌تر از ۱۰۰ بعداً در کنترلر تقسیم می‌شوند.
            if (selected.Count < 2 || selected.Count > 100)
                throw new InvalidOperationException(
                    "تعداد فاکتورهای این درخواست باید بین ۲ تا ۱۰۰ باشد.");

            // ابتدا همهٔ داده‌ها خوانده و بررسی می‌شوند.
            // تا اینجا هیچ رکوردی ذخیره و هیچ HTTP ارسالی انجام نشده است.
            OmidSendInvoiceRequest payload = BuildBatchRequest(selected);

            var result = new OmidBatchSendResult();
            var invoiceIds = new List<Guid>();
            var itemByInno = new Dictionary<long, OmidBatchSendItemResult>();

            // ترتیب payload همان ترتیب selected است.
            for (int i = 0; i < selected.Count; i++)
            {
                Guid invoiceId = Guid.Parse(selected[i].InvoiceID);
                long inno = payload.Invoices[i].Header.Inno!.Value;

                var item = new OmidBatchSendItemResult
                {
                    InvoiceId = invoiceId,
                    Inno = inno,
                    State = "Prepared"
                };

                invoiceIds.Add(invoiceId);
                itemByInno.Add(inno, item);
                result.Items.Add(item);
            }

            var store = new OmidSubmissionStore(_serverInfo);

            // ورود پیش از ثبت Sending انجام می‌شود تا خطای Login به اشتباه
            // به عنوان نتیجهٔ نامشخص ارسال صورتحساب ذخیره نشود.
            string token;
            try
            {
                token = await client.LoginAsync(cancellationToken);
            }
            catch (Exception loginError)
            {
                foreach (Guid invoiceId in invoiceIds)
                {
                    await TryLogAttemptErrorAsync(
                        store,
                        invoiceId,
                        "Login",
                        "/api/Auth/login",
                        loginError,
                        "LoginFailed");
                }

                // Login قبل از ذخیرهٔ دسته و پیش از ارسال صورتحساب انجام می‌شود؛
                // بنابراین در این نقطه ارسال امن است که پس از رفع مشکل تکرار شود.
                throw new InvalidOperationException(
                    "ورود به امید ناموفق بود؛ هیچ‌یک از صورتحساب‌های این دسته ارسال نشده‌اند. " +
                    "وضعیت قبلی آن‌ها تغییر نکرده و پس از رفع مشکل ارتباط می‌توانید دوباره ارسال کنید.",
                    loginError);
            }

            // همهٔ اعضای دسته با هم در OmidInvoiceSubmission ذخیره می‌شوند.
            // اگر یکی از قبل وجود داشته باشد، درخواست به امید ارسال نمی‌شود.
            bool saved =
                await store.TrySaveOmidInvoiceSubmissionBatchAsync(
                    invoiceIds,
                    cancellationToken);

            if (!saved)
                throw new InvalidOperationException(
                    "حداقل یکی از فاکتورهای انتخاب‌شده قبلاً در " +
                    "OmidInvoiceSubmission ثبت شده است. دسته ارسال نشد.");

            // هنوز درخواست HTTP شروع نشده است. اگر تغییر وضعیت خطا بدهد،
            // فقط رکوردهای باقی‌مانده در Prepared را پاک می‌کنیم.
            try
            {
                await store.MarkBatchSendingAsync(
                    invoiceIds,
                    cancellationToken);
            }
            catch (Exception transitionError)
            {
                int deletedCount;

                try
                {
                    deletedCount =
                        await store.DeletePreparedOmidInvoiceSubmissionBatchAsync(
                            invoiceIds,
                            CancellationToken.None);
                }
                catch (Exception cleanupError)
                {
                    throw new AggregateException(
                        "تغییر وضعیت دسته و پاک‌سازی رکوردهای Prepared ناموفق بود. " +
                        "پیش از تلاش دوباره، جدول OmidInvoiceSubmission را بررسی کنید.",
                        transitionError,
                        cleanupError);
                }

                // اگر همهٔ رکوردها Prepared نبوده‌اند، ممکن است تغییر وضعیت
                // در دیتابیس انجام شده باشد؛ خودکار دوباره ارسال نمی‌کنیم.
                if (deletedCount != invoiceIds.Count)
                {
                    throw new InvalidOperationException(
                        "درخواست به امید ارسال نشد، اما وضعیت بعضی رکوردهای محلی " +
                        "Prepared نبود. پیش از تلاش دوباره جدول را بررسی کنید.",
                        transitionError);
                }

                throw new InvalidOperationException(
                    "درخواست به امید ارسال نشد و رکوردهای Prepared پاک شدند؛ " +
                    "می‌توانید پس از بررسی خطا دوباره تلاش کنید.",
                    transitionError);
            }

            foreach (var item in result.Items)
                item.State = "Sending";

            // با ثبت هر رسید، InvoiceID مربوط از این مجموعه حذف می‌شود.
            var withoutStoredReceipt = new HashSet<Guid>(invoiceIds);

            try
            {
                // این خط یک درخواست HTTP با تمام اعضای payload.Invoices می‌فرستد.
                OmidSendResponse response = await client.SendInvoiceAsync(
                    payload,
                    token,
                    cancellationToken);

                if (response == null || !response.Success)
                {
                    throw new InvalidOperationException(
                        response?.Message
                        ?? "امید درخواست ارسال دسته‌ای را نپذیرفت.");
                }

                // ترتیب رسیدهای پاسخ ممکن است با ترتیب فاکتورهای ارسالی
                // فرق داشته باشد؛ بنابراین بر اساس inno تطبیق می‌دهیم.
                var receiptsByInno =
                    new Dictionary<long, List<OmidSendReceipt>>();

                if (response.Receipts != null)
                {
                    foreach (OmidSendReceipt receipt in response.Receipts)
                    {
                        if (receipt == null ||
                            !long.TryParse(
                                receipt.InvoiceNumber?.Trim(),
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture,
                                out long inno) ||
                            !itemByInno.ContainsKey(inno))
                        {
                            result.UnmatchedReceiptCount++;
                            continue;
                        }

                        if (!receiptsByInno.TryGetValue(
                            inno,
                            out List<OmidSendReceipt>? matches))
                        {
                            matches = new List<OmidSendReceipt>();
                            receiptsByInno.Add(inno, matches);
                        }

                        matches.Add(receipt);
                    }
                }

                foreach (var item in result.Items)
                {
                    // برای استعلام، رسید باید requestId داشته باشد.
                    if (!receiptsByInno.TryGetValue(
                            item.Inno,
                            out List<OmidSendReceipt>? matches) ||
                        matches.Count != 1 ||
                        string.IsNullOrWhiteSpace(matches[0].RequestId))
                    {
                        continue;
                    }

                    OmidSendReceipt receipt = matches[0];

                    await store.MarkPendingAsync(
                        item.InvoiceId,
                        receipt,
                        CancellationToken.None);

                    item.State = "Pending";
                    item.RequestId = receipt.RequestId;
                    item.TrackingId = receipt.TrackingId;
                    withoutStoredReceipt.Remove(item.InvoiceId);
                }

                // رسید ناقص، تکراری یا پیدا‌نشده نتیجهٔ آن فاکتور را
                // نامشخص می‌کند؛ آن فاکتور خودکار دوباره ارسال نمی‌شود.
                foreach (var item in result.Items)
                {
                    if (!withoutStoredReceipt.Contains(item.InvoiceId))
                        continue;

                    const string reason =
                        "رسید معتبر و یکتا برای این فاکتور در پاسخ امید پیدا نشد.";

                    await store.MarkUnknownAsync(
                        item.InvoiceId,
                        reason,
                        CancellationToken.None);

                    item.State = "Unknown";
                    item.Message = reason;
                    withoutStoredReceipt.Remove(item.InvoiceId);
                }

                return result;
            }
            catch (Exception sendError)
            {
                if (TryGetDefinitiveRejection(sendError, out string rejection))
                {
                    var rejectionStateErrors = new List<Exception>();
                    foreach (var item in result.Items)
                    {
                        if (!withoutStoredReceipt.Contains(item.InvoiceId))
                            continue;

                        await TryLogAttemptErrorAsync(
                            store,
                            item.InvoiceId,
                            "SendInvoice",
                            "/api/Invoice/SendInvoice",
                            sendError,
                            "Rejected",
                            rejection);

                        try
                        {
                            await store.MarkFailedAsync(
                                item.InvoiceId,
                                rejection,
                                CancellationToken.None);
                            item.State = "Failed";
                            item.Message = rejection;
                            withoutStoredReceipt.Remove(item.InvoiceId);
                        }
                        catch (Exception stateError)
                        {
                            rejectionStateErrors.Add(stateError);
                        }
                    }

                    if (rejectionStateErrors.Count > 0)
                    {
                        rejectionStateErrors.Insert(0, sendError);
                        throw new AggregateException(
                            "امید دستهٔ صورتحساب‌ها را رد کرد، اما ثبت وضعیت بعضی فاکتورها ناموفق بود.",
                            rejectionStateErrors);
                    }

                    throw new InvalidOperationException(
                        $"درخواست دسته‌ای به امید رسید، اما صورتحساب‌ها پذیرفته نشدند. علت: {rejection} " +
                        "پس از اصلاح اطلاعات، امکان ارسال دوباره وجود دارد.",
                        sendError);
                }

                // بعد از شروع ارسال، قطع ارتباط لزوماً به معنی
                // نرسیدن درخواست به امید نیست.
                var stateErrors = new List<Exception>();

                foreach (var item in result.Items)
                {
                    if (!withoutStoredReceipt.Contains(item.InvoiceId))
                        continue;

                    await TryLogAttemptErrorAsync(
                        store,
                        item.InvoiceId,
                        "SendInvoice",
                        "/api/Invoice/SendInvoice",
                        sendError,
                        "Unknown");

                    try
                    {
                        await store.MarkUnknownAsync(
                            item.InvoiceId,
                            sendError.Message,
                            CancellationToken.None);

                        item.State = "Unknown";
                        item.Message = sendError.Message;
                        withoutStoredReceipt.Remove(item.InvoiceId);
                    }
                    catch (Exception stateError)
                    {
                        stateErrors.Add(stateError);
                    }
                }

                if (stateErrors.Count > 0)
                {
                    stateErrors.Insert(0, sendError);

                    throw new AggregateException(
                        "ارسال دسته‌ای و ثبت وضعیت برخی فاکتورها نامشخص است. " +
                        "قبل از ارسال دوباره، جدول OmidInvoiceSubmission را بررسی کنید.",
                        stateErrors);
                }

                throw new InvalidOperationException(
                    "نتیجهٔ ارسال دسته‌ای نامشخص است. " +
                    "فاکتورهای بدون رسید Unknown شدند؛ آن‌ها را خودکار دوباره ارسال نکنید.",
                    sendError);
            }
        }

        /// <summary>
        /// پاسخ HTTP منفیِ ساختاریافتهٔ امید را از قطع ارتباط تشخیص می‌دهد.
        /// فقط وقتی پاسخ واقعی با success=false و errors دریافت شده باشد،
        /// نتیجه را رد قطعی در نظر می‌گیریم؛ سایر خطاها Unknown می‌مانند.
        /// </summary>
        private static bool TryGetDefinitiveRejection(
            Exception exception,
            out string rejection)
        {
            rejection = string.Empty;

            if (exception is not HttpRequestException httpError ||
                httpError.StatusCode == null)
            {
                return false;
            }

            const string responseMarker = "پاسخ سرور:";
            int markerIndex = httpError.Message.IndexOf(
                responseMarker,
                StringComparison.Ordinal);

            if (markerIndex < 0)
                return false;

            string responseJson = httpError.Message[
                (markerIndex + responseMarker.Length)..].Trim();

            try
            {
                JObject response = JObject.Parse(responseJson);
                if (response["success"]?.Value<bool>() != false)
                    return false;

                JToken? errors = response["errors"];
                if (errors is JArray errorArray)
                {
                    rejection = string.Join(
                        " | ",
                        errorArray.Select(item => item?.ToString())
                            .Where(message => !string.IsNullOrWhiteSpace(message)));
                }
                else if (errors != null && errors.Type != JTokenType.Null)
                {
                    rejection = errors.ToString();
                }

                if (string.IsNullOrWhiteSpace(rejection))
                    rejection = response["message"]?.ToString() ?? string.Empty;

                return !string.IsNullOrWhiteSpace(rejection);
            }
            catch
            {
                // اگر بدنه JSON معتبر نباشد، نتیجه را رد قطعی فرض نمی‌کنیم.
                return false;
            }
        }

        /// <summary>
        /// لاگ ثانویه نباید باعث تغییر نتیجهٔ اصلی ارسال شود.
        /// </summary>
        private static async Task TryLogAttemptErrorAsync(
            OmidSubmissionStore store,
            Guid? invoiceId,
            string operation,
            string endpoint,
            Exception error,
            string outcome,
            string? message = null)
        {
            try
            {
                int? statusCode = error is HttpRequestException httpError &&
                                  httpError.StatusCode.HasValue
                    ? (int)httpError.StatusCode.Value
                    : null;

                await store.LogAttemptErrorAsync(
                    invoiceId,
                    operation,
                    endpoint,
                    statusCode,
                    outcome,
                    message ?? error.Message,
                    CancellationToken.None);
            }
            catch (Exception logError)
            {
                System.Diagnostics.Trace.TraceWarning(
                    "Omid attempt log could not be written: {0}",
                    logError.GetType().Name);
            }
        }

        /// <summary>
        /// درخواست ساخته‌شده را پیش از ارسال واقعی بررسی می‌کند.
        /// این متد اطلاعات را تغییر نمی‌دهد و HTTP ارسال نمی‌کند.
        /// </summary>
        public static void ValidateForSend(
            OmidSendInvoiceRequest request)
        {
            if (request == null)
                throw new InvalidOperationException(
                    "درخواست ارسال به امید ساخته نشده است.");

            // FiscalId از MEMORY_ID مشتری گرفته می‌شود.
            if (string.IsNullOrWhiteSpace(request.FiscalId) ||
                request.FiscalId.Trim().Length != 6)
            {
                throw new InvalidOperationException(
                    "FiscalId امید باید شناسهٔ معتبر ۶ کاراکتری حافظهٔ مالیاتی باشد.");
            }

            if (request.Invoices == null ||
                request.Invoices.Count == 0)
            {
                throw new InvalidOperationException(
                    "درخواست امید هیچ فاکتوری ندارد.");
            }

            for (int i = 0; i < request.Invoices.Count; i++)
            {
                var invoice = request.Invoices[i];

                if (invoice?.Header == null)
                    throw new InvalidOperationException(
                        $"سربرگ فاکتور شمارهٔ {i + 1} وجود ندارد.");

                // طبق نتیجهٔ تست قبلی، taxId باید رشتهٔ خالی باشد.
                if (invoice.Header.TaxId != string.Empty)
                    throw new InvalidOperationException(
                        $"taxId فاکتور شمارهٔ {i + 1} باید رشتهٔ خالی باشد.");

                if (!invoice.Header.Inno.HasValue || invoice.Header.Inno.Value <= 0)
                    throw new InvalidOperationException(
                        $"inno فاکتور شمارهٔ {i + 1} خالی یا نامعتبر است.");

                if (string.IsNullOrWhiteSpace(invoice.Header.Indatim))
                    throw new InvalidOperationException(
                        $"indatim فاکتور شمارهٔ {i + 1} خالی است.");

                if (invoice.Body == null || invoice.Body.Count == 0)
                    throw new InvalidOperationException(
                        $"فاکتور شمارهٔ {i + 1} ردیف ندارد.");
            }
        }
    }

    public sealed class OmidBatchSendResult
    {
        public List<OmidBatchSendItemResult> Items { get; set; } = new();

        // تعداد رسیدهایی که به هیچ inno ارسالی وصل نشدند.
        public int UnmatchedReceiptCount { get; set; }
    }

    public sealed class OmidBatchSendItemResult
    {
        public Guid InvoiceId { get; set; }
        public long Inno { get; set; }
        public string State { get; set; } = string.Empty;
        public string? RequestId { get; set; }
        public string? TrackingId { get; set; }
        public string? Message { get; set; }
    }
}
