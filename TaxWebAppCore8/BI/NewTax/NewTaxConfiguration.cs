using TaxServiceAndApp.Models.SharedDTO;

namespace TaxWebAppCore8.BI.NewTax;

public sealed record NewTaxConfiguration(string ApiUrl, string MemoryId, string PrivateKeyPath)
{
    public static NewTaxConfiguration From(DTOServerInfo settings)
    {
        if (!Uri.TryCreate(settings.NEWTAX_API_URL?.Trim(), UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new InvalidOperationException("آدرس NewTax باید HTTPS و بدون نام کاربری، پارامتر یا fragment باشد.");
        var memory = settings.MEMORY_ID?.Trim() ?? "";
        if (memory.Length != 6 || memory.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9')))
            throw new InvalidOperationException("شناسه حافظه NewTax باید ۶ حرف بزرگ لاتین یا رقم باشد.");
        if (string.IsNullOrWhiteSpace(settings.NEWTAX_PRIVATE_KEY_PATH))
            throw new InvalidOperationException("مسیر فایل کلید خصوصی NewTax الزامی است.");
        return new(uri.AbsoluteUri.TrimEnd('/') + "/", memory, settings.NEWTAX_PRIVATE_KEY_PATH.Trim());
    }
}
