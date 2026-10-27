using System.Globalization;
using Newtonsoft.Json;
using NewTaxSdk.Business;
using NewTaxSdk.Dto.Content;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;

namespace TaxWebAppCore8.BI.NewTax;

public static class NewTaxInvoiceMapper
{
    public static InvoiceDto Map(Invoice invoice, DTOServerInfo settings, bool isCancel)
    {
        var source = invoice?.Header ?? throw new InvalidOperationException("فاکتور پیدا نشد.");
        if (invoice!.Rows == null || invoice.Rows.Count == 0)
            throw new InvalidOperationException("فاکتور ردیف ندارد.");
        if (!long.TryParse(source.Inno?.Replace("-", ""), NumberStyles.None, CultureInfo.InvariantCulture, out var serial)
            || serial <= 0 || serial > 999_999_999_999)
            throw new InvalidOperationException("سریال فاکتور باید عدد مثبت با حداکثر ۱۲ رقم باشد.");
        if (source.InvoiceDate == null)
            throw new InvalidOperationException("تاریخ صدور فاکتور الزامی است.");
        if (string.IsNullOrWhiteSpace(settings.TINS))
            throw new InvalidOperationException("شماره اقتصادی فروشنده الزامی است.");
        if (isCancel && string.IsNullOrWhiteSpace(source.Irtaxid))
            throw new InvalidOperationException("شماره مالیاتی مرجع برای ابطال الزامی است.");

        // DB dates without an offset represent Iranian local time, independently of the host timezone.
        var issued = source.InvoiceDate.Value;
        var instant = issued.Kind == DateTimeKind.Unspecified
            ? TimeZoneInfo.ConvertTimeToUtc(issued, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"))
            : issued.ToUniversalTime();
        var taxId = TaxIdGenerator.Create().GenerateTaxId(settings.MEMORY_ID.Trim(), serial, instant);
        var body = JsonConvert.DeserializeObject<List<InvoiceBodyDto>>(JsonConvert.SerializeObject(invoice.Rows))!;
        if (source.Tvam == null || body.Sum(row => row.Vam ?? 0) != source.Tvam.Value)
            throw new InvalidOperationException("مبلغ مالیات کل با جمع مالیات ردیف‌ها برابر نیست.");
        if (body.Any(row => string.IsNullOrWhiteSpace(row.Sstid)))
            throw new InvalidOperationException("شناسه کالا یا خدمت همه ردیف‌ها الزامی است.");

        return new InvoiceDto
        {
            Header = new InvoiceHeaderDto
            {
                Taxid = taxId, Indatim = new DateTimeOffset(instant).ToUnixTimeMilliseconds(),
                Indati2m = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Inno = serial.ToString("X", CultureInfo.InvariantCulture).PadLeft(10, '0'),
                Inty = source.Inty, Inp = source.Inp, Ins = isCancel ? 3 : source.Ins,
                Irtaxid = source.Irtaxid, Tins = settings.TINS.Trim(), Tob = source.Tob,
                Bid = source.Bid, Tinb = source.Tinb, Sbc = source.Sbc, Bpc = source.Bpc,
                Bbc = source.Bbc, Ft = source.Ft, Bpn = source.Bpn, Scln = source.Scln,
                Scc = source.Scc, Crn = source.Crn, Billid = source.Billid,
                Tprdis = source.Tprdis, Tdis = source.Tdis, Tadis = source.Tadis,
                Tvam = source.Tvam, Todam = source.Todam, Tbill = source.Tbill,
                Setm = source.Setm, Cap = source.Cap, Insp = source.Insp, Tvop = source.Tvop,
                Tax17 = source.Tax17, Cdcn = source.Cdcn, Cdcd = source.Cdcd,
                Tonw = source.Tonw, Torv = source.Torv, Tocv = source.Tocv
            },
            Body = body,
            // Payment data is not supplied by the existing DB model; do not invent a payment method.
            Payments = new(), Extension = new()
        };
    }
}
