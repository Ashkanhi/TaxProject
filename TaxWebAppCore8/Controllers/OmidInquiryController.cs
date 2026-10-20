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

                // برای آزمون، استعلام تا آماده‌شدن نتیجه تکرار می‌شود.
                // این کار فاکتور را دوباره ارسال نمی‌کند.
                OmidInquiryRetryResult inquiry = await client.InquiryWithRetryAsync(
                    requestIds, cancellationToken);

                string? savedState = null;
                string? savedMessage = null;
                var itemResults = ReadInquiryItemResults(inquiry.ResponseJson);

                // پاسخ آخرین فراخوانی را برای تک‌تک فاکتورهای همان درخواست
                // ذخیره می‌کنیم. اگر پاسخ، requestId هر نتیجه را داشته باشد،
                // وضعیت اختصاصی همان فاکتور را هم از روی آن تعیین می‌کنیم.
                for (int i = 0; i < invoiceIds.Count; i++)
                {
                    string requestId = requestIds[i];
                    string itemState;
                    string? itemMessage;

                    if (itemResults.TryGetValue(
                        requestId,
                        out (string State, string? Message) itemResult))
                    {
                        itemState = itemResult.State;
                        itemMessage = itemResult.Message;
                    }
                    else if (invoiceIds.Count == 1 &&
                             inquiry.HasFinalResult &&
                             inquiry.FinalSuccess.HasValue)
                    {
                        itemState = inquiry.FinalSuccess.Value
                            ? "Accepted"
                            : "Failed";
                        itemMessage = inquiry.FinalSuccess.Value
                            ? null
                            : inquiry.FinalMessage ?? "امید صورتحساب را نپذیرفت.";
                    }
                    else
                    {
                        itemState = "Pending";
                        itemMessage = $"پس از {inquiry.Attempts} تلاش، نتیجهٔ نهایی هنوز آماده نیست.";
                    }

                    await store.SaveInquiryResultAsync(
                        invoiceIds[i],
                        itemState,
                        itemMessage,
                        inquiry.ResponseJson,
                        cancellationToken);

                    _logger.LogInformation(
                        "استعلام امید برای فاکتور {InvoiceId} پس از {Attempts} تلاش با وضعیت {State} تمام شد.",
                        invoiceIds[i], inquiry.Attempts, itemState);

                    if (invoiceIds.Count == 1)
                    {
                        savedState = itemState;
                        savedMessage = itemMessage;
                        Response.Headers["X-Omid-Inquiry-State"] = itemState;
                        Response.Headers["X-Omid-Inquiry-Message"] =
                            Uri.EscapeDataString(itemMessage ?? string.Empty);
                    }
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

                // JSON نهایی امید بدون تغییر برگردانده می‌شود.
                return Content(
                    inquiry.ResponseJson,
                    "application/json; charset=utf-8");
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
                    message = "استعلام امید ناموفق بود؛ جزئیات را در لاگ سیستم بررسی کنید."
                });
            }
        }

        /// <summary>
        /// وضعیت و پیام هر نتیجه را با requestId متناظر می‌کند.
        /// </summary>
        private static Dictionary<string, (string State, string? Message)>
            ReadInquiryItemResults(string responseJson)
        {
            var result = new Dictionary<string, (string State, string? Message)>(
                StringComparer.OrdinalIgnoreCase);

            try
            {
                JObject root = JObject.Parse(responseJson);
                JToken? data = root["data"];
                IEnumerable<JToken> rows = data is JArray array
                    ? array
                    : data is JObject single
                        ? new[] { single }
                        : Array.Empty<JToken>();

                foreach (JToken rowToken in rows)
                {
                    if (rowToken is not JObject row)
                        continue;

                    string? requestId = row["requestId"]?.Value<string>();
                    if (string.IsNullOrWhiteSpace(requestId))
                        continue;

                    string? invoiceStatus = row["invoiceStatus"]?.Value<string>();
                    JToken? resultData = row["inquiryResultModel"]?["data"]
                        ?? row["data"];
                    bool? success = resultData?["success"]?.Value<bool>();

                    string state = string.Equals(
                        invoiceStatus, "ACCEPTED", StringComparison.OrdinalIgnoreCase) ||
                        success == true
                            ? "Accepted"
                            : string.Equals(
                                invoiceStatus, "REJECTED", StringComparison.OrdinalIgnoreCase) ||
                              success == false
                                ? "Failed"
                                : "Pending";

                    string? message = resultData?["error"]?.ToString();
                    if (string.IsNullOrWhiteSpace(message))
                        message = row["errors"]?.ToString();
                    if (string.IsNullOrWhiteSpace(message))
                        message = row["message"]?.ToString();
                    if (string.IsNullOrWhiteSpace(message) && state == "Failed")
                        message = "امید صورتحساب را نپذیرفت.";
                    if (state == "Accepted")
                        message = null;

                    result[requestId.Trim()] = (state, message);
                }
            }
            catch
            {
                // پاسخ ناسازگار با JSON، وضعیت اختصاصی ندارد؛ چرخه در Pending می‌ماند.
            }

            return result;
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
