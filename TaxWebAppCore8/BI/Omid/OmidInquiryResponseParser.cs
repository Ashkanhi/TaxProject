using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidInquiryItemResult
    {
        public string RequestId { get; init; } = string.Empty;
        public string State { get; init; } = "Pending";
        public string? Message { get; init; }
        public JObject? Row { get; init; }
        // پاسخ خام همان تلاشی که این نتیجه را برگردانده است.
        public string ResponseJson { get; init; } = string.Empty;
        public bool IsFinal => State == "Accepted" || State == "Failed";
    }

    public static class OmidInquiryResponseParser
    {
        public static Dictionary<string, OmidInquiryItemResult> Parse(
            string responseJson, IReadOnlyCollection<string> requestedIds)
        {
            JObject root = JObject.Parse(responseJson);
            if (root["success"]?.Value<bool>() != true)
                throw new InvalidOperationException("استعلام امید ناموفق بود: " + root["message"]);

            JToken? data = root["data"];
            if (data is JObject envelope && envelope["success"]?.Type == JTokenType.Boolean)
            {
                if (envelope["success"]!.Value<bool>() != true)
                    throw new InvalidOperationException("استعلام امید ناموفق بود: " + envelope["message"]);
                data = envelope["data"];
            }

            var result = new Dictionary<string, OmidInquiryItemResult>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in requestedIds)
            {
                if (!Guid.TryParse(id, out Guid guid) || guid == Guid.Empty)
                    throw new InvalidOperationException("requestId استعلام معتبر نیست.");
                result.Add(guid.ToString(), new OmidInquiryItemResult
                {
                    RequestId = guid.ToString(), ResponseJson = responseJson,
                    Message = "نتیجهٔ قطعی این صورتحساب هنوز دریافت نشده است."
                });
            }

            IEnumerable<JObject> rows = data is JArray array ? array.OfType<JObject>()
                : data is JObject single ? new[] { single } : Array.Empty<JObject>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JObject row in rows)
            {
                // پاسخ بی‌شناسه یا متعلق به درخواست دیگر، نتیجهٔ فاکتور انتخاب‌شده نیست.
                if (!Guid.TryParse(row["requestId"]?.Value<string>(), out Guid guid) ||
                    !result.ContainsKey(guid.ToString())) continue;
                string id = guid.ToString();
                if (!seen.Add(id))
                    throw new InvalidOperationException("امید برای یک requestId چند نتیجه برگرداند؛ وضعیت تغییر نکرد.");

                string status = row["invoiceStatus"]?.Value<string>()?.Trim().ToUpperInvariant() ?? "";
                JToken? details = row["inquiryResultModel"]?["data"] ?? row["data"];
                bool? success = details?["success"]?.Type == JTokenType.Boolean
                    ? details["success"]!.Value<bool>() : null;
                JToken? errors = HasContent(row["errors"]) ? row["errors"] : details?["error"];
                bool hasErrors = HasContent(errors);
                string state = "Pending";
                // success=false به تنهایی دلیل رد قطعی نیست؛ نمونهٔ امید پاسخ موقت هم دارد.
                if (status == "REJECTED" || (status.Length == 0 && hasErrors && success != true))
                    state = "Failed";
                else if ((status == "ACCEPTED" || (status.Length == 0 && success == true)) && success != false && !hasErrors)
                    state = "Accepted";

                result[id] = new OmidInquiryItemResult
                {
                    RequestId = id, State = state, Row = (JObject)row.DeepClone(),
                    ResponseJson = responseJson,
                    Message = state == "Accepted" ? null : hasErrors ? errors!.ToString(Formatting.None)
                        : state == "Failed" ? "امید صورتحساب را رد کرده است."
                        : "نتیجهٔ قطعی این صورتحساب هنوز دریافت نشده است."
                };
            }
            return result;
        }

        private static bool HasContent(JToken? value) => value != null &&
            value.Type != JTokenType.Null && value.Type != JTokenType.Undefined &&
            !(value.Type == JTokenType.Boolean && !value.Value<bool>()) &&
            (value is JContainer container ? container.HasValues : !string.IsNullOrWhiteSpace(value.ToString()));
    }
}
