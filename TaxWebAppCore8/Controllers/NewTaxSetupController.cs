using Microsoft.AspNetCore.Mvc;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI;
using TaxWebAppCore8.BI.NewTax;

namespace TaxWebAppCore8.Controllers;

[SystemToolsPassword("newtax-setup")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class NewTaxSetupController(ILogger<NewTaxSetupController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index() => NewTaxController.IsNewTax ? View() : NotFound();

    [HttpGet]
    public async Task<IActionResult> CheckAsync(CancellationToken ct)
    {
        if (!NewTaxController.IsNewTax) return NotFound();
        try { return Json(new { Items = await new NewTaxSetupService(BMain.SrvInfo.DBConn).CheckAsync(ct) }); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "NewTax database structure check failed");
            return StatusCode(500, new { Message = "بررسی انجام نشد؛ اتصال دیتابیس و مجوز مشاهدهٔ ساختار را بررسی کنید." });
        }
    }
}
