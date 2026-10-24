using TaxServiceAndApp.Models.SharedDTO;

namespace TaxWebAppCore8.BI.Omid;

public sealed class OmidSettingsValidationException(string message) : ArgumentException(message) { }

public static class OmidSettingsConnectionTest
{
    public const string ClientName = "OmidSettingsTest";

    public static OmidConfig BuildConfig(IReadOnlyDictionary<string, string> values, IReadOnlyDictionary<string, string> saved)
    {
        string Value(string key, bool secret = false)
        {
            var value = values.GetValueOrDefault(key, "") ?? "";
            if (secret && value.Length == 0) value = saved.GetValueOrDefault(key, "") ?? "";
            if (value.Length > 4096) throw new OmidSettingsValidationException("طول یکی از مقادیر تنظیمات امید بیش از حد مجاز است.");
            return value;
        }

        var config = OmidConfig.FromCustomer(new DTOServerInfo
        {
            OMID_API_URL = Value("OMID_API_URL"), OMID_USERNAME = Value("OMID_USERNAME"),
            OMID_API_KEY = Value("OMID_API_KEY", true), OMID_API_SECRET = Value("OMID_API_SECRET", true),
            OMID_PASSWORD = Value("OMID_PASSWORD", true), OMID_AES_KEY_BASE64 = Value("OMID_AES_KEY_BASE64", true),
            OMID_AES_IV_BASE64 = Value("OMID_AES_IV_BASE64", true)
        });
        if (!Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new OmidSettingsValidationException("آدرس سرویس امید باید یک آدرس معتبر HTTP یا HTTPS باشد.");
        if (string.IsNullOrWhiteSpace(config.UserName) || string.IsNullOrWhiteSpace(config.Password))
            throw new OmidSettingsValidationException("نام کاربری و رمز عبور امید را وارد کنید.");
        if (string.IsNullOrWhiteSpace(config.ApiKey) || string.IsNullOrWhiteSpace(config.ApiSecret))
            throw new OmidSettingsValidationException("کلید API و رمز API امید را وارد کنید.");
        if (config.ApiKey.Contains('\r') || config.ApiKey.Contains('\n'))
            throw new OmidSettingsValidationException("کلید API نباید شامل شکست خط باشد.");
        try
        {
            if (Convert.FromBase64String(config.AesKeyBase64).Length is not (16 or 24 or 32) ||
                Convert.FromBase64String(config.AesIvBase64).Length != 16)
                throw new FormatException();
        }
        catch (FormatException)
        { throw new OmidSettingsValidationException("کلید AES و بردار AES باید Base64 معتبر باشند؛ طول کلید ۱۶، ۲۴ یا ۳۲ بایت و طول بردار ۱۶ بایت است."); }
        return config;
    }
}
