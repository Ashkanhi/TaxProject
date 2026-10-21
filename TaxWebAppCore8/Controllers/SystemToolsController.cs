using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaxWebAppCore8.BI;

namespace TaxWebAppCore8.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SystemToolsController(SystemToolsAccess access) : Controller
{
    [HttpGet]
    public IActionResult Unlock(string target = "setup")
    {
        if (!SystemToolsAccess.ValidTarget(target)) return NotFound();
        // Keep old bookmarks working while the logs password prompt is temporarily disabled.
        if (target == "logs") return RedirectToAction("Index", "SystemLogs");
        // Every menu click opens a fresh password prompt for the selected tool.
        access.Revoke(HttpContext.Session, target);
        return Prompt(target);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("SystemToolsLogin")]
    public IActionResult Unlock(string target, string? password)
    {
        if (!SystemToolsAccess.ValidTarget(target)) return NotFound();
        if (target == "logs") return RedirectToAction("Index", "SystemLogs");
        access.Revoke(HttpContext.Session, target);
        if (!access.Verify(password))
        {
            // Never return the attempted password to the view, ModelState or logs.
            ModelState.Clear();
            ViewData["Error"] = access.IsConfigured ? "رمز عبور صحیح نیست." : "رمز ابزارهای سیستم هنوز در تنظیمات سرور تعیین نشده است.";
            return Prompt(target);
        }
        access.Grant(HttpContext.Session, target);
        return RedirectToAction("Index", SystemToolsAccess.ControllerFor(target));
    }

    private IActionResult Prompt(string target)
    {
        ViewData["Target"] = target;
        ViewData["ToolTitle"] = target == "setup" ? "بررسی صحت ساختار" : "لاگ‌های سیستم";
        ViewData["Configured"] = access.IsConfigured;
        return View("Unlock");
    }
}
