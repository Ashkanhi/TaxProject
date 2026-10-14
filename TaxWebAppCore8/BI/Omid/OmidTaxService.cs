using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
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
                    // فقط یک رسید معتبر با trackingId قابل ثبت است.
                    if (!receiptsByInno.TryGetValue(
                            item.Inno,
                            out List<OmidSendReceipt>? matches) ||
                        matches.Count != 1 ||
                        string.IsNullOrWhiteSpace(matches[0].TrackingId))
                    {
                        continue;
                    }

                    OmidSendReceipt receipt = matches[0];

                    await store.MarkPendingAsync(
                        item.InvoiceId,
                        receipt,
                        CancellationToken.None);

                    item.State = "Pending";
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
                // بعد از شروع ارسال، قطع ارتباط لزوماً به معنی
                // نرسیدن درخواست به امید نیست.
                var stateErrors = new List<Exception>();

                foreach (var item in result.Items)
                {
                    if (!withoutStoredReceipt.Contains(item.InvoiceId))
                        continue;

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

                if (!invoice.Header.Inno.HasValue)
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
        public string? TrackingId { get; set; }
        public string? Message { get; set; }
    }
}