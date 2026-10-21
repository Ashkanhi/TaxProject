using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaxServiceAndApp.BI;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TaxWebAppCore8.BI.Omid;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using Newtonsoft.Json.Linq;

namespace TaxWebAppCore8.Controllers
{
    public class OmidInquiryController : Controller
    {
        private readonly ILogger<OmidInquiryController> _logger;

        public OmidInquiryController(
            ILogger<OmidInquiryController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// صفحهٔ مستقل استعلام فاکتورهای امید را باز می‌کند.
        /// بارگذاری فهرست و اجرای استعلام با دکمهٔ صفحه انجام می‌شود.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            return View();
        }

        /// <summary>فروشگاه‌های همان دیتابیس فاکتورها را برای فیلتر صفحه می‌خواند.</summary>
        [HttpGet]
        public IActionResult GetStores()
        {
            if (!string.Equals(BMain.SrvInfo?.ServiceType?.Trim(),
                    "Omid", StringComparison.OrdinalIgnoreCase))
                return NotFound();

            try
            {
                return Json(new { success = true, data = BMain.GetStore() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در خواندن فروشگاه‌های استعلام امید");
                return StatusCode(500, new
                {
                    success = false,
                    message = "خواندن فروشگاه‌ها ناموفق بود."
                });
            }
        }

        /// <summary>اتصال امید را با ورود آزمایش می‌کند؛ توکن را برنمی‌گرداند.</summary>
        [HttpGet]
        public async Task<IActionResult> CheckConnection(
            CancellationToken cancellationToken)
        {
            if (!string.Equals(BMain.SrvInfo?.ServiceType?.Trim(),
                    "Omid", StringComparison.OrdinalIgnoreCase))
                return NotFound();

            try
            {
                var client = HttpContext.RequestServices
                    .GetRequiredService<OmidApiClient>();
                string token = await client.LoginAsync(cancellationToken);
                return Json(new
                {
                    success = !string.IsNullOrWhiteSpace(token),
                    message = !string.IsNullOrWhiteSpace(token)
                        ? "ارتباط و ورود به امید موفق بود."
                        : "امید توکن ورود برنگرداند."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "تست ارتباط استعلام امید ناموفق بود");
                return StatusCode(502, new
                {
                    success = false,
                    message = "ارتباط با امید ناموفق بود؛ جزئیات در لاگ امید ثبت شد."
                });
            }
        }


        /// <summary>
        /// یک تا صد فاکتور انتخاب‌شده را با شناسه‌های پیگیری ذخیره‌شده
        /// در یک درخواست از امید استعلام می‌گیرد.
        /// هیچ فاکتوری در این مسیر ارسال یا دوباره ارسال نمی‌شود.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InquirySelected(
            [FromBody] List<Guid> invoiceIds,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            if (invoiceIds == null || invoiceIds.Count < 1 || invoiceIds.Count > 100)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بین ۱ تا ۱۰۰ فاکتور برای استعلام انتخاب کنید."
                });
            }

            var savedResults = new Dictionary<Guid, object>();
            try
            {
                var store = new OmidSubmissionStore(BMain.SrvInfo);
                var seenInvoiceIds = new HashSet<Guid>();
                var requestIds = new List<string>();

                foreach (Guid invoiceId in invoiceIds)
                {
                    if (invoiceId == Guid.Empty ||
                        !seenInvoiceIds.Add(invoiceId))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "شناسهٔ فاکتور خالی یا تکراری است."
                        });
                    }

                    // فقط رکورد ارسالِ ثبت‌شده در دیتابیس محلی معتبر است.
                    var submission = await store.GetAsync(
                        invoiceId, cancellationToken);

                    if (submission == null ||
                        string.IsNullOrWhiteSpace(submission.RequestId) ||
                        submission.State == "Prepared" ||
                        submission.State == "Sending")
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"فاکتور {invoiceId} هنوز requestId معتبر برای استعلام ندارد."
                        });
                    }

                    requestIds.Add(submission.RequestId.Trim());
                }

                var client = HttpContext.RequestServices
                    .GetRequiredService<OmidApiClient>();

                var invoiceByRequestId = requestIds.Select((id, index) => new
                {
                    Id = Guid.Parse(id).ToString(), InvoiceId = invoiceIds[index]
                }).ToDictionary(item => item.Id, item => item.InvoiceId, StringComparer.OrdinalIgnoreCase);
                string? savedState = null;
                string? savedMessage = null;

                OmidInquiryRetryResult inquiry = await client.InquiryWithRetryAsync(
                    requestIds, cancellationToken, async (items, attempt) =>
                    {
                        foreach (var item in items.Values)
                        {
                            Guid invoiceId = invoiceByRequestId[item.RequestId];
                            // رسید قدیمی اجازه ندارد وضعیت ارسال جدید همان فاکتور را تغییر دهد.
                            var saved = await store.SaveInquiryResultAsync(
                                invoiceId, item.RequestId, item.State, item.Message,
                                item.ResponseJson, CancellationToken.None);
                            string? state = saved?.State;
                            string? message = saved.HasValue ? saved.Value.Message
                                : "شناسهٔ ارسال این فاکتور تغییر کرده؛ پاسخ قدیمی روی آن اعمال نشد.";
                            savedResults[invoiceId] = new
                            {
                                invoiceId, state, message, attempts = attempt,
                                stale = !saved.HasValue, responseJson = item.ResponseJson
                            };
                            if (invoiceIds.Count == 1)
                            {
                                savedState = state;
                                savedMessage = message;
                            }
                            _logger.LogInformation(
                                "استعلام امید: فاکتور {InvoiceId}، تلاش {Attempt}، وضعیت ذخیره‌شده {State}",
                                invoiceId, attempt, state ?? "Stale");
                        }
                    });

                if (invoiceIds.Count == 1)
                {
                    Response.Headers["X-Omid-Inquiry-State"] = savedState ?? string.Empty;
                    Response.Headers["X-Omid-Inquiry-Message"] = Uri.EscapeDataString(savedMessage ?? string.Empty);
                }

                // تعداد تلاش‌ها را برای نمایش در پنل در هدر پاسخ می‌گذاریم.
                Response.Headers["X-Omid-Inquiry-Attempts"] =
                    inquiry.Attempts.ToString();
                Response.Headers["X-Omid-Inquiry-Final"] =
                    inquiry.HasFinalResult ? "true" : "false";

                OmidApiTraceLogger.TryWriteInquirySummary(
                    invoiceIds,
                    inquiry.Attempts,
                    savedState,
                    savedMessage,
                    inquiry.ResponseJson);

                // نتیجهٔ تجمیعی و وضعیت واقعی ذخیره‌شدهٔ هر فاکتور برای نمایش در جدول.
                JObject panelResponse = JObject.Parse(inquiry.ResponseJson);
                panelResponse["savedResults"] = JArray.FromObject(savedResults.Values);
                return Content(panelResponse.ToString(), "application/json; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در استعلام فاکتورهای انتخاب‌شده از امید");

                // خطاهای زمان استعلام، مانند سایر خطاهای امید، سابقه‌دار می‌شوند.
                try
                {
                    var errorStore = new OmidSubmissionStore(BMain.SrvInfo);
                    foreach (Guid invoiceId in invoiceIds.Distinct())
                    {
                        await errorStore.LogAttemptErrorAsync(
                            invoiceId,
                            "Inquiry",
                            "/api/Invoice/Invoiceinquiry",
                            ex is System.Net.Http.HttpRequestException httpError &&
                            httpError.StatusCode.HasValue
                                ? (int)httpError.StatusCode.Value
                                : null,
                            "Error",
                            ex.Message,
                            CancellationToken.None);
                    }
                }
                catch (Exception logError)
                {
                    _logger.LogWarning(
                        logError,
                        "ثبت سابقهٔ خطای استعلام امید در جدول انجام نشد");
                }

                return StatusCode(502, new
                {
                    success = false,
                    message = "استعلام امید ناموفق بود؛ نتایج دریافت‌شده تا این لحظه ذخیره شده‌اند. جزئیات در لاگ سیستم است.",
                    savedResults = savedResults.Values
                });
            }
        }

        /// <summary>
        /// فاکتورهای دارای شناسهٔ پیگیری امید را می‌خواند. اگر بازهٔ شمسی
        /// داده شود، تاریخ فاکتور را با همان فهرست دیتابیس اصلی فیلتر می‌کند.
        /// این متد درخواستی به سرویس امید نمی‌فرستد.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTrackable(
            string? fromDate,
            string? toDate,
            CancellationToken cancellationToken)
        {
            // این مسیر فقط برای مشتریِ تنظیم‌شده روی سرویس امید فعال است.
            if (!string.Equals(
                BMain.SrvInfo?.ServiceType?.Trim(),
                "Omid",
                StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            bool hasFrom = !string.IsNullOrWhiteSpace(fromDate);
            bool hasTo = !string.IsNullOrWhiteSpace(toDate);

            if (hasFrom != hasTo)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "هر دو تاریخ شروع و پایان را وارد کنید."
                });
            }

            try
            {
                var store = new OmidSubmissionStore(BMain.SrvInfo);
                var invoices = await store.GetTrackableAsync(
                    cancellationToken);

                // تاریخ و اطلاعات نمایشی از همان فهرست دیتابیس اصلی خوانده
                // می‌شوند. CreatedUtc جدول امید، تاریخ ارسال است، نه تاریخ فاکتور.
                var headersById = new Dictionary<Guid,
                    GeneralInvoiceHeaderViewModel>();

                DateTime? start = null;
                DateTime? end = null;

                if (hasFrom)
                {
                    try
                    {
                        start = MD.PersianDateTime.PersianDateTime
                            .Parse(fromDate!.Trim()).ToDateTime().Date;
                        end = MD.PersianDateTime.PersianDateTime
                            .Parse(toDate!.Trim()).ToDateTime().Date;
                    }
                    catch (Exception)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "قالب تاریخ شروع یا پایان معتبر نیست."
                        });
                    }

                    if (start!.Value > end!.Value)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "تاریخ شروع نباید بعد از تاریخ پایان باشد."
                        });
                    }

                }

                // بدون بازه نیز فهرست اصلی را برای ستون‌های مشابه فرم قبلی
                // می‌خوانیم؛ فقط رکوردهای دارای رسید امید در خروجی می‌مانند.
                var headers = await BMain.GetAllInvoiceAsync(
                    new SearchParameters { DateFrom = start, DateTo = end });

                foreach (var header in headers)
                {
                    if (Guid.TryParse(header.InvoiceId, out Guid id))
                        headersById[id] = header;
                }

                var result = new List<object>();

                foreach (var invoice in invoices)
                {
                    bool found = headersById.TryGetValue(
                        invoice.InvoiceId,
                        out GeneralInvoiceHeaderViewModel? header);

                    if (hasFrom && !found)
                        continue;

                    result.Add(new
                    {
                        invoice.InvoiceId,
                        invoice.State,
                        invoice.RequestId,
                        invoice.TrackingId,
                        invoice.InvoiceNumber,
                        InvoiceDate = header?.PersianCreateDate,
                        StoreId = header?.StoreID,
                        StoreName = header?.StoreName,
                        CustomerName = header?.CustomerName,
                        InvoiceDescription = header?.InvoiceDescription,
                        NetPrice = header?.NetPrice,
                        TotalTax = header?.TotalTax,
                        invoice.Fingerprint,
                        invoice.LastError,
                        invoice.LastErrorStage,
                        invoice.LastInquiryResponseJson,
                        invoice.LastInquiryUtc,
                        invoice.CreatedUtc,
                        invoice.UpdatedUtc
                    });
                }

                return Json(new
                {
                    success = true,
                    count = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "خطا در خواندن فاکتورهای قابل‌استعلام امید");

                return StatusCode(500, new
                {
                    success = false,
                    message = "خواندن فاکتورهای قابل‌استعلام ناموفق بود."
                });
            }
        }
    }
}
