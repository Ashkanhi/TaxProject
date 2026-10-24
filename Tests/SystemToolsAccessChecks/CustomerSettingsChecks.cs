using System.Xml.Linq;
using System.Xml.Serialization;
using TaxServiceAndApp.Models.SharedDTO;
using TaxWebAppCore8.BI;

internal static class CustomerSettingsChecks
{
    public static void Run()
    {
        var root = Path.Combine(Path.GetTempPath(), "customer-settings-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "CustomerID"));
        try
        {
            var path = CustomerSettingsStore.EffectivePath(root);
            var doc = new XDocument(new XElement("DTOServerInfo",
                CustomerSettingsStore.Fields.Select(f => new XElement(f.Key, f.Secret ? "fixture-secret" : "fixture")),
                new XComment("preserve this"), new XElement("License", "fixture-license")));
            void Set(string key, string value) => doc.Root!.Element(key)!.Value = value;
            Set("ServiceType", "Omid"); Set("ConnectionTimeOut", "30"); Set("StoreId", "-1");
            Set("OMID_API_URL", "https://example.invalid"); Set("API_URL", "https://example.invalid");
            doc.Save(path);
            var original = File.ReadAllText(path);
            var store = new CustomerSettingsStore(root);
            var first = store.Read();
            Assert(first.Values["UserPass"] == "", "No secret in read model");
            Assert(store.Read(includeSecrets: true).Values["OMID_API_SECRET"] == "fixture-secret", "Authorized editor can display saved secrets");
            first.Values["ServiceType"] = "Tax";
            first.Values["CustomerName"] = "مشتری آزمایشی & نمونه";
            Assert(store.Save(first).Count == 0, "Initial save");
            var effective = CustomerSettingsStore.EffectivePath(root);
            Assert(effective.EndsWith(".local.xml"), "Startup reads override");
            Assert(File.ReadAllText(path) == original, "Distributed template unchanged");
            var saved = XDocument.Load(effective);
            Assert(saved.Root!.Element("UserPass")!.Value == "fixture-secret", "Blank secret preserved");
            Assert(saved.Root.Element("License")!.Value == "fixture-license" && saved.DescendantNodes().OfType<XComment>().Any(), "Unknown elements and comments retained");
            using (var reader = File.OpenRead(effective))
            {
                var dto = (DTOServerInfo)new XmlSerializer(typeof(DTOServerInfo)).Deserialize(reader)!;
                Assert(dto.ServiceType == "Tax" && dto.ConnectionTimeOut == 30, "Startup serializer compatibility");
            }
            Assert(store.Save(first).Count > 0, "Stale form rejected");
            var invalid = store.Read();
            invalid.Values["ServiceType"] = "invalid";
            invalid.Values["ConnectionTimeOut"] = "abc";
            var before = File.ReadAllText(effective);
            Assert(store.Save(invalid).Count >= 2 && File.ReadAllText(effective) == before, "Invalid values do not modify disk");
            var next = store.Read();
            next.Values["UserPass"] = "replacement-secret";
            Assert(store.Save(next).Count == 0, "Replace existing override");
            Assert(File.ReadAllText(effective + ".bak") == before, "Backup previous settings");
            Assert(XDocument.Load(effective).Root!.Element("UserPass")!.Value == "replacement-secret", "Secret updated");
            var switching = store.Read(includeSecrets: true);
            switching.Values["ServiceType"] = "Omid";
            foreach (var field in CustomerSettingsStore.Fields.Where(f => !f.AppliesTo("Omid")))
                switching.Values.Remove(field.Key);
            Assert(store.Save(switching).Count == 0, "Inactive fields may be omitted");
            var omid = store.Read(includeSecrets: true);
            omid.Values["API_URL"] = "malicious-inactive-change";
            Assert(store.Save(omid).Count == 0 && XDocument.Load(effective).Root!.Element("API_URL")!.Value == "https://example.invalid", "Inactive service values are preserved server-side");
            var missing = store.Read();
            missing.Values.Remove("OMID_API_URL");
            Assert(store.Save(missing).Count > 0, "Missing active fields rejected");
            var connectionValues = new Dictionary<string, string>
            {
                ["DataBaseServer"] = "localhost", ["DbName"] = "fixture;Database=other",
                ["UserId"] = "test", ["ConnectionTimeOut"] = "120", ["UserPass"] = ""
            };
            var connection = new System.Data.SqlClient.SqlConnectionStringBuilder(SettingsDatabaseConnection.Build(connectionValues, "saved;password"));
            Assert(connection.InitialCatalog == "fixture;Database=other" && connection.Password == "saved;password", "Connection values escaped and saved password fallback used");
            Assert(connection.ConnectTimeout == 10 && !connection.Pooling && !connection.PersistSecurityInfo, "Connection test is bounded and does not pool credentials");
            connectionValues["UserPass"] = "new-password";
            Assert(new System.Data.SqlClient.SqlConnectionStringBuilder(SettingsDatabaseConnection.Build(connectionValues, "old")).Password == "new-password", "Test uses unsaved password");
            Console.WriteLine("Customer settings checks passed.");
        }
        finally { Directory.Delete(root, true); }
    }

    private static void Assert(bool result, string message)
    {
        if (!result) throw new Exception(message);
    }
}
