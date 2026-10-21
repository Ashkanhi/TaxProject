using Microsoft.AspNetCore.Mvc;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI.Omid;

namespace TaxWebAppCore8.Controllers;

[TaxWebAppCore8.BI.SystemToolsPassword("setup")]
public sealed class OmidSetupController(ILogger<OmidSetupController> logger) : Controller
{
    private static bool IsOmid => string.Equals(BMain.SrvInfo?.ServiceType?.Trim(), "Omid", StringComparison.OrdinalIgnoreCase);

    [HttpGet]
    public IActionResult Index() => IsOmid ? View() : NotFound();

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> CheckAsync(CancellationToken ct)
    {
        if (!IsOmid) return NotFound();
        try
        {
            var result = await new OmidSetupService(BMain.SrvInfo.DBConn).CheckAsync(ct);
            return Json(new { Items = result });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Omid setup check failed");
            return StatusCode(500, new { Message = "بررسی دیتابیس انجام نشد؛ اتصال و مجوز مشاهدهٔ ساختار را بررسی کنید. جزئیات در لاگ ثبت شد." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAsync(string name, CancellationToken ct)
    {
        if (!IsOmid) return NotFound();
        try
        {
            await new OmidSetupService(BMain.SrvInfo.DBConn).CreateAsync(name, ct);
            logger.LogInformation("Omid setup created or verified {ObjectName}", name);
            return Json(new { Message = $"dbo.{name} ایجاد و بررسی شد." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Omid setup create failed for {ObjectName}", name);
            return StatusCode(500, new { Message = "ایجاد انجام نشد و تراکنش برگشت داده شد؛ مجوز ساخت و وابستگی‌ها را بررسی کنید. جزئیات در لاگ ثبت شد." });
        }
    }
}
