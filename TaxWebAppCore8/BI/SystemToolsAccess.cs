using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaxWebAppCore8.BI;

public sealed class SystemToolsAccess(IConfiguration configuration)
{
    private string? Password => configuration["SystemTools:Password"];
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Password);
    public static bool ValidTarget(string? target) => target is "setup" or "logs";
    public static string ControllerFor(string target) => target == "setup" ? "OmidSetup" : "SystemLogs";
    private static string Key(string target) => "SystemTools:" + target;
    private static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));

    public bool Verify(string? supplied) => IsConfigured && supplied != null && supplied.Length <= 1024 &&
        CryptographicOperations.FixedTimeEquals(Hash(Password!), Hash(supplied));

    public void Grant(ISession session, string target)
    {
        if (!ValidTarget(target) || !IsConfigured) throw new InvalidOperationException("Invalid system tools access grant.");
        // A changed password immediately invalidates grants; no plaintext password is stored in session.
        session.SetString(Key(target), Convert.ToBase64String(Hash(Password!)));
        session.SetString(Key(target) + ":Expires", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
    }

    public bool HasAccess(ISession session, string target)
    {
        if (!ValidTarget(target) || !IsConfigured) return false;
        var fingerprint = session.GetString(Key(target));
        return fingerprint != null &&
            string.Equals(fingerprint, Convert.ToBase64String(Hash(Password!)), StringComparison.Ordinal) &&
            long.TryParse(session.GetString(Key(target) + ":Expires"), out var expires) &&
            expires > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void Revoke(ISession session, string target)
    {
        session.Remove(Key(target));
        session.Remove(Key(target) + ":Expires");
    }
}

// The filter protects the whole controller, including AJAX reads and database creation.
public sealed class SystemToolsPasswordAttribute : TypeFilterAttribute
{
    public SystemToolsPasswordAttribute(string target) : base(typeof(SystemToolsPasswordFilter))
    {
        Arguments = [target];
    }
}

public sealed class SystemToolsPasswordFilter(SystemToolsAccess access, string target) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        context.HttpContext.Response.Headers.CacheControl = "no-store";
        if (access.HasAccess(context.HttpContext.Session, target)) return;
        var request = context.HttpContext.Request;
        if (HttpMethods.IsGet(request.Method) &&
            string.Equals(context.RouteData.Values["action"]?.ToString(), "Index", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("Unlock", "SystemTools", new { target });
            return;
        }
        context.Result = new JsonResult(new { Message = "برای ادامه، از منوی ابزارهای سیستم دوباره رمز را وارد کنید." })
        { StatusCode = StatusCodes.Status401Unauthorized };
    }
}
