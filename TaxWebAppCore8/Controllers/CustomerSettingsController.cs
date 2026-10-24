using Microsoft.AspNetCore.Mvc;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI;
using TaxWebAppCore8.BI.Omid;
using Microsoft.Extensions.Options;

namespace TaxWebAppCore8.Controllers;

[SystemToolsPassword("customer")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class CustomerSettingsController : Controller
{
    private CustomerSettingsStore Store => new(BMain.ServerResourcesPath);

    [HttpGet]
    public IActionResult Index()
    {
        try { return View(Store.Read(includeSecrets: true)); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            ViewData["Unavailable"] = true;
            ModelState.AddModelError("", "خواندن فایل تنظیمات ممکن نیست؛ مسیر فایل و دسترسی سرور بررسی شود.");
            return View(new CustomerSettingsForm());
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Index(CustomerSettingsForm form)
    {
        List<string> errors;
        try
        {
            errors = ModelState.IsValid ? Store.Save(form) : ["اطلاعات فرم معتبر نیست."];
            if (errors.Count == 0)
            {
                TempData["CustomerSettingsSaved"] = "تنظیمات ذخیره شد. برای اعمال تغییرات، برنامه را راه‌اندازی مجدد کنید.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or System.Xml.XmlException)
        { errors = ["ذخیره تنظیمات ممکن نشد؛ فایل و دسترسی نوشتن روی پوشه تنظیمات بررسی شود."]; }
        // Values are only displayed on this password-protected, non-cacheable page.
        ModelState.Clear();
        foreach (var error in errors) ModelState.AddModelError("", error);
        return View(form);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [ActionName("TestOmid")]
    public async Task<IActionResult> TestOmidAsync(CustomerSettingsForm form, [FromServices] IHttpClientFactory clients, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || form.Values.GetValueOrDefault("ServiceType") != "Omid")
            return BadRequest(new { Success = false, Message = "برای تست ارتباط امید، سرویس امید را انتخاب کنید و اطلاعات آن را وارد کنید." });
        try
        {
            var saved = Store.Read(includeSecrets: true);
            if (saved.Version != form.Version)
                return Conflict(new { Success = false, Message = "تنظیمات تغییر کرده است؛ صفحه را دوباره باز کنید." });
            var config = OmidSettingsConnectionTest.BuildConfig(form.Values, saved.Values);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(20));
            using var http = clients.CreateClient(OmidSettingsConnectionTest.ClientName);
            var client = new OmidApiClient(http, Options.Create(config));
            await client.LoginAsync(deadline.Token);
            return Json(new { Success = true, Message = "ارتباط و ورود به امید با مقادیر فعلی فرم موفق بود. برای استفاده در ارسال، تغییرات را ذخیره و برنامه را مجدد اجرا کنید." });
        }
        catch (OmidSettingsValidationException ex)
        { return BadRequest(new { Success = false, Message = ex.Message }); }
        catch (OmidApiException ex)
        {
            var message = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden => "امید درخواست ورود را نپذیرفت؛ کلیدها، نام کاربری و رمز عبور را بررسی کنید.",
                System.Net.HttpStatusCode.TooManyRequests => "تعداد درخواست‌ها به امید زیاد است؛ کمی بعد دوباره تست کنید.",
                System.Net.HttpStatusCode.NotFound => "مسیر ورود به امید پیدا نشد؛ آدرس سرویس را بررسی کنید.",
                _ => "امید پاسخ ناموفق داد؛ اطلاعات ورود و کلیدها را بررسی کنید یا کمی بعد دوباره تلاش کنید."
            };
            return Json(new { Success = false, Message = message });
        }
        catch (HttpRequestException)
        { return Json(new { Success = false, Message = "ارتباط با امید برقرار نشد؛ آدرس سرویس، اتصال شبکه و گواهی سرور را بررسی کنید." }); }
        catch (OperationCanceledException)
        { return Json(new { Success = false, Message = "مهلت تست ارتباط با امید پایان یافت؛ دوباره تلاش کنید." }); }
        catch (Exception ex) when (ex is Newtonsoft.Json.JsonException or InvalidOperationException or System.Security.Cryptography.CryptographicException or ArgumentException or FormatException)
        { return Json(new { Success = false, Message = "ورود به امید تأیید نشد؛ پاسخ معتبر یا توکن ورود دریافت نشد. تنظیمات و کلیدهای امید را بررسی کنید." }); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or System.Xml.XmlException)
        { return Json(new { Success = false, Message = "خواندن تنظیمات ذخیره‌شده ممکن نیست؛ صفحه را دوباره باز کنید." }); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TestDatabase(CustomerSettingsForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(new { Success = false, Message = "اطلاعات اتصال معتبر نیست." });
        try
        {
            var saved = Store.Read(includeSecrets: true);
            if (saved.Version != form.Version)
                return Conflict(new { Success = false, Message = "تنظیمات تغییر کرده است؛ صفحه را دوباره باز کنید." });
            var connection = SettingsDatabaseConnection.Build(form.Values, saved.Values.GetValueOrDefault("UserPass", ""));
            await SettingsDatabaseConnection.TestAsync(connection, cancellationToken);
            return Json(new { Success = true, Message = "اتصال به دیتابیس و اجرای پرس‌وجوی آزمایشی موفق بود." });
        }
        catch (ArgumentException)
        { return BadRequest(new { Success = false, Message = "آدرس سرور، نام دیتابیس، نام کاربری و مهلت اتصال (۱ تا ۳۶۰۰ ثانیه) را بررسی کنید." }); }
        catch (System.Data.SqlClient.SqlException ex)
        {
            var message = ex.Number switch
            {
                18456 => "ورود به دیتابیس ناموفق بود؛ نام کاربری و رمز عبور را بررسی کنید.",
                4060 => "دیتابیس در دسترس نیست؛ نام دیتابیس و مجوز کاربر را بررسی کنید.",
                _ => "اتصال برقرار نشد؛ آدرس سرور، دسترسی شبکه و اطلاعات ورود را بررسی کنید."
            };
            return Json(new { Success = false, Message = message });
        }
        catch (OperationCanceledException)
        { return Json(new { Success = false, Message = "مهلت تست اتصال پایان یافت؛ دسترسی سرور را بررسی کنید." }); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or System.Xml.XmlException)
        { return Json(new { Success = false, Message = "خواندن تنظیمات اتصال ممکن نیست." }); }
    }
}
