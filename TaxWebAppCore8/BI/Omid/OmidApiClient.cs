using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaxWebAppCore8.BI.Omid
{
    /// <summary>
    /// مسئول ارتباط HTTP با امید است.
    /// دریافت فاکتور از دیتابیس و ذخیره نتیجه، بعداً در سرویس امید انجام می‌شود.
    /// </summary>
    public sealed class OmidApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly OmidConfig _config;

        public OmidApiClient(
            HttpClient httpClient,
            IOptions<OmidConfig> options)
        {
            // این دو وابستگی را برنامه هنگام ساخت کلاینت در اختیار ما می‌گذارد.
            _httpClient = httpClient;
            _config = options.Value;

            // آدرس پایه برای تمام درخواست‌ها استفاده می‌شود.
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        /// <summary>
        /// با نام کاربری و رمز تنظیمات وارد امید می‌شود
        /// و توکن دریافت‌شده را برمی‌گرداند.
        /// </summary>
        public async Task<string> LoginAsync(
            CancellationToken cancellationToken = default)
        {
            // قبل از درخواست، وجود اطلاعات ورود را بررسی می‌کنیم.
            if (string.IsNullOrWhiteSpace(_config.UserName) ||
                string.IsNullOrWhiteSpace(_config.Password))
            {
                throw new InvalidOperationException(
                    "نام کاربری یا رمز عبور امید تنظیم نشده است.");
            }

            // نام فیلد username مطابق نمونه کد ارسالی امید است.
            var payload = new
            {
                username = _config.UserName,
                password = _config.Password
            };

            // ورود هم مانند سایر درخواست‌ها رمزنگاری و امضا می‌شود،
            // اما هنوز توکنی نداریم که همراه آن ارسال کنیم.
            string responseJson = await PostEncryptedAsync(
                "/api/Auth/login",
                payload,
                token: null,
                cancellationToken: cancellationToken);

            // طبق نمونه، توکن در مسیر data.token پاسخ قرار دارد.
            JObject response = JObject.Parse(responseJson);
            string? token = response["data"]?["token"]?.Value<string>();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "پاسخ ورود امید دریافت شد، اما data.token خالی است.");
            }

            // توکن را برمی‌گردانیم تا برای درخواست‌های بعدی استفاده شود.
            // آن را در کنسول یا لاگ چاپ نمی‌کنیم.
            return token;
        }

        
        /// <summary>
        /// درخواست معتبر فاکتور را به امید می‌فرستد
        /// و پاسخ خام را به مدل قابل‌بررسی تبدیل می‌کند.
        /// </summary>
        public async Task<OmidSendResponse> SendInvoiceAsync(
            OmidSendInvoiceRequest payload,
            CancellationToken cancellationToken = default)
        {
            // قبل از ارتباط شبکه‌ای، دادهٔ ارسالی را بررسی می‌کنیم.
            OmidTaxService.ValidateForSend(payload);

            // توکن لازم برای ارسال را از سرویس امید می‌گیریم.
            string token = await LoginAsync(cancellationToken);

            // درخواست را رمزنگاری و امضا کرده، به امید می‌فرستیم.
            string responseJson = await PostEncryptedAsync(
                "/api/Invoice/SendInvoice",
                payload,
                token,
                cancellationToken);

            // HTTP موفق به‌تنهایی به معنی پذیرش فاکتور نیست.
            // Parser نتیجهٔ کسب‌وکار و شناسه‌های پیگیری را جدا می‌کند.
            return OmidSendResponseParser.Parse(responseJson);
        }


        /// <summary>
        /// داده را رمزنگاری و امضا می‌کند و با POST به امید می‌فرستد.
        /// خروجی، متن پاسخ سرور است.
        /// </summary>
        public async Task<string> PostEncryptedAsync(
            string path,
            object payload,
            string? token = null,
            CancellationToken cancellationToken = default)
        {
            // بررسی وجود کلیدهای موردنیاز قبل از رمزنگاری.
            ValidateSecuritySettings();

            // تبدیل اطلاعات ورودی به JSON فشرده.
            string plainJson = JsonConvert.SerializeObject(
                payload,
                Formatting.None);

            // رمزنگاری اطلاعات با کلید و IV دریافت‌شده از امید.
            string encryptedData = OmidCryptoHelper.EncryptAes(
                plainJson,
                _config.AesKeyBase64,
                _config.AesIvBase64);

            // ساخت بدنه نهایی درخواست.
            // همین رشته بدون تغییر امضا و ارسال می‌شود.
            string bodyJson = JsonConvert.SerializeObject(
                new { data = encryptedData },
                Formatting.None);

            // تولید زمان یونیکس بر حسب ثانیه.
            string timestamp = DateTimeOffset.UtcNow
                .ToUnixTimeSeconds()
                .ToString(System.Globalization.CultureInfo.InvariantCulture);

            // تولید شناسه یکتا برای این درخواست.
            string nonce = Guid.NewGuid().ToString();

            // تولید امضا از بدنه نهایی + زمان + شناسه یکتا.
            string signature = OmidCryptoHelper.GenerateSignature(
                _config.ApiSecret,
                bodyJson,
                timestamp,
                nonce);

            // ساخت درخواست POST.
            using var request = new HttpRequestMessage(HttpMethod.Post, path);

            // قرار دادن JSON رمزنگاری‌شده در بدنه درخواست.
            request.Content = new StringContent(
                bodyJson,
                Encoding.UTF8,
                "application/json");

            // افزودن هدرهای امنیتی الزامی.
            request.Headers.Add("X-Api-Key", _config.ApiKey);
            request.Headers.Add("X-Timestamp", timestamp);
            request.Headers.Add("X-Nonce", nonce);
            request.Headers.Add("X-Signature", signature);

            // برای درخواست‌های بعد از ورود، توکن هم ارسال می‌شود.
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

#if DEBUG
            // خواندن بدنه از خود درخواست برای ثبت دقیق محتوای ارسالی.
            string sentBody = await request.Content.ReadAsStringAsync(
                cancellationToken);

            System.Diagnostics.Debug.WriteLine("===== OMID REQUEST =====");
            System.Diagnostics.Debug.WriteLine(
                $"URL: {new Uri(_httpClient.BaseAddress!, path)}");
            System.Diagnostics.Debug.WriteLine("METHOD: POST");
            System.Diagnostics.Debug.WriteLine("BODY:");
            System.Diagnostics.Debug.WriteLine(sentBody);
#endif

            // ارسال درخواست به امید.
            // در صورت خطای شبکه، پاسخ HTTP نداریم؛ خطا به فراخواننده منتقل می‌شود.
            using HttpResponseMessage response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            // دریافت متن اصلی پاسخ بدون تغییر.
            string responseText = await response.Content.ReadAsStringAsync(
                cancellationToken);

#if DEBUG
            // ثبت پاسخ قبل از بررسی موفقیت، تا پاسخ خطا هم نمایش داده شود.
            System.Diagnostics.Debug.WriteLine("===== OMID RESPONSE =====");
            System.Diagnostics.Debug.WriteLine(
                $"HTTP: {(int)response.StatusCode}");
            System.Diagnostics.Debug.WriteLine("BODY:");
            System.Diagnostics.Debug.WriteLine(responseText);
            System.Diagnostics.Debug.WriteLine("=========================");
#endif

            // اگر HTTP ناموفق باشد، کد وضعیت و متن اصلی پاسخ
            // در قالب خطا به لایه فراخواننده منتقل می‌شود.
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"خطای ارتباط با امید؛ HTTP {(int)response.StatusCode}. " +
                    $"پاسخ سرور: {responseText}",
                    inner: null,
                    statusCode: response.StatusCode);
            }

            // برگرداندن پاسخ برای پردازش توسط فراخواننده.
            return responseText;
        }

        /// <summary>
        /// وجود تنظیمات امنیتی را قبل از ارسال بررسی می‌کند.
        /// </summary>
        private void ValidateSecuritySettings()
        {
            if (string.IsNullOrWhiteSpace(_config.ApiKey) ||
                string.IsNullOrWhiteSpace(_config.ApiSecret) ||
                string.IsNullOrWhiteSpace(_config.AesKeyBase64) ||
                string.IsNullOrWhiteSpace(_config.AesIvBase64))
            {
                throw new InvalidOperationException(
                    "کلیدهای دسترسی یا تنظیمات رمزنگاری امید کامل نیست.");
            }
        }
    }
}