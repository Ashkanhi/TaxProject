using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

namespace TaxWebAppCore8.BI;

public sealed record CustomerSettingField(string Key, string Label, string Group, bool Secret = false, string Services = "")
{
    public bool AppliesTo(string service) => Services.Length == 0 || Services.Split(' ').Contains(service);
}

public sealed class CustomerSettingsForm
{
    public string Version { get; set; } = "";
    public Dictionary<string, string> Values { get; set; } = new();
    public HashSet<string> ConfiguredSecrets { get; set; } = new();
}

// Keep local edits separate from the distributed template, so a build does not overwrite them.
public sealed class CustomerSettingsStore(string resourcesPath)
{
    private static readonly object Gate = new();
    public static readonly Dictionary<string, string> Services = new()
    {
        ["Omid"] = "امید", ["Tax"] = "مستقیم", ["Rima"] = "ریما",
        ["RimaCalculate"] = "ریما با محاسبه", ["Tis"] = "تیس",
        ["NewTax"] = "مستقیم بدون گواهی (NewTax)"
    };
    public static readonly CustomerSettingField[] Fields =
    [
        new("CustomerName", "نام مشتری", "اطلاعات مشتری"),
        new("MEMORY_ID", "شناسه یکتای حافظه مالیاتی", "اطلاعات مشتری"),
        new("TINS", "شماره اقتصادی", "اطلاعات مشتری"),
        new("StoreId", "شناسه فروشگاه", "اطلاعات مشتری"),
        new("ServiceType", "نوع ارسال", "سرویس ارسال"),
        new("API_URL", "آدرس سرویس", "سرویس ارسال", Services: "Tax Rima RimaCalculate Tis"),
        new("NEWTAX_API_URL", "آدرس سرویس NewTax", "تنظیمات NewTax", Services: "NewTax"),
        new("NEWTAX_PRIVATE_KEY_PATH", "مسیر فایل کلید خصوصی PEM روی سرور", "تنظیمات NewTax", Services: "NewTax"),
        new("OMID_API_URL", "آدرس سرویس امید", "تنظیمات امید", Services: "Omid"),
        new("OMID_USERNAME", "نام کاربری امید", "تنظیمات امید", Services: "Omid"),
        new("OMID_PASSWORD", "رمز عبور امید", "تنظیمات امید", true, "Omid"),
        new("OMID_API_KEY", "کلید API", "تنظیمات امید", true, "Omid"),
        new("OMID_API_SECRET", "رمز API", "تنظیمات امید", true, "Omid"),
        new("OMID_AES_KEY_BASE64", "کلید AES (Base64)", "تنظیمات امید", true, "Omid"),
        new("OMID_AES_IV_BASE64", "بردار AES (Base64)", "تنظیمات امید", true, "Omid"),
        new("DataBaseType", "نوع دیتابیس", "اتصال دیتابیس"),
        new("DataBaseServer", "آدرس سرور", "اتصال دیتابیس"),
        new("DbName", "نام دیتابیس", "اتصال دیتابیس"),
        new("UserId", "نام کاربری دیتابیس", "اتصال دیتابیس"),
        new("UserPass", "رمز عبور دیتابیس", "اتصال دیتابیس", true),
        new("ConnectionTimeOut", "مهلت اتصال (ثانیه)", "اتصال دیتابیس"),
        new("TokenType", "نوع گواهی / توکن", "احراز هویت و امضا", Services: "Tax"),
        new("FILE_KEY_PRIVATE", "کلید خصوصی / توکن سرویس", "احراز هویت و امضا", true, "Tax Rima RimaCalculate Tis"),
        new("FILE_CERTIFICATE", "مسیر فایل گواهی روی سرور", "احراز هویت و امضا", Services: "Tax"),
        new("CertificateName", "نام گواهی / نام کاربری", "احراز هویت و امضا", Services: "Tax Rima RimaCalculate"),
        new("PASSWORD", "رمز عبور گواهی / سرویس", "احراز هویت و امضا", true, "Tax Rima RimaCalculate"),
        new("LIBRARY_PATH", "مسیر کتابخانه توکن روی سرور", "احراز هویت و امضا", Services: "Tax")
    ];

    public static string EffectivePath(string root)
    {
        var local = Path.Combine(root, "CustomerID", "CustomerInformation.local.xml");
        return File.Exists(local) ? local : Path.Combine(root, "CustomerID", "CustomerInformation.xml");
    }

    private static XDocument Parse(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var doc = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        if (doc.Root?.Name != "DTOServerInfo") throw new InvalidDataException("Invalid settings root.");
        return doc;
    }

    private static string Version(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    public CustomerSettingsForm Read(bool includeSecrets = false)
    {
        lock (Gate)
        {
            var bytes = File.ReadAllBytes(EffectivePath(resourcesPath));
            var root = Parse(bytes).Root!;
            var form = new CustomerSettingsForm { Version = Version(bytes) };
            foreach (var field in Fields)
            {
                var value = root.Element(field.Key)?.Value ?? "";
                if (field.Secret)
                {
                    if (value.Length > 0) form.ConfiguredSecrets.Add(field.Key);
                    form.Values[field.Key] = includeSecrets ? value : "";
                }
                else form.Values[field.Key] = value;
            }
            var service = Services.Keys.FirstOrDefault(k => k.Equals(form.Values["ServiceType"].Trim(), StringComparison.OrdinalIgnoreCase));
            if (service != null) form.Values["ServiceType"] = service;
            return form;
        }
    }

    public List<string> Save(CustomerSettingsForm form)
    {
        lock (Gate)
        {
            var bytes = File.ReadAllBytes(EffectivePath(resourcesPath));
            if (form.Version != Version(bytes)) return ["تنظیمات در این فاصله تغییر کرده است؛ صفحه را دوباره باز کنید."];
            var doc = Parse(bytes);
            var errors = new List<string>();
            var selectedService = form.Values.GetValueOrDefault("ServiceType", "");
            foreach (var field in Fields)
            {
                // Inactive service fields are preserved regardless of what the browser submits.
                if (!field.AppliesTo(selectedService)) continue;
                if (!form.Values.TryGetValue(field.Key, out var value))
                {
                    errors.Add($"فیلد «{field.Label}» در درخواست موجود نیست.");
                    continue;
                }
                value ??= "";
                if (field.Secret && value.Length == 0) continue;
                if (!field.Secret) value = value.Trim();
                if (value.Length > 4096) { errors.Add($"مقدار «{field.Label}» طولانی است."); continue; }
                try { XmlConvert.VerifyXmlChars(value); }
                catch (XmlException) { errors.Add($"مقدار «{field.Label}» معتبر نیست."); continue; }
                var element = doc.Root!.Element(field.Key);
                if (element == null) doc.Root.Add(new XElement(field.Key, value));
                else element.Value = value;
            }
            string Get(string key) => doc.Root!.Element(key)?.Value ?? "";
            if (!Services.ContainsKey(Get("ServiceType"))) errors.Add("نوع ارسال معتبر را انتخاب کنید.");
            foreach (var key in new[] { "CustomerName", "DataBaseServer", "DbName", "UserId", "DataBaseType" })
                if (string.IsNullOrWhiteSpace(Get(key))) errors.Add($"«{Fields.Single(f => f.Key == key).Label}» الزامی است.");
            if (!int.TryParse(Get("ConnectionTimeOut"), out var timeout) || timeout < 1 || timeout > 3600)
                errors.Add("مهلت اتصال باید عددی بین ۱ تا ۳۶۰۰ باشد.");
            if (!int.TryParse(Get("StoreId"), out var store) || store < -1) errors.Add("شناسه فروشگاه باید عددی از منفی یک به بالا باشد.");
            var urlKey = Get("ServiceType") switch { "Omid" => "OMID_API_URL", "NewTax" => "NEWTAX_API_URL", _ => "API_URL" };
            if (!Uri.TryCreate(Get(urlKey), UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http"))
                errors.Add("آدرس سرویس انتخاب‌شده باید یک آدرس معتبر HTTP یا HTTPS باشد.");
            if (Get("ServiceType") == "NewTax")
            {
                if (uri?.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                    errors.Add("آدرس NewTax باید HTTPS و بدون نام کاربری، پارامتر یا fragment باشد.");
                if (string.IsNullOrWhiteSpace(Get("NEWTAX_PRIVATE_KEY_PATH"))) errors.Add("مسیر کلید خصوصی NewTax الزامی است.");
                if (Get("MEMORY_ID").Length != 6 || Get("MEMORY_ID").Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9')))
                    errors.Add("شناسه حافظه NewTax باید ۶ حرف بزرگ لاتین یا رقم باشد.");
                if (string.IsNullOrWhiteSpace(Get("TINS"))) errors.Add("شماره اقتصادی فروشنده الزامی است.");
            }
            if (errors.Count > 0) return errors;

            var destination = Path.Combine(resourcesPath, "CustomerID", "CustomerInformation.local.xml");
            var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                doc.Save(temporary, SaveOptions.DisableFormatting);
                if (File.Exists(destination)) File.Replace(temporary, destination, destination + ".bak");
                else File.Move(temporary, destination);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return [];
        }
    }
}
