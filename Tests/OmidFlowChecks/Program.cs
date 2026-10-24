using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using TaxWebAppCore8.BI.Omid;

const string a = "006135e5-f7aa-4f0f-93e8-6b35cd5ffa7b";
const string b = "33d148b1-f2f3-4ea5-90cd-1ddde57500f3";
void Check(bool condition, string label)
{
    if (!condition) throw new Exception("FAIL: " + label);
    Console.WriteLine("PASS: " + label);
}
string Envelope(params JObject[] rows) => new JObject { ["success"] = true, ["data"] = new JArray(rows) }.ToString();
JObject Row(string id, string status, bool? success = null) => new JObject
{
    ["requestId"] = id, ["invoiceStatus"] = status,
    ["inquiryResultModel"] = new JObject { ["data"] = new JObject { ["success"] = success, ["error"] = null } }
};
var unknown = OmidInquiryResponseParser.Parse(Envelope(Row(Guid.Empty.ToString(), "", false)), new[] { a });
Check(!unknown[a].IsFinal, "placeholder false is not a rejection");
Check(!OmidInquiryResponseParser.Parse(Envelope(Row(a, "", false)), new[] { a })[a].IsFinal,
    "false without status/errors remains pending");
Check(!OmidInquiryResponseParser.Parse(Envelope(Row(a, "PENDING", false)), new[] { a })[a].IsFinal,
    "explicit pending remains pending");
Check(OmidInquiryResponseParser.Parse(Envelope(Row(a, "REJECTED")), new[] { a })[a].State == "Failed",
    "explicit rejected needs no success flag");
Check(OmidInquiryResponseParser.Parse(Envelope(Row(a, "ACCEPTED", true)), new[] { b })[b].State == "Pending",
    "foreign result is not assigned to selected invoice");
Check(OmidInquiryResponseParser.Parse("{\"success\":true,\"data\":null}", new[] { a })[a].State == "Pending",
    "null data stays pending");
var mixed = OmidInquiryResponseParser.Parse(Envelope(Row(b, "REJECTED"), Row(a, "ACCEPTED", true)), new[] { a, b });
Check(mixed[a].State == "Accepted" && mixed[b].State == "Failed", "out of order mixed results matched by requestId");
var duplicateRejected = false;
try { OmidInquiryResponseParser.Parse(Envelope(Row(a, "ACCEPTED", true), Row(a, "REJECTED")), new[] { a }); }
catch (InvalidOperationException) { duplicateRejected = true; }
Check(duplicateRejected, "duplicate receipt cannot overwrite another result");

string validation = "{\"success\":false,\"message\":\"Internal Error\",\"errors\":[{\"code\":\"INVALID_VALUE\",\"field\":\"mu\"}]}";
Check(OmidSendFailureClassifier.TryGetDefinitiveRejection(new OmidApiException(HttpStatusCode.InternalServerError, validation), out _),
    "structured validation rejection recognized");
Check(!OmidSendFailureClassifier.TryGetDefinitiveRejection(new OmidApiException(HttpStatusCode.InternalServerError,
    "{\"success\":false,\"message\":\"Internal Error\",\"errors\":null}"), out _), "generic server error stays unknown");
Check(!OmidSendFailureClassifier.TryGetDefinitiveRejection(new HttpRequestException("lost connection"), out _), "connection loss stays unknown");
var business = OmidSendResponseParser.Parse("{\"success\":true,\"data\":" + validation + "}");
Check(!business.Success && OmidSendFailureClassifier.TryGetDefinitiveRejection(
    new OmidSendBusinessException(business.Message, business.ResponseJson), out _), "HTTP 200 validation uses same classification");
Check(!OmidSendFailureClassifier.TryGetDefinitiveRejection(new OmidSendBusinessException("duplicate",
    "{\"success\":true,\"data\":{\"success\":false,\"statusCode\":409,\"message\":\"duplicate\"}}"), out _),
    "duplicate invoice number requires reconciliation");

// کل شبکه ساختگی است؛ رمزنگاری و بدنهٔ کلاینت واقعی را بررسی می‌کنیم.
var config = CustomerConfigurationChecks.Run();
await SettingsConnectionChecks.RunAsync(config);
var handler = new FakeOmidHandler(config, new[] { Envelope(Row(a, "ACCEPTED", true)), Envelope(Row(b, "REJECTED")) });
var client = new OmidApiClient(new HttpClient(handler), Options.Create(config));
var savedAttempts = new List<Dictionary<string, OmidInquiryItemResult>>();
Console.WriteLine("Checking two-invoice retry using the real 30-second interval...");
var result = await client.InquiryWithRetryAsync(new[] { a, b }, onAttemptCompleted: (items, attempt) =>
{
    savedAttempts.Add(items.ToDictionary(pair => pair.Key, pair => pair.Value));
    return Task.CompletedTask;
});
Check(handler.LoginCount == 1 && handler.Requests.Count == 2, "one login, two inquiries, no send");
Check(handler.Requests[0].SequenceEqual(new[] { a, b }) && handler.Requests[1].SequenceEqual(new[] { b }),
    "retry contains only unfinished invoice");
Check(savedAttempts[0][a].IsFinal && !savedAttempts[0][b].IsFinal && savedAttempts[1].Count == 1,
    "per-attempt persistence receives partial results immediately");
Check(result.HasFinalResult && result.Attempts == 2 && result.Items[a].State == "Accepted" && result.Items[b].State == "Failed",
    "both results retained across attempts");
Check(JObject.Parse(result.ResponseJson)["data"]!.Count() == 2 && result.FinalSuccess == null,
    "mixed final statuses are not presented as one shared status");
Check(OmidApiTraceLogger.Count == 4, "every HTTP attempt invokes trace logging");

using var cts = new CancellationTokenSource();
var cancelHandler = new FakeOmidHandler(config, new[] { "{\"success\":true,\"data\":null}" });
bool persistedBeforeCancel = false;
try
{
    await new OmidApiClient(new HttpClient(cancelHandler), Options.Create(config)).InquiryWithRetryAsync(new[] { a }, cts.Token,
        (items, attempt) => { persistedBeforeCancel = true; cts.Cancel(); return Task.CompletedTask; });
}
catch (OperationCanceledException) { }
Check(persistedBeforeCancel && cancelHandler.Requests.Count == 1, "cancellation stops retry after current result is delivered for storage");
Console.WriteLine("All Omid flow checks passed. No real Omid API called.");

sealed class FakeOmidHandler(OmidConfig config, IEnumerable<string> responses) : HttpMessageHandler
{
    private readonly Queue<string> responses = new(responses);
    public int LoginCount { get; private set; }
    public List<string[]> Requests { get; } = new();
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.RequestUri!.Host != new Uri(config.BaseUrl).Host || request.Headers.GetValues("X-Api-Key").Single() != config.ApiKey)
            throw new Exception("Customer URL or API key not used");
        var body = await request.Content!.ReadAsStringAsync(ct);
        var signature = OmidCryptoHelper.GenerateSignature(config.ApiSecret, body,
            request.Headers.GetValues("X-Timestamp").Single(), request.Headers.GetValues("X-Nonce").Single());
        if (request.Headers.GetValues("X-Signature").Single() != signature) throw new Exception("Customer signing secret not used");
        var wirePayload = JObject.Parse(body);
        using var customerAes = Aes.Create();
        customerAes.Key = Convert.FromBase64String(config.AesKeyBase64);
        customerAes.IV = Convert.FromBase64String(config.AesIvBase64);
        using var customerDecryptor = customerAes.CreateDecryptor();
        var ciphertext = Convert.FromBase64String(wirePayload["data"]!.Value<string>()!);
        var customerPayload = JObject.Parse(Encoding.UTF8.GetString(customerDecryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length)));
        string response;
        if (request.RequestUri!.AbsolutePath == "/api/Auth/login")
        {
            if (customerPayload["username"]?.Value<string>() != config.UserName || customerPayload["password"]?.Value<string>() != config.Password)
                throw new Exception("Customer login credentials not used");
            LoginCount++;
            response = "{\"success\":true,\"data\":{\"token\":\"fake-token\"}}";
        }
        else
        {
            if (request.RequestUri.AbsolutePath != "/api/Invoice/Invoiceinquiry") throw new Exception("Unexpected send");
            var wire = JObject.Parse(await request.Content!.ReadAsStringAsync(ct));
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(config.AesKeyBase64); aes.IV = Convert.FromBase64String(config.AesIvBase64);
            var bytes = Convert.FromBase64String(wire["data"]!.Value<string>()!);
            using var decryptor = aes.CreateDecryptor();
            string plain = Encoding.UTF8.GetString(decryptor.TransformFinalBlock(bytes, 0, bytes.Length));
            Requests.Add(JObject.Parse(plain)["requestIds"]!.Values<string>().Select(x => x!).ToArray());
            response = responses.Dequeue();
        }
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) };
    }
}

namespace TaxWebAppCore8.BI.Omid
{
    // این تست تنها استعلام را اجرا می‌کند؛ ارسال واقعی عمداً قابل اجرا نیست.
    internal static class OmidTaxService
    {
        public static void ValidateForSend(OmidSendInvoiceRequest request) => throw new NotSupportedException("No real send in tests");
    }
    internal static class OmidApiTraceLogger
    {
        public static int Count;
        public static void TryWrite(string path, string? plain, string? wire, int? http, string? response, Exception? error) => Count++;
    }
}
