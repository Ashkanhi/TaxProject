using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaxWebAppCore8.BI.Omid
{
    // نتیجهٔ کلی که امید برای درخواست ارسال برگردانده است.
    public sealed class OmidSendResponse
    {
        public bool Success { get; set; }
        public string ResponseJson { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        // یک پاسخ ارسال می‌تواند نتیجهٔ چند فاکتور را داشته باشد.
        public List<OmidSendReceipt> Receipts { get; set; } = new();
    }

    // اطلاعات پیگیری مربوط به یک فاکتور در پاسخ امید.
    public sealed class OmidSendReceipt
    {
        public string RequestId { get; set; } = string.Empty;
        public string TrackingId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Fingerprint { get; set; } = string.Empty;
    }

    public static class OmidSendResponseParser
    {
        /// <summary>
        /// متن JSON پاسخ ارسال را به یک مدل قابل‌استفاده در برنامه تبدیل می‌کند.
        /// این متد هیچ چیزی در دیتابیس ثبت نمی‌کند.
        /// </summary>
        public static OmidSendResponse Parse(string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
                throw new InvalidOperationException(
                    "پاسخ ارسال امید خالی است.");

            JObject root;

            try
            {
                root = JObject.Parse(responseJson);
            }
            catch (JsonReaderException ex)
            {
                throw new InvalidOperationException(
                    "پاسخ ارسال امید JSON معتبر نیست.",
                    ex);
            }

            if (root["success"]?.Value<bool>() == false)
                return new OmidSendResponse
                {
                    Success = false, ResponseJson = responseJson,
                    Message = root["message"]?.Value<string>() ?? "امید ارسال را نپذیرفت.",
                    StatusCode = root["statusCode"]?.Value<int?>()
                };

            // بعضی نسخه‌های پاسخ، نتیجه را در data.success/data.data می‌گذارند؛
            // پاسخ جدید امید ممکن است receipt را مستقیم در data برگرداند.
            if (root["data"] is not JToken responseData ||
                responseData.Type == JTokenType.Null)
                throw new InvalidOperationException(
                    "بخش data در پاسخ ارسال امید پیدا نشد.");

            JObject? envelope = responseData as JObject;
            bool hasNestedResult =
                envelope?["success"]?.Type == JTokenType.Boolean;

            var result = new OmidSendResponse
            {
                ResponseJson = responseJson,
                Success = hasNestedResult
                    ? envelope!["success"]!.Value<bool>()
                    : root["success"]?.Value<bool>() ?? false,
                StatusCode = envelope?["statusCode"]?.Value<int?>()
                    ?? root["statusCode"]?.Value<int?>(),
                Message = envelope?["message"]?.Value<string>()
                    ?? root["message"]?.Value<string>()
                    ?? string.Empty
            };

            // پاسخ قدیمی‌تر: data.data آرایهٔ رسیدهاست.
            // پاسخ جدیدتر: خود data یک رسید تکی است.
            JToken? receiptData = hasNestedResult
                ? envelope!["data"]
                : responseData;

            if (receiptData is JArray invoiceResults)
            {
                foreach (JToken item in invoiceResults)
                {
                    if (item is not JObject invoiceResult)
                        throw new InvalidOperationException(
                            "یکی از ردیف‌های پاسخ ارسال امید معتبر نیست.");

                    result.Receipts.Add(ParseReceipt(invoiceResult));
                }
            }
            else if (receiptData is JObject singleReceipt &&
                     (singleReceipt["requestId"] != null ||
                      singleReceipt["trackingId"] != null ||
                      singleReceipt["inno"] != null))
            {
                result.Receipts.Add(ParseReceipt(singleReceipt));
            }

            return result;
        }

        private static OmidSendReceipt ParseReceipt(JObject value)
        {
            return new OmidSendReceipt
            {
                RequestId = value["requestId"]?.Value<string>()
                    ?? string.Empty,
                TrackingId = value["trackingId"]?.Value<string>()
                    ?? string.Empty,
                InvoiceNumber = value["inno"]?.Value<string>()
                    ?? string.Empty,
                Fingerprint = value["fingerprint"]?.Value<string>()
                    ?? string.Empty
            };
        }

        /// <summary>
        /// نتیجهٔ ارسال یک فاکتور را بررسی می‌کند و رسید پیگیری آن را برمی‌گرداند.
        /// این متد نه درخواست HTTP می‌فرستد و نه چیزی در دیتابیس ثبت می‌کند.
        /// </summary>
        public static OmidSendReceipt RequireSingleAccepted(
            OmidSendResponse response)
        {
            if (response == null)
                throw new InvalidOperationException(
                    "نتیجهٔ ارسال امید دریافت نشده است.");

            // HTTP 200 کافی نیست؛ خود امید نیز باید ارسال را پذیرفته باشد.
            if (!response.Success)
                throw new OmidSendBusinessException(
                    string.IsNullOrWhiteSpace(response.Message)
                        ? "امید درخواست ارسال فاکتور را نپذیرفت."
                        : response.Message, response.ResponseJson);

            // فعلاً هر درخواست عملیاتی فقط یک فاکتور خواهد داشت.
            // بنابراین باید دقیقاً یک رسید در پاسخ ببینیم.
            if (response.Receipts == null ||
                response.Receipts.Count != 1)
            {
                throw new InvalidOperationException(
                    "تعداد رسیدهای پاسخ امید با ارسال یک فاکتور سازگار نیست.");
            }

            var receipt = response.Receipts[0];

            // طبق پاسخ جدید امید، requestId شناسهٔ لازم برای استعلام است.
            if (receipt == null ||
                string.IsNullOrWhiteSpace(receipt.RequestId))
            {
                throw new InvalidOperationException(
                    "امید درخواست را پذیرفت، اما requestId برنگرداند؛ استعلام ممکن نیست.");
            }

            return receipt;
        }
    }
}
