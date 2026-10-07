using System;
using TaxServiceAndApp.Models;
using TaxWebAppCore8.Models.SharedDTO;
using TaxServiceAndApp.Models.SharedDTO;
using System.Threading;
using System.Threading.Tasks;

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
        /// فاکتور را از دیتابیس می‌خواند و درخواست قابل‌نمایش امید را می‌سازد.
        /// این متد هیچ درخواست HTTP ارسال نمی‌کند.
        /// </summary>
        public OmidSendInvoiceRequest BuildRequest(
            DTOInvoiceLimitParameter invoice)
        {
            // برای پیدا کردن فاکتور، شناسهٔ فاکتور و فروشگاه لازم‌اند.
            if (invoice == null ||
                !Guid.TryParse(invoice.InvoiceID, out _) ||
                !invoice.StoreID.HasValue)
            {
                throw new InvalidOperationException(
                    "InvoiceID معتبر و StoreID برای ارسال به امید لازم است.");
            }

            // همان شاخهٔ خواندن دیتابیس که RimaCalculate استفاده می‌کند.
            // نتیجهٔ این شاخه در Header و Rows قرار می‌گیرد.
            var source = _context.GetLastInvoiceForSend(
                invoicetype: 1,
                invoice: invoice,
                isForTis: 2);

            // فاکتور بدون سربرگ یا بدون ردیف قابل تبدیل نیست.
            if (source?.Header == null)
                throw new InvalidOperationException(
                    "سربرگ فاکتور از دیتابیس برنگشت.");

            if (source.Rows == null || source.Rows.Count == 0)
                throw new InvalidOperationException(
                    "ردیف‌های فاکتور از دیتابیس برنگشتند.");

            // بنا بر تصمیم فعلی، MEMORY_ID همین مشتری همان FiscalId امید است.
            string fiscalId = _serverInfo.MEMORY_ID?.Trim();

            if (string.IsNullOrWhiteSpace(fiscalId))
                throw new InvalidOperationException(
                    "MEMORY_ID در CustomerInformation.xml خالی است.");

            // دادهٔ دیتابیس، FiscalId و شناسهٔ اقتصادی فروشنده
            // به مدل JSON موردنیاز امید تبدیل می‌شوند.
            return OmidInvoiceMapper.Map(
                source,
                fiscalId,
                _serverInfo.TINS);
        }




        /// <summary>
        /// یک فاکتور را پس از رزرو در دیتابیس به امید ارسال می‌کند.
        /// Pending به معنی دریافت رسید ارسال است، نه تأیید نهایی صورتحساب.
        /// </summary>
        public async Task<(string LocalInvoiceId, OmidSendReceipt Receipt)>
            SubmitSingleInvoiceAsync(
                DTOInvoiceLimitParameter invoice,
                OmidApiClient client,
                CancellationToken cancellationToken = default)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            // ابتدا فاکتور را بخوان و داده‌ها را بررسی کن؛
            // فاکتور نامعتبر نباید در جدول ارسال رزرو شود.
            var payload = BuildRequest(invoice);
            ValidateForSend(payload);

            if (!Guid.TryParse(invoice.InvoiceID, out Guid localInvoiceId))
                throw new InvalidOperationException("InvoiceID فاکتور معتبر نیست.");

            var store = new OmidSubmissionStore(_serverInfo);

            // کلید اصلی جدول، رزرو هم‌زمان یا ارسال دوبارهٔ همین فاکتور را می‌بندد.
            bool reserved = await store.TryReserveAsync(
                localInvoiceId,
                cancellationToken);

            if (!reserved)
                throw new InvalidOperationException(
                    "این فاکتور قبلاً برای ارسال به امید ثبت شده است؛ " +
                    "قبل از هر اقدام، وضعیت آن را در OmidInvoiceSubmission بررسی کنید.");

            // از این نقطه به بعد، ممکن است درخواست به امید رسیده باشد.
            await store.MarkSendingAsync(localInvoiceId, cancellationToken);

            try
            {
                OmidSendResponse response = await client.SendInvoiceAsync(
                    payload,
                    cancellationToken);

                // HTTP موفق به‌تنهایی کافی نیست؛ رسید معتبر هم لازم است.
                OmidSendReceipt receipt =
                    OmidSendResponseParser.RequireSingleAccepted(response);

                // حتی اگر کاربر درخواست را لغو کرده باشد، رسید دریافت‌شده
                // باید در دیتابیس ثبت شود.
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
                // پس از شروع ارسال، خطا یا قطع ارتباط لزوماً ثابت نمی‌کند
                // که امید فاکتور را دریافت نکرده است؛ ارسال خودکار مجدد ممنوع است.
                try
                {
                    await store.MarkUnknownAsync(
                        localInvoiceId,
                        sendError.Message,
                        CancellationToken.None);
                }
                catch (Exception stateError)
                {
                    // اگر ثبت Unknown هم شکست خورد، هر دو خطا را نگه می‌داریم.
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
        /// درخواست ساخته‌شده را پیش از ارسال واقعی به امید بررسی می‌کند.
        /// این متد اطلاعات را تغییر نمی‌دهد و هیچ درخواست HTTP ارسال نمی‌کند.
        /// </summary>
        public static void ValidateForSend(
            OmidSendInvoiceRequest request)
        {
            // بدون درخواست، چیزی برای بررسی یا ارسال وجود ندارد.
            if (request == null)
                throw new InvalidOperationException(
                    "درخواست ارسال به امید ساخته نشده است.");

            // FiscalId از MEMORY_ID مشتری گرفته می‌شود.
            // طبق اطلاعاتی که داریم باید یک شناسهٔ ۶ کاراکتری باشد.
            if (string.IsNullOrWhiteSpace(request.FiscalId) ||
                request.FiscalId.Trim().Length != 6)
            {
                throw new InvalidOperationException(
                    "FiscalId امید باید شناسهٔ معتبر ۶ کاراکتری حافظهٔ مالیاتی باشد.");
            }

            // درخواست باید دست‌کم شامل یک فاکتور باشد.
            if (request.Invoices == null ||
                request.Invoices.Count == 0)
            {
                throw new InvalidOperationException(
                    "درخواست امید هیچ فاکتوری ندارد.");
            }

            // هر فاکتور موجود در درخواست جداگانه بررسی می‌شود.
            for (int i = 0; i < request.Invoices.Count; i++)
            {
                var invoice = request.Invoices[i];

                // بدون سربرگ، فاکتور قابل ارسال نیست.
                if (invoice?.Header == null)
                    throw new InvalidOperationException(
                        $"سربرگ فاکتور شمارهٔ {i + 1} وجود ندارد.");

                // طبق پاسخ امید، taxId باید در JSON رشتهٔ خالی باشد.
                // OmidInvoiceMapper آن را با string.Empty مقداردهی می‌کند.
                if (invoice.Header.TaxId != string.Empty)
                    throw new InvalidOperationException(
                        $"taxId فاکتور شمارهٔ {i + 1} باید رشتهٔ خالی باشد.");

                // شمارهٔ فاکتور باید از دیتابیس خوانده و به عدد تبدیل شده باشد.
                if (!invoice.Header.Inno.HasValue)
                    throw new InvalidOperationException(
                        $"inno فاکتور شمارهٔ {i + 1} خالی یا نامعتبر است.");

                // زمان صدور فاکتور برای درخواست امید لازم است.
                if (string.IsNullOrWhiteSpace(invoice.Header.Indatim))
                    throw new InvalidOperationException(
                        $"indatim فاکتور شمارهٔ {i + 1} خالی است.");

                // فاکتور باید دست‌کم یک ردیف کالا یا خدمت داشته باشد.
                if (invoice.Body == null ||
                    invoice.Body.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"فاکتور شمارهٔ {i + 1} ردیف ندارد.");
                }
            }
        }
    }
}