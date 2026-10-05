using System;
using System.Security.Cryptography;
using System.Text;

namespace TaxWebAppCore8.BI.Omid
{
    /// <summary>
    /// ابزارهای امنیتی ارتباط با امید:
    /// ۱. رمزنگاری اطلاعات قبل از ارسال
    /// ۲. تولید امضا برای بررسی صحت درخواست
    /// </summary>
    public static class OmidCryptoHelper
    {
        /// <summary>
        /// JSON اصلی را با AES رمزنگاری می‌کند
        /// و نتیجه را به‌صورت رشته Base64 برمی‌گرداند.
        /// </summary>
        /// <param name="plainText">JSON اصلی، قبل از رمزنگاری</param>
        /// <param name="aesKeyBase64">کلید AES دریافتی از امید، با قالب Base64</param>
        /// <param name="aesIvBase64">بردار اولیه دریافتی از امید، با قالب Base64</param>
        public static string EncryptAes(
            string plainText,
            string aesKeyBase64,
            string aesIvBase64)
        {
            // کلید و IV در تنظیمات به‌صورت متن Base64 نگهداری می‌شوند.
            // برای استفاده در AES باید آن‌ها را به آرایه بایت تبدیل کنیم.
            byte[] key = Convert.FromBase64String(aesKeyBase64);
            byte[] iv = Convert.FromBase64String(aesIvBase64);

            // ایجاد ابزار رمزنگاری AES.
            // using باعث می‌شود منابع آن پس از پایان متد آزاد شوند.
            using Aes aes = Aes.Create();

            // تعیین کلید رمزنگاری و بردار اولیه مطابق اطلاعات دریافتی از امید.
            aes.Key = key;
            aes.IV = iv;

            // مطابق مستندات امید، حالت رمزنگاری باید CBC باشد.
            aes.Mode = CipherMode.CBC;

            // AES داده را در بلوک‌های ۱۶ بایتی پردازش می‌کند.
            // PKCS7 بایت‌های لازم برای تکمیل بلوک آخر را اضافه می‌کند.
            aes.Padding = PaddingMode.PKCS7;

            // ساخت عملیات رمزنگاری با تنظیمات بالا.
            using ICryptoTransform encryptor = aes.CreateEncryptor();

            // تبدیل JSON به بایت با UTF-8؛
            // این کدگذاری از متن فارسی هم پشتیبانی می‌کند.
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            // رمزنگاری تمام بایت‌های JSON از ابتدا تا انتها.
            byte[] encryptedBytes = encryptor.TransformFinalBlock(
                plainBytes,
                0,
                plainBytes.Length);

            // تبدیل بایت‌های رمز‌شده به متن Base64 برای قرارگرفتن در JSON.
            // Base64 خودش رمزنگاری نیست؛ فقط نمایش متنی داده باینری است.
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// امضای درخواست را مطابق فرمول امید تولید می‌کند.
        /// سرور با محاسبه همین مقدار، صحت درخواست را بررسی می‌کند.
        /// </summary>
        /// <param name="apiSecret">کلید محرمانه امضا، دریافتی از امید</param>
        /// <param name="bodyJson">
        /// بدنه نهایی درخواست شامل فیلد data؛ دقیقاً همان متن ارسالی
        /// </param>
        /// <param name="timestamp">زمان یونیکس بر حسب ثانیه، مطابق هدر درخواست</param>
        /// <param name="nonce">شناسه یکتای درخواست، مطابق هدر درخواست</param>
        public static string GenerateSignature(
            string apiSecret,
            string bodyJson,
            string timestamp,
            string nonce)
        {
            // مطابق نمونه امید، ApiSecret مستقیماً با UTF-8 به بایت تبدیل می‌شود.
            // برخلاف کلید AES، این مقدار را از Base64 تبدیل نمی‌کنیم.
            byte[] keyBytes = Encoding.UTF8.GetBytes(apiSecret);

            // ترتیب اتصال بخش‌ها در محاسبه امضا مهم است:
            // بدنه نهایی + زمان + شناسه یکتا
            // بین این بخش‌ها فاصله یا جداکننده اضافه نمی‌کنیم.
            string message = bodyJson + timestamp + nonce;

            // تبدیل متن ترکیبی به بایت برای محاسبه امضا.
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // ایجاد HMAC-SHA256 با کلید محرمانه.
            // این عملیات امضا تولید می‌کند و داده را رمزنگاری نمی‌کند.
            using var hmac = new HMACSHA256(keyBytes);

            // محاسبه امضای پیام.
            byte[] hash = hmac.ComputeHash(messageBytes);

            // تبدیل امضا به Base64؛
            // کلاینت این مقدار را در هدر X-Signature قرار می‌دهد.
            return Convert.ToBase64String(hash);
        }
    }
}