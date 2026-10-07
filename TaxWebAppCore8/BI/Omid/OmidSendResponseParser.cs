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
        public int? StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        // یک پاسخ ارسال می‌تواند نتیجهٔ چند فاکتور را داشته باشد.
        public List<OmidSendReceipt> Receipts { get; set; } = new();
    }

    // اطلاعات پیگیری مربوط به یک فاکتور در پاسخ امید.
    public sealed class OmidSendReceipt
    {
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

            // مطابق پروژهٔ نمونه، نتیجهٔ اصلی ارسال در data قرار دارد.
            if (root["data"] is not JObject data)
                throw new InvalidOperationException(
                    "بخش data در پاسخ ارسال امید پیدا نشد.");

            if (data["success"]?.Type != JTokenType.Boolean)
                throw new InvalidOperationException(
                    "فیلد data.success در پاسخ ارسال امید پیدا نشد.");

            var result = new OmidSendResponse
            {
                Success = data["success"]!.Value<bool>(),
                StatusCode = data["statusCode"]?.Value<int?>(),
                Message = data["message"]?.Value<string>()
                    ?? string.Empty
            };

            // در نمونهٔ امید، نتیجهٔ هر فاکتور در آرایهٔ data.data است.
            if (data["data"] is JArray invoiceResults)
            {
                foreach (JToken item in invoiceResults)
                {
                    if (item is not JObject invoiceResult)
                        throw new InvalidOperationException(
                            "یکی از ردیف‌های پاسخ ارسال امید معتبر نیست.");

                    result.Receipts.Add(new OmidSendReceipt
                    {
                        TrackingId =
                            invoiceResult["trackingId"]?.Value<string>()
                            ?? string.Empty,

                        InvoiceNumber =
                            invoiceResult["inno"]?.Value<string>()
                            ?? string.Empty,

                        Fingerprint =
                            invoiceResult["fingerprint"]?.Value<string>()
                            ?? string.Empty
                    });
                }
            }

            return result;
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
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(response.Message)
                        ? "امید درخواست ارسال فاکتور را نپذیرفت."
                        : response.Message);

            // فعلاً هر درخواست عملیاتی فقط یک فاکتور خواهد داشت.
            // بنابراین باید دقیقاً یک رسید در پاسخ ببینیم.
            if (response.Receipts == null ||
                response.Receipts.Count != 1)
            {
                throw new InvalidOperationException(
                    "تعداد رسیدهای پاسخ امید با ارسال یک فاکتور سازگار نیست.");
            }

            var receipt = response.Receipts[0];

            // trackingId برای استعلام بعدی و اتصال پاسخ به فاکتور محلی لازم است.
            if (receipt == null ||
                string.IsNullOrWhiteSpace(receipt.TrackingId))
            {
                throw new InvalidOperationException(
                    "امید درخواست را پذیرفت، اما trackingId برنگرداند.");
            }

            return receipt;
        }
    }
}