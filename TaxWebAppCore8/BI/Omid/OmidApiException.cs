using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidApiException : HttpRequestException
    {
        public string ResponseJson { get; }
        public OmidApiException(HttpStatusCode status, string responseJson)
            : base($"خطای امید؛ HTTP {(int)status}. پاسخ سرور: {responseJson}", null, status)
            => ResponseJson = responseJson;
    }

    public sealed class OmidSendBusinessException : InvalidOperationException
    {
        public string ResponseJson { get; }
        public OmidSendBusinessException(string message, string responseJson) : base(message)
            => ResponseJson = responseJson;
    }

    public static class OmidSendFailureClassifier
    {
        public static bool TryGetDefinitiveRejection(Exception exception, out string rejection)
        {
            rejection = string.Empty;
            string? json = exception switch
            {
                OmidApiException http => http.ResponseJson,
                OmidSendBusinessException business => business.ResponseJson,
                _ => null
            };
            if (json == null) return false;
            try
            {
                JObject root = JObject.Parse(json);
                JObject response = root["data"] is JObject nested && nested["success"]?.Type == JTokenType.Boolean
                    ? nested : root;
                if (response["success"]?.Value<bool>() != false) return false;
                // خطای تکراری بودن شماره ممکن است نشان‌دهندهٔ ارسال قبلی باشد؛ دوباره ارسال نمی‌کنیم.
                if (response["statusCode"]?.Value<int?>() == 409) return false;
                var errors = response["errors"] as JArray;
                string message = response["message"]?.ToString() ?? string.Empty;
                if (message.Contains("تکراری") || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)) return false;
                // Internal Error عمومی و سایر خطاهای نامشخص، مجوز ارسال مجدد نیستند.
                bool IsValidation(JToken error)
                {
                    string code = (error as JObject)?["code"]?.ToString() ?? "";
                    string text = error.ToString();
                    if (text.Contains("تکراری") || text.Contains("duplicate", StringComparison.OrdinalIgnoreCase)) return false;
                    return code == "INVALID_VALUE" || code == "VALIDATION_ERROR" || code == "REQUIRED" ||
                        text.Contains("الزامی") || text.Contains("باید") ||
                        text.Contains("The JSON value could not be converted", StringComparison.OrdinalIgnoreCase);
                }
                if (errors?.Count > 0 && errors.All(IsValidation))
                    rejection = string.Join(" | ", errors.Select(item => item.ToString()));
                else if ((errors == null || errors.Count == 0) && IsValidation(new JValue(message)))
                    rejection = message;
                return !string.IsNullOrWhiteSpace(rejection);
            }
            catch { return false; }
        }
    }
}
