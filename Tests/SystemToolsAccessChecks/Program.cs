using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using TaxWebAppCore8.BI;
using TaxWebAppCore8.Controllers;

static void Check(bool value, string message)
{
    if (!value) throw new Exception(message);
}

var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
var access = new SystemToolsAccess(config);
CustomerSettingsChecks.Run();
Check(SystemToolsAccess.ValidTarget("customer") && SystemToolsAccess.ControllerFor("customer") == "CustomerSettings", "Customer settings routing");
var session = new TestSession();
Check(!access.Verify("") && !access.HasAccess(session, "setup"), "Empty configuration fails closed");
config["SystemTools:Password"] = "Test-only-Pass_123!";
Check(access.Verify("Test-only-Pass_123!"), "Configured password accepted");
Check(!access.Verify("test-only-Pass_123!") && !access.Verify(null), "Incorrect password rejected");
Check(!access.Verify(new string('a', 1025)), "Oversized password rejected");
access.Grant(session, "setup");
Check(access.HasAccess(session, "setup") && !access.HasAccess(session, "logs"), "Grant is limited to selected tool");
Check(!session.GetString("SystemTools:setup")!.Contains("Test-only"), "No plaintext password in session");
session.SetString("SystemTools:setup:Expires", "1");
Check(!access.HasAccess(session, "setup"), "Expired grant rejected");
access.Grant(session, "setup");
config["SystemTools:Password"] = "Changed-password";
Check(!access.HasAccess(session, "setup"), "Changing password invalidates existing grants");
access.Grant(session, "setup");
access.Revoke(session, "setup");
Check(!access.HasAccess(session, "setup"), "Menu click can revoke existing grant");

AuthorizationFilterContext Context(string action, string method = "GET")
{
    var http = new DefaultHttpContext { Session = session };
    http.Request.Method = method;
    var route = new RouteData();
    route.Values["action"] = action;
    return new(new ActionContext(http, route, new ActionDescriptor()), new List<IFilterMetadata>());
}
var customerFilter = new SystemToolsPasswordFilter(access, "customer");
var customerPage = Context("Index");
customerFilter.OnAuthorization(customerPage);
Check(customerPage.Result is RedirectToActionResult, "Customer page requires password");
var customerPost = Context("Index", "POST");
customerFilter.OnAuthorization(customerPost);
Check(customerPost.Result is JsonResult { StatusCode: 401 }, "Customer save requires password");
access.Grant(session, "customer");
var authorizedCustomer = Context("Index", "POST");
customerFilter.OnAuthorization(authorizedCustomer);
Check(authorizedCustomer.Result == null && !access.HasAccess(session, "setup"), "Customer access is scoped");
access.Revoke(session, "customer");
var filter = new SystemToolsPasswordFilter(access, "setup");
var page = Context("Index");
filter.OnAuthorization(page);
Check(page.Result is RedirectToActionResult { ControllerName: "SystemTools", ActionName: "Unlock" }, "Direct page request requires password");
foreach (var (action, method) in new[] { ("Check", "GET"), ("Create", "POST") })
{
    var api = Context(action, method);
    filter.OnAuthorization(api);
    Check(api.Result is JsonResult { StatusCode: 401 }, "Unauthorized API blocked: " + action);
}
access.Grant(session, "setup");
var authorized = Context("Create", "POST");
filter.OnAuthorization(authorized);
Check(authorized.Result == null, "Authenticated tool request allowed");
var controller = new SystemToolsController(access)
{
    ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { Session = session } }
};
Check(controller.Unlock("https://example.com") is NotFoundResult, "Untrusted redirect target rejected");
controller.Unlock("setup");
Check(!access.HasAccess(session, "setup"), "Fresh menu click asks again");
Check(controller.Unlock("setup", "wrong") is ViewResult && !access.HasAccess(session, "setup"), "Failed login grants no access");
Check(controller.Unlock("setup", "Changed-password") is RedirectToActionResult { ControllerName: "OmidSetup" }, "Successful login redirects to fixed target");
Check(access.HasAccess(session, "setup"), "Successful login grants access");
var post = typeof(SystemToolsController).GetMethod("Unlock", new[] { typeof(string), typeof(string) })!;
Check(Attribute.IsDefined(post, typeof(ValidateAntiForgeryTokenAttribute)), "Login is protected against CSRF");
config["SystemTools:Password"] = "";
Check(!access.HasAccess(session, "setup"), "Removing password disables previous grants");
Console.WriteLine("All system tools password checks passed. No database or production settings used.");

sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> values = new();
    public bool IsAvailable => true;
    public string Id => "test";
    public IEnumerable<string> Keys => values.Keys;
    public void Clear() => values.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => values.Remove(key);
    public void Set(string key, byte[] value) => values[key] = value;
    public bool TryGetValue(string key, out byte[] value) => values.TryGetValue(key, out value!);
}
