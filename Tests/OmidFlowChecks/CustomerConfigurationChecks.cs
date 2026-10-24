using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using System.Xml.Serialization;
using TaxServiceAndApp.Models.SharedDTO;
using TaxWebAppCore8.BI;
using TaxWebAppCore8.BI.Omid;

internal static class CustomerConfigurationChecks
{
    public static OmidConfig Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "omid-customer-configuration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "CustomerID"));
        try
        {
            var xml = new XDocument(new XElement("DTOServerInfo", CustomerSettingsStore.Fields.Select(f => new XElement(f.Key, "fixture"))));
            void Set(string key, string value) => xml.Root!.Element(key)!.Value = value;
            Set("ServiceType", "Omid"); Set("StoreId", "1"); Set("ConnectionTimeOut", "30");
            Set("OMID_API_URL", "https://old-xml.invalid"); Set("OMID_API_KEY", "old-xml-key");
            xml.Save(Path.Combine(root, "CustomerID", "CustomerInformation.xml"));
            var store = new CustomerSettingsStore(root);
            var form = store.Read(includeSecrets: true);
            form.Values["OMID_API_URL"] = "https://unit-test.invalid";
            form.Values["OMID_API_KEY"] = "customer-api-key";
            form.Values["OMID_API_SECRET"] = "customer-api-secret";
            form.Values["OMID_USERNAME"] = "customer-user";
            form.Values["OMID_PASSWORD"] = " customer-password ";
            form.Values["OMID_AES_KEY_BASE64"] = Convert.ToBase64String(new byte[32]);
            form.Values["OMID_AES_IV_BASE64"] = Convert.ToBase64String(new byte[16]);
            form.Values["MEMORY_ID"] = "ABC123";
            Ensure(store.Save(form).Count == 0, "Settings form saved to local XML");

            using var file = File.OpenRead(CustomerSettingsStore.EffectivePath(root));
            var customer = (DTOServerInfo)new XmlSerializer(typeof(DTOServerInfo)).Deserialize(file)!;
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Omid:BaseUrl"] = "https://wrong-appsettings.invalid", ["Omid:ApiKey"] = "wrong-appsettings-key",
                ["Omid:Password"] = "wrong-appsettings-password"
            }).Build());
            services.AddCustomerOmidConfiguration(customer);
            using var provider = services.BuildServiceProvider();
            var config = provider.GetRequiredService<IOptions<OmidConfig>>().Value;
            Ensure(config.BaseUrl == form.Values["OMID_API_URL"] && config.ApiKey == form.Values["OMID_API_KEY"], "Client uses form's local XML, not old appsettings");
            Ensure(config.ApiSecret == form.Values["OMID_API_SECRET"] && config.AesKeyBase64 == form.Values["OMID_AES_KEY_BASE64"] && config.AesIvBase64 == form.Values["OMID_AES_IV_BASE64"], "Signing and encryption use customer settings");
            Ensure(config.UserName == form.Values["OMID_USERNAME"] && config.Password == form.Values["OMID_PASSWORD"] && config.FiscalId == "ABC123", "Login and fiscal ID use customer settings; password whitespace preserved");

            var emptyServices = new ServiceCollection();
            emptyServices.AddCustomerOmidConfiguration(new DTOServerInfo());
            using var emptyProvider = emptyServices.BuildServiceProvider();
            var empty = emptyProvider.GetRequiredService<IOptions<OmidConfig>>().Value;
            Ensure(empty.ApiKey == "" && empty.BaseUrl == "", "Missing XML settings have no fallback credentials or URL");
            var rejected = false;
            try { _ = new OmidApiClient(new HttpClient(), Options.Create(empty)); }
            catch (InvalidOperationException) { rejected = true; }
            Ensure(rejected, "Missing customer URL produces an actionable error");
            return config;
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static void Ensure(bool result, string message)
    {
        if (!result) throw new Exception("FAIL: " + message);
        Console.WriteLine("PASS: " + message);
    }
}
