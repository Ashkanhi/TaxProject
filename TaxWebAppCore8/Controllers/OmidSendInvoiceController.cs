using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models.SharedDTO;
using TaxWebAppCore8.BI.Omid;
using TaxWebAppCore8.Models.SharedDTO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TaxWebAppCore8.Controllers
{
    public class OmidSendInvoiceController : Controller
    {
        private readonly ILogger<OmidSendInvoiceController> _logger;

        public OmidSendInvoiceController(
            ILogger<OmidSendInvoiceController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// فقط برای تست در محیط Development.
        /// درخواست چندفاکتوری را می‌سازد و خلاصه‌اش را نشان می‌دهد؛
        /// هیچ فاکتوری ذخیره یا به امید ارسال نمی‌شود.
        /// </summary>
        [HttpPost]
        public IActionResult PreviewSelectedInvoices(
            [FromBody] List<DTOInvoiceLimitParameter> invoices,
            [FromServices] IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
                return NotFound();

            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            if (invoices == null || invoices.Count < 2 || invoices.Count > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "برای تست، بین ۲ تا ۱۰۰ فاکتور لازم است."
                });
            }

            try
            {
                var service = BMain._omidService
                    ?? throw new InvalidOperationException(
                        "سرویس امید آماده نشده است.");

                // این متد فقط از دیتابیس می‌خواند و درخواست را در حافظه می‌سازد.
                OmidSendInvoiceRequest request =
                    service.BuildBatchRequest(invoices);

                var summary = new List<object>();

                foreach (var item in request.Invoices)
                {
                    summary.Add(new
                    {
                        inno = item.Header.Inno,
                        indatim = item.Header.Indatim,
                        rowCount = item.Body.Count
                    });
                }

                return Json(new
                {
                    success = true,
                    fiscalId = request.FiscalId,
                    count = request.Invoices.Count,
                    invoices = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در پیش‌نمایش چند فاکتور امید");

                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// یک تا صد فاکتور انتخاب‌شده را به امید ارسال می‌کند.
        /// نتیجهٔ Pending به معنی پذیرش درخواست است، نه تأیید نهایی فاکتور.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendSelectedInvoices(
            [FromBody] List<DTOInvoiceLimitParameter> invoices,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            // خطای تبدیل JSON (مثلاً BookerWorkstationID خالی برای مدل عددی)
            // نباید به‌اشتباه به کاربر به‌صورت خطای «تعداد فاکتورها» نمایش داده شود.
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState
                    .SelectMany(entry => entry.Value!.Errors.Select(error =>
                        $"{entry.Key}: {error.ErrorMessage}"))
                    .ToList();

                string errorDetails = string.Join(" | ", modelErrors);
                _logger.LogWarning(
                    "ورودی ارسال امید به مدل تبدیل نشد: {ModelErrors}",
                    errorDetails);

                // این خطا قبل از تماس با API رخ می‌دهد؛ آن را در لاگ سیستم هم نگه می‌داریم.
                ClsTools.Logger.NPSLogger
                    .GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)?
                    .LogError("Omid SendSelectedInvoices ModelState: " + errorDetails);

                bool missingWorkstation = modelErrors.Any(error =>
                    error.Contains("BookerWorkstationID",
                        StringComparison.OrdinalIgnoreCase));

                return BadRequest(new
                {
                    success = false,
                    pendingCount = 0,
                    message = missingWorkstation
                        ? "BookerWorkstationID یکی از فاکتورهای انتخاب‌شده خالی یا نامعتبر است؛ هیچ ارسالی انجام نشد."
                        : "اطلاعات یکی از فاکتورهای انتخاب‌شده قابل خواندن نیست؛ هیچ ارسالی انجام نشد."
                });
            }

            if (invoices == null || invoices.Count < 1 || invoices.Count > 100)
            {
                return Json(new
                {
                    success = false,
                    pendingCount = 0,
                    message = "در هر درخواست باید بین ۱ تا ۱۰۰ فاکتور باشد."
                });
            }

            try
            {
                var client = HttpContext.RequestServices
                    .GetRequiredService<OmidApiClient>();

                var service = BMain._omidService
                    ?? throw new InvalidOperationException("سرویس امید آماده نشده است.");

                // یک فاکتور از همان مسیر تکیِ موجود ارسال می‌شود.
                if (invoices.Count == 1)
                {
                    var sent = await service.SubmitSingleInvoiceAsync(
                        invoices[0], client, cancellationToken);

                    return Json(new
                    {
                        success = true,
                        pendingCount = 1,
                        message = "درخواست یک فاکتور پذیرفته شد؛ نتیجهٔ نهایی باید استعلام شود.",
                        items = new[]
                        {
                    new
                    {
                        invoiceId = sent.LocalInvoiceId,
                        state = "Pending",
                        requestId = sent.Receipt.RequestId,
                        trackingId = sent.Receipt.TrackingId
                    }
                }
                    });
                }

                // چند فاکتور در یک درخواست به امید ارسال می‌شوند.
                var result = await service.SubmitBatchInvoicesAsync(
                    invoices, client, cancellationToken);

                int pendingCount = 0;
                int unknownCount = 0;
                var items = new List<object>();

                foreach (var item in result.Items)
                {
                    if (item.State == "Pending")
                        pendingCount++;
                    else
                        unknownCount++;

                    items.Add(new
                    {
                        invoiceId = item.InvoiceId,
                        inno = item.Inno,
                        state = item.State,
                        requestId = item.RequestId,
                        trackingId = item.TrackingId,
                        message = item.Message
                    });
                }

                return Json(new
                {
                    success = unknownCount == 0,
                    pendingCount,
                    unknownCount,
                    message = unknownCount == 0
                        ? $"درخواست {pendingCount} فاکتور پذیرفته شد؛ نتیجهٔ نهایی باید استعلام شود."
                        : $"{pendingCount} فاکتور Pending و {unknownCount} فاکتور Unknown است. فاکتور Unknown را دوباره ارسال نکنید.",
                    items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال فاکتورهای انتخاب‌شده به امید");

                return Json(new
                {
                    success = false,
                    pendingCount = 0,
                    message = ex is InvalidOperationException || ex is AggregateException
                        ? ex.Message
                        : "نتیجهٔ ارسال مشخص نیست. پیش از تلاش دوباره، وضعیت فاکتورها را بررسی کنید."
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SendInvoice(
            DTOInvoiceLimitParameter request,
            CancellationToken cancellationToken)
        {
            // این مسیر فقط برای مشتریِ تنظیم‌شده روی امید است.
            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            try
            {
                if (request == null)
                    throw new InvalidOperationException(
                        "اطلاعات فاکتور ارسال نشده است.");

                // کلاینت امید فقط در همین مسیر و پس از بررسی ServiceType ساخته می‌شود.
                var client = HttpContext.RequestServices
                    .GetRequiredService<OmidApiClient>();

                var service = BMain._omidService
                    ?? throw new InvalidOperationException(
                        "سرویس امید آماده نشده است.");

                // این متد رزرو، ارسال و ثبت trackingId را انجام می‌دهد.
                var sent = await service.SubmitSingleInvoiceAsync(
                    request,
                    client,
                    cancellationToken);

                return Json(new
                {
                    success = true,
                    Data = new ResultModel
                    {
                        Success = true,
                        Status = "Pending",
                        Message = "امید درخواست را پذیرفت؛ نتیجهٔ نهایی باید استعلام شود.",
                        Data = new
                        {
                            sent.LocalInvoiceId,
                            sent.Receipt.RequestId,
                            sent.Receipt.TrackingId,
                            sent.Receipt.InvoiceNumber,
                            sent.Receipt.Fingerprint
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در ارسال فاکتور {InvoiceId} به امید",
                    request?.InvoiceID);

                // در وضعیت نامشخص، کاربر نباید بدون بررسی دوباره ارسال کند.
                string message =
                    ex is InvalidOperationException ||
                    ex is AggregateException
                        ? ex.Message
                        : "نتیجهٔ ارسال مشخص نیست. پیش از تلاش دوباره وضعیت فاکتور را بررسی کنید.";

                return Json(new
                {
                    success = false,
                    Data = new ResultModel
                    {
                        Success = false,
                        Status = "Error",
                        Message = message
                    }
                });
            }
        }

        /// <summary>
        /// وضعیت ثبت‌شدهٔ فاکتور را فقط از دیتابیس محلی می‌خواند.
        /// این متد فاکتور را به امید ارسال نمی‌کند.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatus(
            string invoiceId,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            if (!Guid.TryParse(invoiceId, out Guid id) || id == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "InvoiceId معتبر نیست."
                });
            }

            try
            {
                var store = new OmidSubmissionStore(BMain.SrvInfo);
                var status = await store.GetAsync(id, cancellationToken);

                if (status == null)
                {
                    return Json(new
                    {
                        success = true,
                        found = false,
                        message = "برای این فاکتور هنوز رکورد ارسال امید وجود ندارد."
                    });
                }

                return Json(new
                {
                    success = true,
                    found = true,
                    Data = new
                    {
                        status.InvoiceId,
                        status.State,
                        status.RequestId,
                        status.TrackingId,
                        status.InvoiceNumber,
                        status.Fingerprint,
                        status.CreatedUtc,
                        status.UpdatedUtc
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در خواندن وضعیت امید برای فاکتور {InvoiceId}",
                    invoiceId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "خواندن وضعیت محلی فاکتور ناموفق بود."
                });
            }
        }


        /// <summary>
        /// نتیجهٔ فاکتور را از امید می‌پرسد؛ فاکتور جدیدی ارسال نمی‌کند.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Inquiry(
            string invoiceId,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            if (!Guid.TryParse(invoiceId, out Guid id) || id == Guid.Empty)
                return BadRequest("InvoiceId معتبر نیست.");

            try
            {
                // رکورد ارسال همین فاکتور را از دیتابیس محلی می‌خوانیم.
                var store = new OmidSubmissionStore(BMain.SrvInfo);
                var submission = await store.GetAsync(id, cancellationToken);

                if (submission == null)
                    return NotFound("رکورد ارسال امید برای این فاکتور پیدا نشد.");

                if (submission.State != "Pending" ||
                    string.IsNullOrWhiteSpace(submission.RequestId))
                {
                    return BadRequest(
                        "این فاکتور در وضعیت Pending با requestId معتبر نیست.");
                }

                // با requestId ذخیره‌شده، نتیجه را از امید می‌گیریم.
                var client = HttpContext.RequestServices
                    .GetRequiredService<OmidApiClient>();

                string responseJson = await client.InquiryAsync(
                    submission.RequestId,
                    cancellationToken);

                // فعلاً پاسخ واقعی امید را بدون تغییر نمایش می‌دهیم.
                return Content(responseJson, "application/json; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در استعلام فاکتور {InvoiceId} از امید",
                    invoiceId);

                return StatusCode(
                    502,
                    "استعلام ناموفق بود؛ جزئیات را در لاگ امید بررسی کنید.");
            }
        }
    }
}
