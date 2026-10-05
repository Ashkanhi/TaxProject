using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI.Omid;
using TaxWebAppCore8.Models.SharedDTO;

namespace TaxWebAppCore8.Controllers
{
    public class OmidTestController : Controller
    {
        private readonly OmidApiClient _client;
        private readonly IWebHostEnvironment _environment;
        private readonly OmidConfig _config;

        public OmidTestController(
            OmidApiClient client,
            IWebHostEnvironment environment,
            IOptions<OmidConfig> options)
        {
            _client = client;
            _environment = environment;
            _config = options.Value;
        }

        // صفحه تست فقط در محیط Development در دسترس است.
        [HttpGet]
        public IActionResult Index()
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            return View();
        }

        // ورود به امید را آزمایش می‌کند؛ توکن را نمایش نمی‌دهد.
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

        // فاکتور را از دیتابیس می‌خواند و JSON امید را می‌سازد.
        // این متد هیچ درخواستی به API امید ارسال نمی‌کند.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PreviewInvoice(
            DTOInvoiceLimitParameter request)
        {
            if (!_environment.IsDevelopment())
                return NotFound();

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
                // همان شاخه‌ای که RimaCalculate استفاده می‌کند:
                // خروجی آن در Header و Rows قرار می‌گیرد.
                var source = BMain._Context.GetLastInvoiceForSend(
                    (short)1,
                    request,
                    isForTis: 2);

                if (source?.Header == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "سربرگ فاکتور از دیتابیس برنگشت. " +
                            "InvoiceID، StoreID و BookerWorkstationID را بررسی کنید."
                    });
                }

                if (source.Rows == null || source.Rows.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ردیف‌های فاکتور از دیتابیس برنگشت."
                    });
                }

                // تبدیل Header و Rows دیتابیس به مدل درخواست امید.
                string fiscalIdForPreview =
                    string.IsNullOrWhiteSpace(_config.FiscalId)
                        ? "__FISCAL_ID_PREVIEW_ONLY__"
                        : _config.FiscalId;

                var payload = OmidInvoiceMapper.Map(
                    source,
                    fiscalIdForPreview,
                    BMain.SrvInfo.TINS);

                // نمایش JSON ساخته‌شده؛ هیچ ارسالی انجام نمی‌شود.
                string json = JsonConvert.SerializeObject(
                    payload,
                    Formatting.Indented);

                return Content(
                    json,
                    "application/json; charset=utf-8");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
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