using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OfficeOpenXml;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI.NewTax;
using TaxWebAppCore8.Models.SharedDTO;

namespace TaxWebAppCore8.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class NewTaxController(ILogger<NewTaxController> logger) : Controller
{
    public static bool IsNewTax => string.Equals(BMain.SrvInfo?.ServiceType?.Trim(), "NewTax", StringComparison.OrdinalIgnoreCase);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!IsNewTax) context.Result = NotFound();
        base.OnActionExecuting(context);
    }

    [HttpGet] public IActionResult Index() => RedirectToAction(nameof(Send));
    [HttpGet] public IActionResult Send() { ViewData["Inquiry"] = false; return View("Invoices"); }
    [HttpGet] public IActionResult Inquiry() { ViewData["Inquiry"] = true; return View("Invoices"); }
    [HttpGet] public IActionResult Dashboard() => View();

    [HttpGet]
    public IActionResult Stores()
    {
        try { return Json(new { Success = true, Rows = BMain.GetStore() }); }
        catch (Exception ex) { return Failure(ex, "فهرست فروشگاه‌ها دریافت نشد."); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> List(NewTaxPageFilter filter)
    {
        if (!ModelState.IsValid) return BadRequest(new { Message = "فیلترهای جستجو معتبر نیستند." });
        try
        {
            var rows = await BMain._newTaxService.GetAllInvoice(filter.ToSearch());
            return Json(new { Success = true, Rows = rows });
        }
        catch (ArgumentException ex) { return BadRequest(new { Message = ex.Message }); }
        catch (Exception ex) { return Failure(ex, "فهرست صورتحساب‌ها دریافت نشد؛ اتصال دیتابیس را بررسی کنید."); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSelected([FromBody] List<DTOInvoiceLimitParameter> invoices)
    {
        if (!ModelState.IsValid || invoices == null || invoices.Count is < 1 or > 100
            || invoices.Any(x => x == null || !Guid.TryParse(x.InvoiceID, out _)))
            return BadRequest(new { Message = "بین ۱ تا ۱۰۰ صورتحساب معتبر را انتخاب کنید." });
        try { return Json(await BMain._newTaxService.SendGroupInvoicesForGroupingAsync(invoices)); }
        catch (Exception ex) { return Failure(ex, "ارسال تکمیل نشد؛ پیش از تلاش مجدد، وضعیت صورتحساب‌ها را استعلام کنید."); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> InquirySelected([FromBody] List<Guid> invoiceIds)
    {
        if (!ModelState.IsValid || invoiceIds == null || invoiceIds.Count is < 1 or > 100 || invoiceIds.Any(id => id == Guid.Empty))
            return BadRequest(new { Message = "بین ۱ تا ۱۰۰ صورتحساب معتبر را انتخاب کنید." });
        var results = new List<object>();
        foreach (var id in invoiceIds.Distinct())
        {
            try
            {
                var result = await BMain._newTaxService.GetInvoiceStatusByRefrenceAsync(id.ToString());
                results.Add(new { InvoiceID = id, result.Success, result.Status, result.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "NewTax inquiry failed for invoice {InvoiceId}", id);
                results.Add(new { InvoiceID = id, Success = false, Status = "", Message = "پاسخ استعلام دریافت نشد؛ دوباره استعلام کنید." });
            }
        }
        return Json(new { Success = true, Rows = results });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckConnection()
    {
        try
        {
            var success = await BMain._newTaxService.CheckConnectionAsync();
            return Json(new { Success = success, Message = success ? "ارتباط با سامانه و دریافت اطلاعات حافظه موفق بود." : "ارتباط تأیید نشد؛ تنظیمات سرویس را بررسی کنید." });
        }
        catch (Exception ex) { return Failure(ex, "ارتباط برقرار نشد؛ آدرس سرویس، کلید خصوصی و شناسه حافظه را بررسی کنید."); }
    }

    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        try
        {
            var rows = await BMain._newTaxService.GetAllInvoice(new() { StorID = -1 });
            // These are the customer's shared local invoice statuses, not an Omid submission ledger.
            var states = rows.GroupBy(row => row.InvoiceId)
                .Select(group => group.OrderByDescending(row => row.fLastSendDate).ThenByDescending(row => row.fID).First().fSendState ?? 1).ToList();
            return Json(new { Total = states.Count, Unsent = states.Count(s => s == 1),
                Pending = states.Count(s => s == 2), Failed = states.Count(s => s == 3),
                Accepted = states.Count(s => s == 4), Other = states.Count(s => s is < 1 or > 4), CheckedAt = DateTimeOffset.UtcNow });
        }
        catch (Exception ex) { return Failure(ex, "آمار دریافت نشد؛ اتصال دیتابیس را بررسی کنید."); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(NewTaxPageFilter filter)
    {
        if (!ModelState.IsValid) return BadRequest(new { Message = "فیلترهای جستجو معتبر نیستند." });
        try
        {
            var rows = await BMain._newTaxService.GetAllInvoice(filter.ToSearch());
            using var workbook = new ExcelPackage();
            var sheet = workbook.Workbook.Worksheets.Add("صورتحساب‌ها");
            sheet.View.RightToLeft = true;
            var headings = new[] { "شماره فاکتور", "شعبه", "تاریخ ثبت", "مشتری", "وضعیت", "مبلغ خالص", "مالیات", "شماره پیگیری" };
            for (var column = 0; column < headings.Length; column++) sheet.Cells[1, column + 1].Value = headings[column];
            var rowNumber = 2;
            foreach (var row in rows)
            {
                object[] values = { row.RealInvoiceNumber, row.StoreName ?? "", row.PersianCreateDate, row.CustomerName ?? "", row.SendStatStr,
                    row.NetPrice, row.TotalTax, row.fTaxHistoryReferenceNumber ?? row.taxReferenceNumber ?? row.ReferenceNumber ?? "" };
                for (var column = 0; column < values.Length; column++) sheet.Cells[rowNumber, column + 1].Value = values[column];
                rowNumber++;
            }
            sheet.Cells[1, 1, 1, headings.Length].Style.Font.Bold = true;
            sheet.View.FreezePanes(2, 1);
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns(12, 42);
            return File(workbook.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NewTax-Invoices.xlsx");
        }
        catch (ArgumentException ex) { return BadRequest(new { Message = ex.Message }); }
        catch (Exception ex) { return Failure(ex, "خروجی اکسل ساخته نشد؛ دوباره تلاش کنید."); }
    }

    private IActionResult Failure(Exception ex, string message)
    {
        logger.LogError(ex, "NewTax page operation failed");
        return StatusCode(500, new { Success = false, Message = message });
    }
}
