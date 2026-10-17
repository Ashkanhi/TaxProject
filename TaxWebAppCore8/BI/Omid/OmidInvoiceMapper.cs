using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TaxServiceAndApp.Models.TaxDTO;

namespace TaxWebAppCore8.BI.Omid
{
    public static class OmidInvoiceMapper
    {
        public static OmidSendInvoiceRequest Map(
            Invoice source,
            string fiscalId,
            string tins)
        {
            if (source?.Header == null)
                throw new InvalidOperationException(
                    "سربرگ فاکتور از دیتابیس برنگشت.");

            if (source.Rows == null || source.Rows.Count == 0)
                throw new InvalidOperationException(
                    "ردیف‌های فاکتور از دیتابیس برنگشت.");

            if (string.IsNullOrWhiteSpace(fiscalId))
                throw new InvalidOperationException(
                    "Omid: FiscalId در تنظیمات خالی است.");

            var sourceHeader = source.Header;

            // پیش از ساخت درخواست ارسال، شمارهٔ فاکتور را از دادهٔ اصلی بررسی می‌کنیم.
            // در صورت خالی یا نامعتبر بودن، هیچ رکوردی رزرو و هیچ HTTP ارسالی انجام نمی‌شود.
            if (!long.TryParse(
                    sourceHeader.Inno?.Replace("-", ""),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long inno) || inno <= 0)
            {
                throw new InvalidOperationException(
                    $"شمارهٔ فاکتور (inno) برای فاکتور {sourceHeader.InvoiceId} " +
                    "در دیتابیس خالی یا نامعتبر است؛ ارسال انجام نشد.");
            }

            var invoice = new OmidInvoice
            {
                Header = new OmidInvoiceHeader
                {
                    TaxId = string.Empty,
                    Indatim = FormatOmidIndatim(sourceHeader.Indatim),
                    Inty = sourceHeader.Inty,
                    Inno = inno,
                    Inp = sourceHeader.Inp,
                    Crn = sourceHeader.Crn,
                    Tonw = sourceHeader.Tonw,
                    Setm = sourceHeader.Setm,
                    Tax17 = sourceHeader.Tax17,
                    Ins = sourceHeader.Ins,
                    Tins = tins,
                    Tob = sourceHeader.Tob,
                    Bid = sourceHeader.Bid,
                    Tinb = sourceHeader.Tinb,
                    Sbc = sourceHeader.Sbc,
                    Bpc = sourceHeader.Bpc,
                    Bbc = sourceHeader.Bbc,
                    Tprdis = sourceHeader.Tprdis,
                    Tdis = sourceHeader.Tdis,
                    Tadis = sourceHeader.Tadis,
                    Tvam = sourceHeader.Tvam,
                    Todam = sourceHeader.Todam ?? 0m,
                    Tbill = sourceHeader.Tbill,
                    Tvop = sourceHeader.Tvop,
                    Cap = sourceHeader.Cap,
                    Insp = sourceHeader.Insp?.ToString(
                        CultureInfo.InvariantCulture)
                },

                Body = source.Rows.Select(row =>
                    new OmidInvoiceBodyItem
                    {
                        Sstt = row.sstt,
                        Sstid = row.sstid,
                        Prdis = row.prdis,
                        Dis = row.dis,
                        Adis = row.adis,
                        Vra = row.vra,
                        Vam = row.vam,
                        Tsstam = row.tsstam,
                        Cfee = row.cfee?.ToString(
                            CultureInfo.InvariantCulture),
                        Cut = row.cut,
                        Exr = row.exr?.ToString(
                            CultureInfo.InvariantCulture),
                        Odr = row.odr,
                        Odam = row.odam,
                        Olr = row.olr,
                        Olam = row.olam,
                        Olt = row.olt,
                        Odt = row.odt,
                        Cop = row.cop,
                        Vop = row.vop,
                        Am = row.am,
                        Mu = row.mu,
                        Fee = row.fee,
                        Consfee = row.consfee,
                        Spro = row.spro,
                        Bros = row.bros,
                        Tcpbs = row.tcpbs,
                        Cui = row.cui?.ToString(
                            CultureInfo.InvariantCulture),
                        Bsrn = row.bsrn
                    }).ToList(),

                // پرداخت‌ها را بعد از بررسی منبع واقعی‌شان پر می‌کنیم.
                Payments = new List<OmidPayment>()
            };

            var request = new OmidSendInvoiceRequest
            {
                FiscalId = fiscalId
            };

            request.Invoices.Add(invoice);
            return request;
        }


        /// <summary>
        /// زمان صدور ذخیره‌شده در دیتابیس را از Unix milliseconds
        /// به تاریخ شمسی مورد انتظار نمونهٔ امید تبدیل می‌کند.
        /// </summary>
        private static string FormatOmidIndatim(long? unixMilliseconds)
        {
            if (!unixMilliseconds.HasValue)
                throw new InvalidOperationException(
                    "تاریخ صدور فاکتور (indatim) در دیتابیس خالی است.");

            // مقدار دیتابیس، زمان Unix برحسب میلی‌ثانیه است.
            DateTimeOffset utcTime =
                DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds.Value);

            // تاریخ را بر اساس ساعت ایران محاسبه می‌کنیم.
            DateTimeOffset iranTime =
                utcTime.ToOffset(TimeSpan.FromMinutes(210));

            var persianCalendar = new PersianCalendar();

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0000}/{1:00}/{2:00}",
                persianCalendar.GetYear(iranTime.DateTime),
                persianCalendar.GetMonth(iranTime.DateTime),
                persianCalendar.GetDayOfMonth(iranTime.DateTime));
        }
    }
}
