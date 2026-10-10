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
    }
}