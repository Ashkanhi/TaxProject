using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidInquiryRetryResult
    {
        public OmidInquiryRetryResult(
            string responseJson,
            int attempts,
            bool hasFinalResult,
            bool? finalSuccess = null,
            string? finalMessage = null)
        {
            ResponseJson = responseJson;
            Attempts = attempts;
            HasFinalResult = hasFinalResult;
            FinalSuccess = finalSuccess;
            FinalMessage = finalMessage;
        }

        public string ResponseJson { get; }
        public int Attempts { get; }
        public bool HasFinalResult { get; }
        public bool? FinalSuccess { get; }
        public string? FinalMessage { get; }
    }

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
            // توکن لازم برای ارسال را از سرویس امید می‌گیریم.
            string token = await LoginAsync(cancellationToken);

            return await SendInvoiceAsync(
                payload,
                token,
                cancellationToken);
        }

        /// <summary>
        /// فاکتور را با توکنی که قبلاً گرفته شده ارسال می‌کند.
        /// سرویس ارسال از این overload استفاده می‌کند تا خطای Login
        /// قبل از ثبت وضعیت Sending رخ دهد.
        /// </summary>
        public async Task<OmidSendResponse> SendInvoiceAsync(
            OmidSendInvoiceRequest payload,
            string token,
            CancellationToken cancellationToken = default)
        {
            // اعتبارسنجی قبل از درخواست شبکه‌ای ارسال فاکتور.
            OmidTaxService.ValidateForSend(payload);

            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(
                    "توکن امید برای ارسال صورتحساب خالی است.");

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
        /// سازگاری با فراخوانی تکی فعلی؛ همان مسیر مشترک استعلام را اجرا می‌کند.
        /// </summary>
        public Task<string> InquiryAsync(
            string requestId,
            CancellationToken cancellationToken = default)
        {
            return InquiryAsync(
                new[] { requestId },
                cancellationToken);
        }

        /// <summary>
        /// یک یا چند requestId را در یک درخواست به امید استعلام می‌زند.
        /// پاسخ فعلاً خام برمی‌گردد تا ساختار واقعی آن را بررسی کنیم.
        /// </summary>
        public async Task<string> InquiryAsync(
            IReadOnlyCollection<string> requestIds,
            CancellationToken cancellationToken = default)
        {
            // عدد ۱۰۰ محدودیت داخلی برنامهٔ ما برای اندازهٔ هر دسته است؛
            // در مستند، سقف رسمی تعداد requestIds مشخص نشده است.
            if (requestIds == null ||
                requestIds.Count == 0 ||
                requestIds.Count > 100)
            {
                throw new InvalidOperationException(
                    "در هر درخواست استعلام، بین ۱ تا ۱۰۰ requestId لازم است.");
            }

            var ids = new List<string>();
            var uniqueIds = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string requestId in requestIds)
            {
                if (string.IsNullOrWhiteSpace(requestId))
                    throw new InvalidOperationException(
                        "یکی از requestIdهای استعلام خالی است.");

                string id = requestId.Trim();

                if (!uniqueIds.Add(id))
                    throw new InvalidOperationException(
                        $"requestId {id} در انتخاب‌ها تکراری است.");

                ids.Add(id);
            }

            // برای کل دسته یک‌بار وارد امید می‌شویم.
            string token = await LoginAsync(cancellationToken);

            // برای یک فاکتور نیز requestIds آرایه‌ای با یک عضو خواهد بود.
            var payload = new { requestIds = ids };

            return await PostEncryptedAsync(
                "/api/Invoice/Invoiceinquiry",
                payload,
                token,
                cancellationToken);
        }

        /// <summary>
        /// برای آزمون، استعلام را حداکثر ۲۰ بار و با فاصلهٔ ۳۰ ثانیه تکرار می‌کند.
        /// Login فقط یک‌بار انجام می‌شود و فاکتور دوباره ارسال نمی‌شود.
        /// </summary>
        public async Task<OmidInquiryRetryResult> InquiryWithRetryAsync(
            IReadOnlyCollection<string> requestIds,
            CancellationToken cancellationToken = default)
        {
            const int maxAttempts = 20;
            TimeSpan retryInterval = TimeSpan.FromSeconds(30);

            if (requestIds == null || requestIds.Count == 0 ||
                requestIds.Count > 100)
            {
                throw new InvalidOperationException(
                    "در هر درخواست استعلام، بین ۱ تا ۱۰۰ requestId لازم است.");
            }

            var ids = new List<string>();
            var uniqueIds = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string requestId in requestIds)
            {
                if (string.IsNullOrWhiteSpace(requestId))
                    throw new InvalidOperationException(
                        "یکی از requestIdهای استعلام خالی است.");

                string id = requestId.Trim();
                if (!uniqueIds.Add(id))
                    throw new InvalidOperationException(
                        $"requestId {id} در انتخاب‌ها تکراری است.");

                ids.Add(id);
            }

            // مثل نمونهٔ امید، برای همهٔ تلاش‌ها یک توکن می‌گیریم.
            string token = await LoginAsync(cancellationToken);
            var payload = new { requestIds = ids };
            string lastResponse = string.Empty;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                lastResponse = await PostEncryptedAsync(
                    "/api/Invoice/Invoiceinquiry",
                    payload,
                    token,
                    cancellationToken);

                if (TryReadFinalInquiryResult(
                    lastResponse,
                    out bool? finalSuccess,
                    out string? finalMessage))
                {
                    return new OmidInquiryRetryResult(
                        lastResponse,
                        attempt,
                        hasFinalResult: true,
                        finalSuccess,
                        finalMessage);
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(retryInterval, cancellationToken);
                }
            }

            return new OmidInquiryRetryResult(
                lastResponse, maxAttempts, hasFinalResult: false);
        }

        // ساختار نتیجه را مطابق نمونهٔ ارسالی امید بررسی می‌کند.
        private static bool TryReadFinalInquiryResult(
            string responseJson,
            out bool? finalSuccess,
            out string? finalMessage)
        {
            finalSuccess = null;
            finalMessage = null;
            JObject response = JObject.Parse(responseJson);
            if (response["success"]?.Value<bool>() != true)
                return false;

            JToken? data = response["data"];
            if (data == null || data.Type == JTokenType.Null)
                return false;

            var results = new List<JObject>();
            if (data is JArray items)
            {
                foreach (JToken item in items)
                {
                    if (FindInquiryResultData(item) is not JObject itemResult ||
                        itemResult["success"]?.Type != JTokenType.Boolean)
                    {
                        return false;
                    }

                    results.Add(itemResult);
                }
            }
            else if (FindInquiryResultData(data) is JObject itemResult &&
                     itemResult["success"]?.Type == JTokenType.Boolean)
            {
                results.Add(itemResult);
            }

            if (results.Count == 0)
                return false;

            bool[] statuses = results
                .Select(item => item["success"]!.Value<bool>())
                .ToArray();

            // فقط وقتی تمام صورتحساب‌های دسته وضعیت یکسان دارند،
            // می‌توان یک وضعیت مشترک را به رکوردها نسبت داد.
            if (statuses.All(value => value))
                finalSuccess = true;
            else if (statuses.All(value => !value))
                finalSuccess = false;

            finalMessage = string.Join(
                " | ",
                results.Select(GetInquiryMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .Distinct());

            return true;
        }

        private static JToken? FindInquiryResultData(JToken? item)
        {
            if (item is not JObject obj)
                return null;

            JToken? result = obj["inquiryResultModel"]?["data"];
            result ??= obj["data"];

            // برخی پاسخ‌ها خود مدل نتیجه را مستقیماً در data قرار می‌دهند.
            if (result == null && obj["success"]?.Type == JTokenType.Boolean)
                result = obj;

            return result;
        }

        private static string? GetInquiryMessage(JObject result)
        {
            string? message = result["message"]?.ToString();
            if (!string.IsNullOrWhiteSpace(message))
                return message;

            JToken? errors = result["errors"];
            return errors == null || errors.Type == JTokenType.Null
                ? null
                : errors.ToString(Formatting.None);
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

            string? responseText = null;
            int? httpStatusCode = null;
            Exception? callError = null;

            try
            {
                // هر دو سرویس login و SendInvoice از همین مسیر عبور می‌کنند.
                using HttpResponseMessage response = await _httpClient.SendAsync(
                    request,
                    cancellationToken);

                httpStatusCode = (int)response.StatusCode;
                responseText = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"خطای ارتباط با امید؛ HTTP {httpStatusCode}. " +
                        $"پاسخ سرور: {responseText}",
                        inner: null,
                        statusCode: response.StatusCode);
                }

                return responseText;
            }
            catch (Exception ex)
            {
                callError = ex;
                throw;
            }
            finally
            {
                // درخواست خوانا، بدنهٔ رمزنگاری‌شده و پاسخ همان فراخوانی
                // در یک رخداد لاگ ثبت می‌شوند؛ اطلاعات محرمانه پوشانده می‌شوند.
                OmidApiTraceLogger.TryWrite(
                    path,
                    plainJson,
                    bodyJson,
                    httpStatusCode,
                    responseText,
                    callError);
            }
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
