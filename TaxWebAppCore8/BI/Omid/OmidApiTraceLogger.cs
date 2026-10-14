using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaxWebAppCore8.BI.Omid
{
    /// <summary>
    /// درخواست و پاسخ هر فراخوانی API امید را در فایل لاگ جداگانه ثبت می‌کند.
    /// فایل‌های این کلاس در صفحهٔ «لاگ‌های سیستم» قابل مشاهده‌اند.
    /// </summary>
    internal static class OmidApiTraceLogger
    {
        // اگر چند درخواست هم‌زمان تمام شوند، متن رخدادهایشان
        // هنگام نوشتن در فایل با یکدیگر مخلوط نشود.
        private static readonly object FileLock = new();

        /// <summary>
        /// یک رخداد شامل نام تابع، درخواست و پاسخ همان فراخوانی ثبت می‌کند.
        /// خطا در نوشتن لاگ نباید نتیجهٔ ارسال فاکتور را تغییر دهد.
        /// </summary>
        public static void TryWrite(
            string servicePath,
            string? payloadJson,
            string? wireJson,
            int? httpStatusCode,
            string? responseJson,
            Exception? error)
        {
            try
            {
                DateTime now = DateTime.Now;

                // همان پوشه‌ای که صفحهٔ «لاگ‌های سیستم» از آن فایل‌ها را می‌خواند.
                string directory = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonApplicationData),
                    "ArghamFinanceCompany",
                    "Logs");

                Directory.CreateDirectory(directory);

                // لاگ امید از لاگ عمومی سیستم جداست.
                // برای جلوگیری از بزرگ‌شدن یک فایل، هر روز فایل خودش را دارد.
                string filePath = Path.Combine(
                    directory,
                    $"OmidApiTrace_{now:yyyyMMdd}.txt");

                // نام تابعی که این مسیر API را فراخوانی می‌کند.
                string functionName = servicePath switch
                {
                    "/api/Auth/login" => nameof(OmidApiClient.LoginAsync),

                    "/api/Invoice/SendInvoice" =>
                        nameof(OmidApiClient.SendInvoiceAsync),

                    "/api/Invoice/Invoiceinquiry" =>
                        nameof(OmidApiClient.InquiryAsync),

                    _ => servicePath
                };

                var entry = new StringBuilder();

                // قالب تاریخ با قالب مورد انتظار صفحهٔ لاگ یکسان است.
                // درخواست و پاسخ همین فراخوانی در یک رخداد قرار می‌گیرند.
                entry.Append(now.ToString(
                        "yyyy-MM-dd HH:mm:ss,fff",
                        CultureInfo.InvariantCulture))
                    .Append(" | تابع: ")
                    .Append(functionName)
                    .Append(" | POST ")
                    .AppendLine(servicePath);

                entry.Append("کد HTTP: ")
                    .AppendLine(
                        httpStatusCode?.ToString(
                            CultureInfo.InvariantCulture)
                        ?? "پاسخ HTTP دریافت نشد");

                // این JSON خواناست و برای پیدا کردن خطاهای فیلدهایی
                // مثل header.todam استفاده می‌شود.
                entry.AppendLine(
                        "JSON درخواست پیش از رمزنگاری:")
                    .AppendLine(SanitizeJson(payloadJson));

                // بدنه‌ای که واقعاً با HTTP ارسال شده است.
                // مقدار data در این بدنه رمزنگاری‌شده خواهد بود.
                entry.AppendLine(
                        "JSON ارسالی روی HTTP (رمزنگاری‌شده):")
                    .AppendLine(SanitizeJson(wireJson));

                // پاسخ خام JSON همان فراخوانی، چه موفق و چه ناموفق.
                // توکن احتمالی پاسخ login قبل از ثبت پوشانده می‌شود.
                entry.AppendLine("JSON پاسخ امید:")
                    .AppendLine(SanitizeJson(responseJson));

                if (error != null)
                {
                    // متن کامل Exception را اینجا نمی‌نویسیم؛ ممکن است
                    // پاسخ سرور یا اطلاعات محرمانه را دوباره شامل شود.
                    // جزئیات پاسخ HTTP در بخش JSON پاسخ بالا آمده است.
                    entry.Append("نوع خطا: ")
                        .AppendLine(error.GetType().Name);
                }

                entry.AppendLine();

                lock (FileLock)
                {
                    File.AppendAllText(
                        filePath,
                        entry.ToString(),
                        Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // لاگ قابلیت کمکی است؛ خرابی دیسک یا نداشتن مجوز
                // نباید باعث شود ارسال فاکتور وضعیت دیگری پیدا کند.
                // فقط نام نوع خطا را ثبت می‌کنیم، نه دادهٔ درخواست را.
                System.Diagnostics.Trace.TraceWarning(
                    "Omid API trace could not be written: {0}",
                    ex.GetType().Name);
            }
        }

        /// <summary>
        /// JSON را برای نمایش آماده می‌کند و مقدار فیلدهای محرمانه
        /// را قبل از ذخیره در فایل می‌پوشاند.
        /// </summary>
        private static string SanitizeJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return "(دریافت نشد)";

            try
            {
                JToken token = JToken.Parse(json);

                Redact(token);

                // قالب تورفته خواندن درخواست و پاسخ را در صفحهٔ لاگ آسان می‌کند.
                return token.ToString(Formatting.Indented);
            }
            catch (JsonReaderException)
            {
                // متن غیر JSON را خام ثبت نمی‌کنیم؛ ممکن است اطلاعات حساس داشته باشد.
                return "(متن دریافتی JSON معتبر نبود؛ متن خام در لاگ ذخیره نشد)";
            }
        }

        /// <summary>
        /// همهٔ سطح‌های JSON، از جمله data.token، را بررسی می‌کند.
        /// </summary>
        private static void Redact(JToken token)
        {
            if (token is JObject obj)
            {
                foreach (JProperty property in obj.Properties())
                {
                    if (IsSecret(property.Name))
                    {
                        property.Value = "***";
                    }
                    else
                    {
                        Redact(property.Value);
                    }
                }
            }
            else if (token is JArray array)
            {
                foreach (JToken item in array)
                {
                    Redact(item);
                }
            }
        }

        /// <summary>
        /// نام فیلدهایی که مقدارشان نباید در فایل لاگ دیده شود.
        /// </summary>
        private static bool IsSecret(string name)
        {
            return name.Equals(
                    "password",
                    StringComparison.OrdinalIgnoreCase)
                || name.Equals(
                    "token",
                    StringComparison.OrdinalIgnoreCase)
                || name.Equals(
                    "accessToken",
                    StringComparison.OrdinalIgnoreCase)
                || name.Equals(
                    "refreshToken",
                    StringComparison.OrdinalIgnoreCase)
                || name.Equals(
                    "apiKey",
                    StringComparison.OrdinalIgnoreCase)
                || name.Equals(
                    "apiSecret",
                    StringComparison.OrdinalIgnoreCase)
                || name.Equals(
                    "signature",
                    StringComparison.OrdinalIgnoreCase)
                || name.Equals(
                    "authorization",
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}