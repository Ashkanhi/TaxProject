using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI.Omid;
using TaxWebAppCore8.Models.SharedDTO;

namespace TaxWebAppCore8.Controllers
{
    /// <summary>
    /// کنترلر آزمایش اتصال و پیش‌نمایش درخواست امید.
    /// فقط در محیط Development قابل استفاده است.
    /// </summary>
    public class OmidTestController : Controller
    {
        // کلاینت ارتباط HTTP با امید؛ اینجا برای تست ورود استفاده می‌شود.
        private readonly OmidApiClient _client;

        // مشخص می‌کند برنامه در محیط Development اجرا می‌شود یا نه.
        private readonly IWebHostEnvironment _environment;

        public OmidTestController(
            OmidApiClient client,
            IWebHostEnvironment environment)
        {
            _client = client;
            _environment = environment;
        }

        /// <summary>
        /// صفحهٔ تست امید را نشان می‌دهد.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            return View();
        }

        /// <summary>
        /// ورود به امید را تست می‌کند، ولی توکن را در پاسخ نمایش نمی‌دهد.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            try
            {
                await _client.LoginAsync(cancellationToken);

                return Json(new
                {
                    success = true,
                    message = "ورود به امید موفق بود و توکن دریافت شد."
                });
            }
            catch (FormatException)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "قالب Base64 کلید AES یا IV معتبر نیست."
                });
            }
            catch (CryptographicException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "آماده‌سازی رمزنگاری ناموفق بود.",
                    detail = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    success = false,
                    upstreamStatusCode = (int?)ex.StatusCode,
                    message = ex.StatusCode.HasValue
                        ? "سرور امید پاسخ HTTP ناموفق برگرداند."
                        : "ارتباط با سرور امید برقرار نشد."
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, new
                {
                    success = false,
                    message = "درخواست لغو شد یا مهلت ارتباط پایان یافت."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "تنظیمات یا قالب پاسخ امید نیاز به بررسی دارد."
                });
            }
        }

        /// <summary>
        /// فاکتور را از دیتابیس می‌خواند و JSON امید را نمایش می‌دهد.
        /// این متد هیچ فاکتوری به API امید ارسال نمی‌کند.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PreviewInvoice(
            DTOInvoiceLimitParameter request)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            // شناسهٔ فاکتور و فروشگاه برای یافتن فاکتور لازم‌اند.
            if (request == null ||
                !Guid.TryParse(request.InvoiceID, out _) ||
                !request.StoreID.HasValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "InvoiceID معتبر و StoreID لازم است."
                });
            }

            try
            {
                // سرویس از Context دیتابیس و تنظیمات همین مشتری استفاده می‌کند.
                var omidService = new OmidTaxService(
                    BMain._Context,
                    BMain.SrvInfo);

                // خود سرویس MEMORY_ID را به‌عنوان FiscalId می‌خواند.
                var payload = omidService.BuildRequest(request);

                // فقط JSON ساخته‌شده را نمایش می‌دهیم؛ ارسال HTTP نداریم.
                string json = JsonConvert.SerializeObject(
                    payload,
                    Formatting.Indented);

                return Content(
                    json,
                    "application/json; charset=utf-8");
            }
            catch (InvalidOperationException ex)
            {
                // خطاهای قابل‌انتظار، مثل پیدا نشدن فاکتور.
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                // جزئیات خطاهای غیرمنتظره در پاسخ عمومی نمایش داده نمی‌شوند.
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "ساخت پیش‌نمایش ناموفق بود؛ جزئیات خطا را در Debug بررسی کنید."
                });
            }
        }
    }
}