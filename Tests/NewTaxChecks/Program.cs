using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using NewTaxSdk.Abstraction;
using NewTaxSdk.Abstraction.Signatory;
using NewTaxSdk.Dto.Config;
using NewTaxSdk.Dto.Content;
using NewTaxSdk.Dto.Properties;
using NewTaxSdk.Dto.Transfer;
using NewTaxSdk.Enums;
using NewTaxSdk.Exceptions;
using NewTaxSdk.Extensions;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using TaxWebAppCore8.BI;
using TaxWebAppCore8.BI.NewTax;
using TaxWebAppCore8.Models.DBContextMedels;
using TaxWebAppCore8.Models.SharedDTO;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    checks++;
    Console.WriteLine("PASS " + name);
}
void Throws(Action action, string name)
{
    try { action(); } catch (InvalidOperationException) { Check(true, name); return; }
    throw new Exception(name);
}
var settings = new DTOServerInfo { MEMORY_ID = "A1119R", TINS = "1234567890",
    NEWTAX_API_URL = "https://example.invalid/req/api", NEWTAX_PRIVATE_KEY_PATH = "fixture.pem" };
Invoice Fixture() => new()
{
    Header = new TaxInvoiceHeaderSysDTO { InvoiceDate = new DateTime(2026, 10, 27, 12, 0, 0),
        Inno = "123", Inty = 1, Inp = 1, Ins = 1, Bpc = "1234567890", Bbc = "42",
        Tprdis = 100.50m, Tadis = 100.50m, Tbill = 110.50m, Tvam = 10m },
    Rows = new() { new TaxCollectData.Library.Dto.BodyItemDto { sstid = "1234567890123", vam = 10, fee = 100.50m } }
};
var mapped = NewTaxInvoiceMapper.Map(Fixture(), settings, false);
Check(mapped.Header.Inno == "000000007B", "Numeric invoice serial is hexadecimal");
Check(mapped.Header.Bpc == "1234567890" && mapped.Header.Bbc == "42", "Buyer postcode and branch are independent");
Check(mapped.Header.Tprdis == 100.50m && mapped.Body[0].Fee == 100.50m && mapped.Body[0].Vam == 10m, "Mapping preserves decimal amounts and body tax");
Check(mapped.Header.Indatim == new DateTimeOffset(2026,10,27,8,30,0,TimeSpan.Zero).ToUnixTimeMilliseconds(), "Iranian unspecified date uses Iran offset");
Check(mapped.Header.Taxid == new TaxCollectData.Library.Providers.TaxIdProvider(new TaxCollectData.Library.Algorithms.VerhoeffAlgorithm())
    .GenerateTaxId(settings.MEMORY_ID, 123, new DateTime(2026,10,27,8,30,0,DateTimeKind.Utc)), "Tax ID agrees with existing algorithm for fixed input");
var bad = Fixture(); bad.Header.Tvam = 1;
Throws(() => NewTaxInvoiceMapper.Map(bad, settings, false), "Tax mismatch blocks payload creation");
bad = Fixture(); bad.Header.InvoiceDate = null;
Throws(() => NewTaxInvoiceMapper.Map(bad, settings, false), "Missing date is rejected");
Throws(() => NewTaxInvoiceMapper.Map(Fixture(), settings, true), "Cancellation needs reference tax ID");
var cancel = Fixture(); cancel.Header.Irtaxid = "reference-tax-id";
Check(NewTaxInvoiceMapper.Map(cancel, settings, true).Header.Ins == 3, "Cancellation uses subject 3");
settings.NEWTAX_API_URL = "http://example.invalid";
Throws(() => NewTaxConfiguration.From(settings), "NewTax requires HTTPS");
settings.NEWTAX_API_URL = "https://example.invalid/req/api/";

var context = DispatchProxy.Create<IContext, ContextProxy>();
var proxy = (ContextProxy)(object)context;
proxy.Invoice = Fixture();
var client = new FakeClient();
var store = new FakeStore();
var service = new NewTaxService(context, settings, client, store);
var id = Guid.NewGuid().ToString();
var request = new DTOInvoiceLimitParameter { InvoiceID = id };
proxy.Invoice.Header.Tvam = 0;
Check(!(await service.SendInvoiceAsync(request)).Success && client.Sends == 0, "Invalid invoice never reaches client");
proxy.Invoice = Fixture();
var result = await service.SendInvoiceAsync(request);
Check(result.Success && client.Sends == 1 && store.Sent == 1 && store.LastId == Guid.Parse(id), "Accepted invoice persists against original invoice ID");
store.FailSent = true;
result = await service.SendInvoiceAsync(request);
Check(!result.Success && result.Status == "SENT_SAVE_FAILED" && result.Data is PacketResponse, "Save failure retains acknowledgement and is not reported as successful");
store.FailSent = false;
proxy.History = new DTOTaxHistory { ReferenceNumber = client.Reference };
client.Status = "SUCCESS";
result = await service.GetInvoiceStatusByRefrenceAsync(id);
Check(result.Success && store.State == 4, "Successful inquiry is final success");
client.Status = "FAILED";
result = await service.GetInvoiceStatusByRefrenceAsync(id);
Check(result.Message.Contains("fixture error") && store.State == 3, "SDK msg error is shown for rejected invoice");
client.Status = "TIMEOUT";
await service.GetInvoiceStatusByRefrenceAsync(id);
Check(store.State == 2, "Timeout keeps unresolved state");
client.Mismatch = true;
var savesBefore = store.Inquiries;
Check(!(await service.GetInvoiceStatusByRefrenceAsync(id)).Success && store.Inquiries == savesBefore, "Unmatched reference cannot update invoice status");
client.Mismatch = false;
var dateResults = await service.GetInvoiceStatusByDateRange("1405/08/05", "1405/08/06");
Check(dateResults.Count == 1 && dateResults[0].ReferenceNumber == client.Reference, "Date inquiry adapts SDK responses to the existing controller model");

// Routing does not fall through to the old Tax singleton; local lists need no network initialization.
BMain.SrvInfo = settings; settings.ServiceType = " newtax "; BMain._newTaxService = service;
BMain._taxService = null!;
await BMain.GetAllInvoiceAsync(new SearchParameters());
Check(client.Initializations == 0, "NewTax local list routes without initializing old or new network client");

var filter = new NewTaxPageFilter { FromDate = "۱۴۰۵/۰۸/۰۱", ToDate = "1405/08/05", StoreIds = new() { 2, 2, -1 } };
var search = filter.ToSearch();
Check(search.DateFrom < search.DateTo && search.StorIDs!.SequenceEqual(new[] { 2 }) && search.Status == null, "NewTax page filters accept Persian dates and all statuses");
filter.FromDate = "1405/08/20";
try { filter.ToSearch(); throw new Exception("Inverted dates were accepted"); }
catch (ArgumentException) { Check(true, "NewTax page filters reject inverted dates"); }
var page = new TaxWebAppCore8.Controllers.NewTaxController(Microsoft.Extensions.Logging.Abstractions.NullLogger<TaxWebAppCore8.Controllers.NewTaxController>.Instance);
Check(page.Send() is Microsoft.AspNetCore.Mvc.ViewResult { ViewName: "Invoices" } && page.ViewData["Inquiry"] is false, "NewTax send uses dedicated invoice page");
Check(page.Inquiry() is Microsoft.AspNetCore.Mvc.ViewResult { ViewName: "Invoices" } && page.ViewData["Inquiry"] is true, "NewTax inquiry uses dedicated invoice page");
settings.ServiceType = "Tax";
var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(new Microsoft.AspNetCore.Http.DefaultHttpContext(), new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
var executing = new Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext(actionContext, new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>(), new Dictionary<string, object?>(), page);
page.OnActionExecuting(executing);
Check(executing.Result is Microsoft.AspNetCore.Mvc.NotFoundResult, "NewTax pages and operations are unavailable in old Tax mode");
settings.ServiceType = "NewTax";

var folder = Path.Combine(Path.GetTempPath(), "newtax-check-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
try
{
    var keyPath = Path.Combine(folder, "key.pem");
    using var rsa = RSA.Create(2048);
    File.WriteAllText(keyPath, rsa.ExportPkcs8PrivateKeyPem());
    var services = new ServiceCollection();
    services.AddTaxApi("https://example.invalid/req/api/", settings.MEMORY_ID,
        new NormalProperties(ClientType.SELF_TSP), new Pkcs8SignatoryConfig(keyPath, null));
    var tlsCallback = System.Net.ServicePointManager.ServerCertificateValidationCallback;
    using var provider = services.BuildServiceProvider();
    var signer = provider.GetRequiredService<ITransferSignatory>();
    var signature = signer.Sign("certificate-free-fixture");
    Check(rsa.VerifyData(Encoding.UTF8.GetBytes("certificate-free-fixture"), Convert.FromBase64String(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1), "PEM signs a verifiable request without a certificate");
    var sender = provider.GetRequiredService<IHttpRequestSender>();
    Check(ReferenceEquals(tlsCallback, System.Net.ServicePointManager.ServerCertificateValidationCallback), "SDK construction leaves global TLS validation unchanged");
    var senderType = typeof(IHttpRequestSender).Assembly.GetType("NewTaxSdk.Business.RestSharpHttpRequestSender")!;
    using var http = new HttpClient(new UnauthorizedHandler()) { BaseAddress = new Uri("https://example.invalid") };
    var fakeSender = (IHttpRequestSender)Activator.CreateInstance(senderType, http)!;
    try { await fakeSender.SendPostRequestAsync<object>("/fixture", "{}", new()); throw new Exception("401 did not propagate"); }
    catch (TaxHttpException ex) { Check(ex.HttpStatusCode == 401, "HTTP 401 is preserved for token handling"); }

    settings.NEWTAX_PRIVATE_KEY_PATH = keyPath;
    var sdkSender = new FakeSdkSender(Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()));
    using var sdkClient = new NewTaxClient(settings, services => services.AddSingleton<IHttpRequestSender>(sdkSender));
    await sdkClient.InitializeAsync();
    await sdkClient.InitializeAsync();
    Check(sdkSender.ServerRequests == 1 && sdkSender.TokenRequests == 1, "Client caches server information and token");
    sdkSender.RejectInquiryOnce = true;
    await sdkClient.InquiryAsync(sdkSender.Reference);
    Check(sdkSender.TokenRequests == 2 && sdkSender.InquiryRequests == 2, "Read retries exactly once with a fresh token after 401");
    await sdkClient.SendAsync(mapped);
    Check(sdkSender.SendRequests == 1 && sdkSender.SawEncryptedPacket, "Client sends one signed encrypted packet and accepts acknowledgement");
    sdkSender.RejectSend = true;
    try { await sdkClient.SendAsync(mapped); throw new Exception("Rejected send did not fail"); }
    catch (TaxHttpException) { Check(sdkSender.SendRequests == 2, "Client does not replay an invoice after 401"); }

    Directory.CreateDirectory(Path.Combine(folder, "CustomerID"));
    var xml = new XDocument(new XElement("DTOServerInfo", CustomerSettingsStore.Fields.Select(f => new XElement(f.Key, "fixture"))));
    void Set(string name, string value) => xml.Root!.Element(name)!.Value = value;
    Set("ServiceType", "Tax"); Set("API_URL", "https://old.invalid"); Set("FILE_KEY_PRIVATE", "old-key.pem");
    Set("FILE_CERTIFICATE", "old-certificate.cer"); Set("ConnectionTimeOut", "30"); Set("StoreId", "-1");
    Set("MEMORY_ID", "A1119R"); Set("NEWTAX_API_URL", "https://example.invalid/req/api/"); Set("NEWTAX_PRIVATE_KEY_PATH", keyPath);
    xml.Save(Path.Combine(folder, "CustomerID", "CustomerInformation.xml"));
    var settingsStore = new CustomerSettingsStore(folder);
    var form = settingsStore.Read(); form.Values["ServiceType"] = "NewTax";
    form.Values["API_URL"] = "overwrite-attempt"; form.Values["FILE_CERTIFICATE"] = "overwrite-attempt";
    Check(settingsStore.Save(form).Count == 0, "NewTax settings save without a certificate");
    var saved = XDocument.Load(CustomerSettingsStore.EffectivePath(folder));
    Check(saved.Root!.Element("API_URL")!.Value == "https://old.invalid" && saved.Root.Element("FILE_CERTIFICATE")!.Value == "old-certificate.cer"
        && saved.Root.Element("FILE_KEY_PRIVATE")!.Value == "old-key.pem", "Switching to NewTax preserves old Tax settings");
    form = settingsStore.Read(); form.Values["NEWTAX_API_URL"] = "http://example.invalid";
    Check(settingsStore.Save(form).Count > 0, "Settings reject insecure NewTax URL");
}
finally { Directory.Delete(folder, true); }
var setup = NewTaxSetupCatalog.Objects;
Check(setup.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == setup.Count, "NewTax setup object names are unique");
Check(!setup.Any(x => x.Name.StartsWith("Omid", StringComparison.OrdinalIgnoreCase)), "NewTax setup excludes Omid-specific tables");
Check(setup.SelectMany(x => x.Dependencies ?? []).All(name => setup.Any(x => x.Name == name)), "NewTax setup dependencies are complete");
Check(new[] { "TaxHistory", "tTaxHistoryStateParallel", "tSendInvoiceTaxTissLog", "TaxGovermentInvoiceInfo", "TaxGovermentInvoiceFinal", "TaxGovermentSendLog", "vwTaxHistory", "vw_TaxGovermentInvoiceHeaderForShow" }.All(name => setup.Any(x => x.Name == name)), "NewTax read and state-store objects are covered");
Check(setup.Single(x => x.Name == "TaxGovermentSaveInvoiceInsert").ArgumentCount == 5
    && setup.Single(x => x.Name == "TaxGovermentInvoiceFinalInsertUpdate").ArgumentCount == 6
    && setup.Single(x => x.Name == "TaxGovermentSaveInvoiceLogInsert").ArgumentCount == 5, "NewTax save procedure arities match the state store");
Check(setup.Single(x => x.Name == "UV_TaxInfo_BulkSale").Columns.Length > 3, "NewTax bulk source columns come from the reviewed detail SQL");
Check(SystemToolsAccess.ValidTarget("newtax-setup") && SystemToolsAccess.ControllerFor("newtax-setup") == "NewTaxSetup"
    && SystemToolsAccess.ControllerFor("setup") == "OmidSetup", "NewTax setup has an independent password-protected route");
Console.WriteLine($"{checks} NewTax checks passed; no live network or database used.");

public class ContextProxy : DispatchProxy
{
    public Invoice Invoice = null!;
    public DTOTaxHistory History = null!;
    protected override object? Invoke(MethodInfo? method, object?[]? args) => method!.Name switch
    {
        "GetLastInvoiceForSend" => Invoice,
        "GetTaxHistory" => History,
        "GetAllInvoice" => Task.FromResult(new List<GeneralInvoiceHeaderViewModel>()),
        _ => throw new Exception("Unexpected context call: " + method.Name)
    };
}
public sealed class FakeClient : INewTaxClient
{
    public int Sends, Initializations;
    public string Reference = Guid.NewGuid().ToString(), Status = "SUCCESS";
    public bool Mismatch;
    public Task InitializeAsync() { Initializations++; return Task.CompletedTask; }
    public Task<bool> CheckConnectionAsync() => Task.FromResult(true);
    public Task<PacketResponse> SendAsync(InvoiceDto invoice) { Sends++; return Task.FromResult(new PacketResponse(Guid.NewGuid().ToString(), Reference, null!, null!)); }
    public Task<List<InquiryResultModel>> InquiryAsync(string reference) => Task.FromResult(new List<InquiryResultModel>
    { new(Mismatch ? "other-reference" : reference, "uid", Status, JsonDocument.Parse("{\"error\":[{\"code\":\"42\",\"msg\":\"fixture error\"}]}").RootElement.Clone(), "INVOICE", "A1119R") });
    public Task<List<InquiryResultModel>> InquiryByDateAsync(string from, string to) => InquiryAsync(Reference);
}
public sealed class FakeStore : INewTaxStateStore
{
    public int Sent, Inquiries, State;
    public Guid LastId;
    public bool FailSent;
    public Task SaveSentAsync(Guid id, PacketResponse packet, InvoiceDto invoice, bool cancel)
    { if (FailSent) throw new IOException("fixture write failure"); Sent++; LastId = id; return Task.CompletedTask; }
    public Task SaveInquiryAsync(Guid id, InquiryResultModel response, int state, string message, string codes)
    { Inquiries++; State = state; return Task.CompletedTask; }
}
public sealed class UnauthorizedHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized));
}
public sealed class FakeSdkSender(string publicKey) : IHttpRequestSender
{
    public int ServerRequests, TokenRequests, InquiryRequests, SendRequests;
    public bool RejectInquiryOnce, RejectSend, SawEncryptedPacket;
    public string Reference = Guid.NewGuid().ToString();
    public Task<NewTaxSdk.Dto.HttpResponse<T?>> SendPostRequestAsync<T>(string url, string body, Dictionary<string, string> headers)
    {
        object? data;
        if (url.Contains("GET_SERVER_INFORMATION"))
        {
            ServerRequests++;
            data = new { serverTime = 0, publicKeys = new[] { new { key = publicKey, id = "server-key", algorithm = "RSA", purpose = 1 } } };
        }
        else if (url.Contains("GET_TOKEN"))
        {
            TokenRequests++; data = new { token = "fixture-token", expiresIn = 3600 };
        }
        else if (url.Contains("INQUIRY_BY_REFERENCE_NUMBER"))
        {
            InquiryRequests++;
            if (RejectInquiryOnce) { RejectInquiryOnce = false; throw new TaxHttpException(401); }
            data = new[] { new { referenceNumber = Reference, uid = "uid", status = "SUCCESS", data = new { error = Array.Empty<object>(), warning = Array.Empty<object>() }, packetType = "INVOICE", fiscalId = "A1119R" } };
        }
        else if (url.Contains("normal-enqueue"))
        {
            SendRequests++;
            if (RejectSend) throw new TaxHttpException(401);
            using var json = JsonDocument.Parse(body);
            SawEncryptedPacket = body.Contains("symmetricKey") && body.Contains("dataSignature") && body.Contains("server-key") && !body.Contains("\"symmetricKey\":\"error\"");
            var accepted = new { timestamp = 0, result = new[] { new { uid = "uid", referenceNumber = Reference, errorCode = (string?)null, errorDetail = (string?)null } }, errors = Array.Empty<object>() };
            return Respond<T>(accepted);
        }
        else throw new Exception("Unexpected SDK URL " + url);
        return Respond<T>(new { timestamp = 0, result = new { uid = "uid", packetType = "fixture", data }, errors = Array.Empty<object>() });
    }
    private static Task<NewTaxSdk.Dto.HttpResponse<T?>> Respond<T>(object value)
    {
        var response = System.Text.Json.JsonSerializer.Deserialize<T>(System.Text.Json.JsonSerializer.Serialize(value), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return Task.FromResult(new NewTaxSdk.Dto.HttpResponse<T?>(response, 200));
    }
}
