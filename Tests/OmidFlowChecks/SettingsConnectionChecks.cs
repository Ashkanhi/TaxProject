using Microsoft.Extensions.Options;
using TaxWebAppCore8.BI.Omid;

internal static class SettingsConnectionChecks
{
    public static async Task RunAsync(OmidConfig active)
    {
        var saved = new Dictionary<string, string>
        {
            ["OMID_API_URL"] = active.BaseUrl, ["OMID_USERNAME"] = active.UserName,
            ["OMID_PASSWORD"] = active.Password, ["OMID_API_KEY"] = active.ApiKey,
            ["OMID_API_SECRET"] = active.ApiSecret, ["OMID_AES_KEY_BASE64"] = active.AesKeyBase64,
            ["OMID_AES_IV_BASE64"] = active.AesIvBase64
        };
        var unsaved = new Dictionary<string, string>(saved)
        {
            ["OMID_API_URL"] = "https://unsaved-form.invalid", ["OMID_USERNAME"] = "unsaved-user",
            ["OMID_PASSWORD"] = "unsaved-password", ["OMID_API_KEY"] = "unsaved-key",
            ["OMID_API_SECRET"] = "unsaved-secret",
            ["OMID_AES_KEY_BASE64"] = Convert.ToBase64String(Enumerable.Repeat((byte)5, 32).ToArray()),
            ["OMID_AES_IV_BASE64"] = Convert.ToBase64String(Enumerable.Repeat((byte)8, 16).ToArray())
        };
        var testConfig = OmidSettingsConnectionTest.BuildConfig(unsaved, saved);
        var handler = new FakeOmidHandler(testConfig, []);
        var token = await new OmidApiClient(new HttpClient(handler), Options.Create(testConfig)).LoginAsync();
        Ensure(token == "fake-token" && handler.LoginCount == 1 && handler.Requests.Count == 0, "Unsaved form authenticates using new keys and encryption; no invoice sent");
        Ensure(active.ApiKey == saved["OMID_API_KEY"] && active.UserName == saved["OMID_USERNAME"], "Testing does not modify the active configuration");
        var fallback = new Dictionary<string, string>(unsaved) { ["OMID_PASSWORD"] = "", ["OMID_API_KEY"] = "" };
        var withSavedSecrets = OmidSettingsConnectionTest.BuildConfig(fallback, saved);
        Ensure(withSavedSecrets.Password == active.Password && withSavedSecrets.ApiKey == active.ApiKey, "Empty secret inputs preserve saved values as the form promises");
        Reject(new Dictionary<string, string>(unsaved) { ["OMID_AES_KEY_BASE64"] = "bad-base64" }, saved);
        Reject(new Dictionary<string, string>(unsaved) { ["OMID_AES_IV_BASE64"] = Convert.ToBase64String(new byte[8]) }, saved);
        Reject(new Dictionary<string, string>(unsaved) { ["OMID_API_KEY"] = "secret\r\nheader" }, saved);
        Reject(new Dictionary<string, string>(unsaved) { ["OMID_USERNAME"] = "" }, saved);
        Reject(new Dictionary<string, string>(unsaved) { ["OMID_API_URL"] = "" }, saved);
        Console.WriteLine("PASS: Invalid form settings are rejected before network requests");
    }

    private static void Reject(Dictionary<string, string> form, Dictionary<string, string> saved)
    {
        try { OmidSettingsConnectionTest.BuildConfig(form, saved); }
        catch (OmidSettingsValidationException) { return; }
        throw new Exception("FAIL: Invalid settings were accepted");
    }

    private static void Ensure(bool result, string message)
    {
        if (!result) throw new Exception("FAIL: " + message);
        Console.WriteLine("PASS: " + message);
    }
}
