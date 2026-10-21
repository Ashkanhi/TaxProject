using Microsoft.AspNetCore.Mvc;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI.Omid;

namespace TaxWebAppCore8.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class OmidDashboardController(ILogger<OmidDashboardController> logger) : Controller
{
    private static bool IsOmid => string.Equals(BMain.SrvInfo?.ServiceType?.Trim(), "Omid", StringComparison.OrdinalIgnoreCase);

    [HttpGet]
    public IActionResult Index() => IsOmid ? View() : NotFound();

    [HttpGet]
    public async Task<IActionResult> SummaryAsync(CancellationToken ct)
    {
        if (!IsOmid) return NotFound();
        try
        {
            return Json(await new OmidDashboardService(BMain.SrvInfo.DBConn).ReadAsync(ct));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Could not load Omid dashboard counts");
            return StatusCode(500, new { Message = "آمار دریافت نشد. اتصال دیتابیس و بخش «بررسی صحت ساختار» را بررسی کنید." });
        }
    }
}
