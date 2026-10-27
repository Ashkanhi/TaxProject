
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Reflection;
using TaxServiceAndApp.Models;
using System.Threading.Tasks;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models.SharedDTO;
using System;
using System.Collections.Generic;
using ClsTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting.Server;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Microsoft.AspNetCore.Hosting;
using TaxWebAppCore8.Models.SharedDTO;
using Microsoft.AspNetCore.SignalR;
using TaxWebAppCore8.BI.Omid;

namespace TaxWebAppCore8.Controllers
{

    public class SendInvoiceController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SendInvoiceController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        private static string GetPersianUserError(Exception exception, string operationTitle)
        {
            if (exception is UriFormatException ||
                exception.Message.Contains("Invalid URI", StringComparison.OrdinalIgnoreCase) ||
                exception.Message.Contains("hostname could not be parsed", StringComparison.OrdinalIgnoreCase))
            {
                return "آدرس سرویس ریما در تنظیمات معتبر نیست. لطفاً تنظیم API_URL بررسی شود.";
            }

            if (exception is TaskCanceledException)
                return $"زمان انتظار برای {operationTitle} به پایان رسید. لطفاً دوباره تلاش کنید.";

            return $"در {operationTitle} خطای داخلی رخ داد. جزئیات فنی در گزارش سیستم ثبت شد.";
        }

        public ActionResult Index()
        {
            if (NewTaxController.IsNewTax) return RedirectToAction("Send", "NewTax");
            return View("~/Views/Home/Index.cshtml");
        }

        [HttpPost]
        public async Task<ActionResult> GetInvoiceStatusByDateRange(string fromdate, string todate)
        {
            try
            {
                var res = await BMain.GetInvoiceStatusByDateRangeAsync(fromdate, todate);
                var result = Json(new
                {
                    success = true,
                    res = res,
                });

                return result;
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetInvoiceStatusByRefrence(string invoiceID)
        {
            try
            {
                ResultModel res = await BMain.GetInvoiceStatusByRefrenceAsync(invoiceID);

                var result = Json(new
                {
                    success = true,
                    res = res,
                });
                return result;
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetGroupingInvoiceStatusByRefrence(List<string> ids)
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";

                List<Guid> idList = ids.Select(Guid.Parse).ToList();
                List<ResultModel> ListOfResult = new List<ResultModel>();

                foreach (var item in idList)
                {
                    ResultModel res = await BMain.GetInvoiceStatusByRefrenceAsync(item.ToString());
                    ListOfResult.Add(res);
                }

                var result = Json(new
                {
                    success = true,
                    Data = ListOfResult,
                });

                return result;
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }
        [HttpPost]
        public async Task<ActionResult> SendInvoice(DTOInvoiceLimitParameter request)
        {
            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogInfoMessage($"::SendController::" +
                    $"SendInvoice:IN:request.invoiceId::" + request.InvoiceID);

                var res = await BMain.SendInvoiceAsync(request);
                var result = Json(new
                {
                    success = true,
                    Data = res,
                });
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                   .LogInfoMessage($"::SendController::" +
                   $"SendInvoice:OUT:request.invoiceId::" + request.InvoiceID
                   + res.ToString()
                   );
                return result;

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                    LogError($"::{MethodBase.GetCurrentMethod().DeclaringType.Name}::" +
                    $"SendController::ID::"
                    + "SendInvoice::" + ex.Message + "::" + ex?.InnerException?.Message, ex);

                var data = new { ex, success = false };
                return Json(data);
            }
        }

        //[HttpPost]
        //public async Task<ActionResult> SendGroupingInvoice
        //    (List<DTOInvoiceLimitParameter> stringList, int SumCount)
        //{
        //    try
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.OK;
        //        Response.ContentType = "application/json; charset=utf-8";
        //        if (stringList == null || SumCount == 0)
        //        {
        //            var data = new { message = "داده ها به درستی به سمت سرور انتقال پیدا نمی کند!", success = false };
        //            return Json(data);
        //        }
        //        //List<Guid> idList = stringList.Select(Guid.Parse).ToList();
        //        string jsonUserObject = HttpContext.Session.GetString("CollectDatas");
        //        List<DTOInvoiceLimitParameter>
        //                collect=new List<DTOInvoiceLimitParameter>();
        //        int countOfCollect = 0;
        //        if (jsonUserObject != null)
        //        {
        //            collect = System.Text.Json.JsonSerializer.
        //                 Deserialize<List<DTOInvoiceLimitParameter>>(jsonUserObject);
        //            countOfCollect = collect.Count;
        //        }

        //        if (stringList.Count >= SumCount
        //            || (stringList.Count + countOfCollect) >= SumCount)
        //        {
        //            collect.AddRange(stringList);
        //            HttpContext.Session.Remove("CollectDatas");

        //            if (collect != null && collect.Count > 0)
        //            {
        //                var res = await BMain.SendGroupInvoicesAsync(collect);
        //                if (res.Success == true)
        //                {
        //                    var result = Json(new
        //                    {
        //                        success = true,
        //                        res = res,
        //                        countOfSend = res.Count
        //                    });

        //                    return result;
        //                }
        //                else
        //                {
        //                    var data = new { res, message = res.Message, success = false };
        //                    return Json(data);
        //                }
        //            }
        //            else
        //            {
        //                var data = new { message = "فاکتوری انتخاب نشده است", success = false };
        //                return Json(data);
        //            }
        //        }
        //        else 
        //        {
        //            HttpContext.Session.Remove("CollectDatas");


        //            if (collect == null)
        //                collect = new List<DTOInvoiceLimitParameter>();
        //            collect.AddRange(stringList);

        //            string serilize = System.Text.Json.JsonSerializer.Serialize(collect);
        //            HttpContext.Session.SetString("CollectDatas", serilize);

        //            var data = new { message = "مابقی رو ارسال نمایید",
        //                success = true,Partial=true };
        //            return Json(data);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var data = new { ex, success = false };
        //        return Json(data);
        //    }
        //}
        [HttpPost]
        public async Task<ActionResult> SendGroupingInvoice(List<DTOInvoiceLimitParameter> stringList, int SumCount, int batchSize, string? operationId)
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";
                ClsTools.Logger.NPSLogger.GetLogger
                  (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
            + " SumCount ::" + SumCount);
                if (stringList == null || SumCount == 0)
                {
                    var data = new { message = "داده ها به درستی به سمت سرور انتقال پیدا نمی کند!", success = false };
                    return Json(data);
                }

                string clientIpAddress = HttpContext.Connection?.RemoteIpAddress?.ToString();
                ClsTools.Logger.NPSLogger.GetLogger
                 (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
               LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
           + " clientIpAddress ::" + clientIpAddress);
                // هر ارسال گروهی فضای جمع‌آوری جداگانه دارد؛ دو tab یا دو کاربر پشت یک IP
                // نباید batchهای یکدیگر را در Session ادغام کنند.
                string sessionScope = string.IsNullOrWhiteSpace(operationId)
                    ? clientIpAddress
                    : $"{clientIpAddress}_{operationId}";
                string sessionKey = $"CollectDatas_{sessionScope}";
                ClsTools.Logger.NPSLogger.GetLogger
                (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
              LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
          + " sessionKey ::" + sessionKey);
                string jsonUserObject = HttpContext.Session.GetString(sessionKey);
               
                List<DTOInvoiceLimitParameter>? collect = string.IsNullOrEmpty(jsonUserObject)
                    ? new List<DTOInvoiceLimitParameter>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<DTOInvoiceLimitParameter>>
                    (jsonUserObject);

                if (collect == null) {
                    ClsTools.Logger.NPSLogger.GetLogger
              (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
            LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
        + " collect == null ::********************" );
                    collect = new List<DTOInvoiceLimitParameter>(); }

                ClsTools.Logger.NPSLogger.GetLogger
                   (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " InitAsync ::" + stringList.Count);


                if (stringList.Count >= SumCount || (stringList.Count + collect.Count) >= SumCount
                    || stringList.Count< batchSize )
                {
                    ClsTools.Logger.NPSLogger.GetLogger
                   (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " InitAsync :Starting.............:" + stringList.Count);
                    var checkResult = await BMain.CheckConnectionAsync();
                    if (!checkResult)
                    {
                        return Json(new { message = "ارتباط با سامانه مودیان برقرار نمی باشد!", success = false });
                    }


                    collect.AddRange(stringList);
                    HttpContext.Session.Remove(sessionKey);

                    if (collect.Count > 0)
                    {
                        var res = await BMain.SendGroupInvoicesAsync(collect, operationId);
                        if (res.Success)
                        {
                            return Json(new { success = true, res = res, countOfSend = res.Count });
                        }
                        else
                        {
                            return Json(new { res, message = res.Message, success = false });
                        }
                    }
                    else
                    {
                        return Json(new { message = "فاکتوری انتخاب نشده است", success = false });
                    }
                }
                else
                {
                    collect.AddRange(stringList);
                    string serializedData = System.Text.Json.JsonSerializer.Serialize(collect);
                    HttpContext.Session.Remove(sessionKey);

                    HttpContext.Session.SetString(sessionKey, serializedData);
                    ClsTools.Logger.NPSLogger.GetLogger
                  (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
            + " InitAsync :collect.............:" + collect.Count);
                    return Json(new { message = "مابقی رو ارسال نمایید", success = true, Partial = true });
                }
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                       LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                   + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                // Log the exception for debugging
                Console.WriteLine(ex.Message);
                return Json(new { message = GetPersianUserError(ex, "ارسال گروهی فاکتورها"), success = false });
            }
        }
        [HttpPost]
        public ActionResult GetAllTissInfoItems(Guid InvoiceID, int StoreID, int BookerWorkstationID)
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";

                var res = BMain.GetAllTissInfoItems(InvoiceID, StoreID, BookerWorkstationID);

                if (res != null)
                {
                    var result = Json(new
                    {
                        success = true,
                        Items = res,
                        countOfResult = res.Count,
                        SumOfItems = res.Sum(a => a.tsstam.HasValue ? a.tsstam.Value : 0).ToString("#,0")
                    });

                    return result;
                }
                else
                {
                    var data = new { message = "خطا در سمت سرور!", success = false };
                    return Json(data);
                }
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }


        [HttpPost]
        public async Task<ActionResult> GetInvoice(string invoiceId)
        {
            try
            {
                var res = await BMain.GetInvoiceAsync(invoiceId);
                var data = new { res, success = true };
                return Json(data);
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetInvoiceByID(string invoiceID)
        {
            try
            {
                var res = await BMain.GetInvoiceByIDAsync(invoiceID);
                var data = new { res, success = true };
                return Json(data);
            }
            catch (Exception ex)
            {

                var data = new { ex, success = false };
                return Json(data);
            }
        }



        /// <summary>
        /// متد اصلی فیلتر برای بارگذاری داده در دیتاتیبل
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        /// [HttpPost]
        [HttpPost]

        public async Task<ActionResult> GetAllInvoice(SearchParameters searchFilter)
        {
            try
            {

                var list = await BMain.GetAllInvoiceAsync(searchFilter);

                // فقط در صفحهٔ ارسال امید، فاکتورهای ثبت‌شدهٔ امید را پنهان می‌کنیم.
                // فهرست پایه دست‌نخورده می‌ماند تا صفحهٔ استعلام بتواند آن‌ها را پیدا کند.
                if (string.Equals(BMain.SrvInfo?.ServiceType?.Trim(),
                    "Omid", StringComparison.OrdinalIgnoreCase))
                {
                    var omidStore = new OmidSubmissionStore(BMain.SrvInfo);
                    var submittedIds = await omidStore
                        .GetSubmittedInvoiceIdsAsync(HttpContext.RequestAborted);
                    var failedErrors = await omidStore
                        .GetFailedErrorsAsync(HttpContext.RequestAborted);

                    // خطای رد قطعی امید کنار همان فاکتور در ستون توضیح وضعیت دیده می‌شود.
                    foreach (var invoice in list)
                    {
                        if (Guid.TryParse(invoice.InvoiceId, out var invoiceId) &&
                            failedErrors.TryGetValue(invoiceId, out string? error))
                        {
                            invoice.fListOfErrorCaption = "خطای امید: " + error;
                        }
                    }

                    list = list.Where(invoice =>
                        Guid.TryParse(invoice.InvoiceId, out var invoiceId) &&
                        !submittedIds.Contains(invoiceId)).ToList();
                }

                var result = Json(new
                {
                    success = true,
                    Data = list,

                });

                return result;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]

        public async Task<ActionResult> GetAllInvoiceInExcel(SearchParameters searchFilter, string? operationId)
        {
            try
            {
                var list = await BMain.GetAllInvoiceAsync(searchFilter);
                if (list != null)
                {
                    string filename = "گزارش فاکتورها" + DateTime.Now.Millisecond + ".xlsx";
                    var directory = "~/Resources/FilesData/";
                    string directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, "Resources", "FilesData");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    string filePath = directoryPath + @"/" + filename;
                    var newFile = new FileInfo(filePath);
                    decimal sumOftsstam = 0;
                    decimal sumOfadis = 0;
                    decimal sumOfdis = 0;
                    decimal sumOfvam = 0;
                    decimal sumOfprdis = 0;
                    using (ExcelPackage xlPackage = new ExcelPackage(newFile))
                    {
                        var sheet = xlPackage.Workbook.Worksheets.Add("CardsList");
                        sheet.View.RightToLeft = true;
                        int j = 1;
                        sheet.Cells[j, 1].Value = "شماره فاکتور";
                        sheet.Cells[j, 2].Value = "وضعیت";
                        sheet.Cells[j, 3].Value = "تاریخ ثبت";
                        sheet.Cells[j, 4].Value = "مشتری";
                        sheet.Cells[j, 5].Value = "مبلغ فاکتور(ریال)";
                        sheet.Cells[j, 6].Value = "مبلغ تخفیف";
                        sheet.Cells[j, 7].Value = "مبلغ خالص(ریال)";
                        sheet.Cells[j, 8].Value = "مالیات";
                        sheet.Cells[j, 9].Value = "توضیحات";

                        sheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkSlateBlue);
                        sheet.Row(1).Style.Font.Bold = true;
                        sheet.Row(1).Style.Font.Size = 13;
                        sheet.Row(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
                        int countofAll = list.Count;
                        int countofSend = 0;
                        foreach (var playObject in list)
                        {
                            var ItemsOfFactor = BMain.GetAllTissInfoItems(Guid.Parse(playObject.InvoiceId), playObject.StoreID.Value,
                                playObject.BookerWorkstationID.Value);

                            countofSend++;
                            var message =
                                     "\r\n تعداد : " + countofSend +
                                    " از " + countofAll;

                            var progressClients = string.IsNullOrWhiteSpace(operationId)
                                ? BMain._hubContext.Clients.All
                                : BMain._hubContext.Clients.Group(operationId);
                            await progressClients.SendAsync("SendToTaxExcel", new
                            {
                                operationId,
                                message,
                                processed = countofSend,
                                total = countofAll,
                                succeeded = countofSend,
                                failed = 0,
                                completed = countofSend == countofAll
                            });

                            if (ItemsOfFactor == null) continue;
                            j++;

                            sheet.Cells[j, 1].Value = playObject.RealInvoiceNumber;
                            sheet.Cells[j, 2].Value = playObject.SendStatStr;
                            sheet.Cells[j, 3].Value = playObject.PersianCreateDate;
                            sheet.Cells[j, 4].Value = playObject.CustomerName;
                            sheet.Cells[j, 5].Value = playObject.TotalPriceWithSeperator;
                            sheet.Cells[j, 6].Value = playObject.TotalDiscount;
                            sheet.Cells[j, 7].Value = playObject.NetPriceWithSeperator;
                            sheet.Cells[j, 8].Value = playObject.TotalTax;
                            sheet.Cells[j, 9].Value = playObject.InvoiceDescription;

                            sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Row(j).Style.Font.Bold = true;
                            sheet.Row(j).Style.Font.Size = 13;
                            sheet.Row(j).Style.Font.Color.SetColor(System.Drawing.Color.Black);

                            sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Row(j).Style.Font.Size = 12;
                            sheet.Row(j).Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                            sheet.Row(j).Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);

                            switch (playObject.fSendState)
                            {
                                case 1:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                                    break;
                                case 2:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepPink);
                                    break;
                                case 3:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    break;
                                case 4:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LimeGreen);
                                    break;
                            }

                            // Add a new row for ItemsOfFactor
                            j++;


                            sheet.Cells[j, 1].Value = "اقلام فاکتور";
                            sheet.Cells[j, 2].Value = "نام کالا";
                            sheet.Cells[j, 3].Value = "قیمت(ریال)";
                            sheet.Cells[j, 4].Value = "مقدار";
                            sheet.Cells[j, 5].Value = "قیمت قبل تخفیف(ریال)";
                            sheet.Cells[j, 6].Value = "مالیات";
                            sheet.Cells[j, 7].Value = "تخفیف";
                            sheet.Cells[j, 8].Value = "قیمت با تخفیف(ریال)";
                            sheet.Cells[j, 9].Value = "قابل پرداخت(ریال)";

                            sheet.Cells[j, 1].Style.Font.Bold = true;
                            sheet.Cells[j, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            sheet.Cells[j, 1].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sheet.Row(j).Style.Font.Bold = true;

                            // Display the items of the factor in a formatted manner (e.g., bulleted list or table)
                            foreach (var item in ItemsOfFactor)
                            {
                                j++;
                                sheet.Cells[j, 2].Value = item.sstt;
                                sheet.Cells[j, 3].Value = item.fee?.ToString("#,0");
                                sheet.Cells[j, 4].Value = item.am?.ToString("#,0");
                                sheet.Cells[j, 5].Value = item.prdis?.ToString("#,0");
                                sheet.Cells[j, 6].Value = item.vam?.ToString("#,0");
                                sheet.Cells[j, 7].Value = item.dis?.ToString("#,0");
                                sheet.Cells[j, 8].Value = item.adis?.ToString("#,0");
                                sheet.Cells[j, 9].Value = item.tsstam?.ToString("#,0");
                            }
                            j++;


                            sheet.Cells[j, 5].Value = "جمع قیمت قبل تخفیف: " + ItemsOfFactor.Sum
                                (a => a.prdis.HasValue ? a.prdis.Value : 0)
                                .ToString("#,0");

                            sumOfprdis += ItemsOfFactor.Sum
                                (a => a.prdis.HasValue ? a.prdis.Value : 0);

                            sheet.Cells[j, 5].Style.Font.Bold = true;
                            sheet.Cells[j, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 5].Style.Font.Color.SetColor(System.Drawing.Color.Black);


                            sheet.Cells[j, 6].Value = "جمع مالیات: " + ItemsOfFactor.Sum
                                (a => a.vam.HasValue ? a.vam.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 6].Style.Font.Bold = true;
                            sheet.Cells[j, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 6].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfvam += ItemsOfFactor.Sum
                                (a => a.vam.HasValue ? a.vam.Value : 0);

                            sheet.Cells[j, 7].Value = "جمع تخفیف: " + ItemsOfFactor.Sum
                                (a => a.dis.HasValue ? a.dis.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 7].Style.Font.Bold = true;
                            sheet.Cells[j, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfdis += ItemsOfFactor.Sum
                                (a => a.dis.HasValue ? a.dis.Value : 0);

                            sheet.Cells[j, 8].Value = "جمع قیمت با تخفیف: " + ItemsOfFactor.Sum
                                (a => a.adis.HasValue ? a.adis.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 8].Style.Font.Bold = true;
                            sheet.Cells[j, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 8].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfadis += ItemsOfFactor.Sum
                                (a => a.adis.HasValue ? a.adis.Value : 0);

                            sheet.Cells[j, 9].Value = "جمع مبلغ: " + ItemsOfFactor.Sum
                                (a => a.tsstam.HasValue ? a.tsstam.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 9].Style.Font.Bold = true;
                            sheet.Cells[j, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 9].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOftsstam += ItemsOfFactor.Sum
                                (a => a.tsstam.HasValue ? a.tsstam.Value : 0);

                            j++;
                        }


                        j++;


                        sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Row(j).Style.Font.Bold = true;
                        sheet.Row(j).Style.Font.Size = 13;
                        sheet.Row(j).Style.Font.Color.SetColor(System.Drawing.Color.Black);


                        sheet.Cells[j, 5].Value = "جمع قیمت قبل تخفیف: " + sumOfprdis
                            .ToString("#,0");


                        sheet.Cells[j, 5].Style.Font.Bold = true;
                        sheet.Cells[j, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 5].Style.Font.Color.SetColor(System.Drawing.Color.Black);


                        sheet.Cells[j, 6].Value = "جمع مالیات: " + sumOfvam
                            .ToString("#,0");
                        sheet.Cells[j, 6].Style.Font.Bold = true;
                        sheet.Cells[j, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 6].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 7].Value = "جمع تخفیف: " + sumOfdis
                            .ToString("#,0");
                        sheet.Cells[j, 7].Style.Font.Bold = true;
                        sheet.Cells[j, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 8].Value = "جمع قیمت با تخفیف: " + sumOfadis
                            .ToString("#,0");
                        sheet.Cells[j, 8].Style.Font.Bold = true;
                        sheet.Cells[j, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 8].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 9].Value = "جمع مبلغ: " + sumOftsstam
                            .ToString("#,0");
                        sheet.Cells[j, 9].Style.Font.Bold = true;
                        sheet.Cells[j, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 9].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells["A:P"].AutoFitColumns();
                        xlPackage.Save();
                    }

                    Response.StatusCode = (int)HttpStatusCode.OK;
                    Response.ContentType = "application/json; charset=utf-8";

                    return Json(new
                    {
                        success = true,
                        Data = filename,
                    });
                }
                return Json(new
                {
                    success = false,
                    Data = "",
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]

        public async Task<ActionResult> GetAllInvoiceInExcelSummary(SearchParameters searchFilter, string? operationId)
        {
            try
            {
                var list = await BMain.GetAllInvoiceAsync(searchFilter);
                if (list != null)
                {
                    string filename = "گزارش فاکتورها" + DateTime.Now.Millisecond + ".xlsx";
                    var directory = "~/Resources/FilesData/";
                    string directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, "Resources", "FilesData");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    string filePath = directoryPath + @"/" + filename;
                    var newFile = new FileInfo(filePath);
                    decimal sumOftsstam = 0;
                    decimal sumoFCount = 0;
                    decimal sumOfadis = 0;
                    decimal sumOfdis = 0;
                    decimal sumOfvam = 0;
                    decimal sumOfprdis = 0;
                    using (ExcelPackage xlPackage = new ExcelPackage(newFile))
                    {
                        var sheet = xlPackage.Workbook.Worksheets.Add("CardsList");
                        sheet.View.RightToLeft = true;
                        int j = 1;

                        sheet.Cells[j, 1].Value = "شماره فاکتور";
                        sheet.Cells[j, 2].Value = "وضعیت";
                        sheet.Cells[j, 3].Value = "تاریخ ثبت";
                        sheet.Cells[j, 4].Value = "مشتری";
                        sheet.Cells[j, 5].Value = "مبلغ فاکتور(ریال)";
                        sheet.Cells[j, 6].Value = "مبلغ تخفیف";
                        sheet.Cells[j, 7].Value = "مبلغ خالص(ریال)";
                        sheet.Cells[j, 8].Value = "مالیات";
                        sheet.Cells[j, 9].Value = "توضیحات";
                        sheet.Cells[j, 10].Value = "";
                        sheet.Cells[j, 11].Value = "جمع قیمت قبل تخفیف";
                        sheet.Cells[j, 12].Value = "جمع مالیات";
                        sheet.Cells[j, 13].Value = "جمع تخفیف";
                        sheet.Cells[j, 14].Value = "جمع قیمت با تخفیف";
                        sheet.Cells[j, 15].Value = "جمع مبلغ";

                        sheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkSlateBlue);
                        sheet.Row(1).Style.Font.Bold = true;
                        sheet.Row(1).Style.Font.Size = 13;
                        sheet.Row(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
                        int countofAll = list.Count;
                        int countofSend = 0;

                        j++;
                        foreach (var playObject in list)
                        {
                            var ItemsOfFactor = BMain.GetAllTissInfoItems(Guid.Parse(playObject.InvoiceId), playObject.StoreID.Value,
                                playObject.BookerWorkstationID.Value);

                            countofSend++;
                            var message =
                                     "\r\n تعداد : " + countofSend +
                                    " از " + countofAll;

                            var progressClients = string.IsNullOrWhiteSpace(operationId)
                                ? BMain._hubContext.Clients.All
                                : BMain._hubContext.Clients.Group(operationId);
                            await progressClients.SendAsync("SendToTaxExcel", new
                            {
                                operationId,
                                message,
                                processed = countofSend,
                                total = countofAll,
                                succeeded = countofSend,
                                failed = 0,
                                completed = countofSend == countofAll
                            });

                            if (ItemsOfFactor == null) continue;


                            sheet.Cells[j, 1].Value = playObject.RealInvoiceNumber;
                            sheet.Cells[j, 2].Value = playObject.SendStatStr;
                            sheet.Cells[j, 3].Value = playObject.PersianCreateDate;
                            sheet.Cells[j, 4].Value = playObject.CustomerName;
                            sheet.Cells[j, 5].Value = playObject.TotalPriceWithSeperator;
                            sheet.Cells[j, 6].Value = playObject.TotalDiscount;
                            sheet.Cells[j, 7].Value = playObject.NetPriceWithSeperator;
                            sheet.Cells[j, 8].Value = playObject.TotalTax;
                            sheet.Cells[j, 9].Value = playObject.InvoiceDescription;

                            sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Row(j).Style.Font.Bold = true;
                            sheet.Row(j).Style.Font.Size = 13;
                            sheet.Row(j).Style.Font.Color.SetColor(System.Drawing.Color.Black);

                            sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Row(j).Style.Font.Size = 12;
                            sheet.Row(j).Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                            sheet.Row(j).Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);

                            switch (playObject.fSendState)
                            {
                                case 1:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                                    break;
                                case 2:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepPink);
                                    break;
                                case 3:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    break;
                                case 4:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LimeGreen);
                                    break;
                            }


                            sheet.Cells[j, 10].Style.Font.Bold = true;
                            sheet.Cells[j, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            sheet.Cells[j, 10].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sheet.Row(j).Style.Font.Bold = true;


                            sheet.Cells[j, 11].Value = ItemsOfFactor.Sum
                                (a => a.prdis.HasValue ? a.prdis.Value : 0)
                                .ToString("#,0");

                            sumOfprdis += ItemsOfFactor.Sum
                                (a => a.prdis.HasValue ? a.prdis.Value : 0);

                            sheet.Cells[j, 11].Style.Font.Bold = true;
                            sheet.Cells[j, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 11].Style.Font.Color.SetColor(System.Drawing.Color.Black);


                            sheet.Cells[j, 12].Value = ItemsOfFactor.Sum
                                (a => a.vam.HasValue ? a.vam.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 12].Style.Font.Bold = true;
                            sheet.Cells[j, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 12].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfvam += ItemsOfFactor.Sum
                                (a => a.vam.HasValue ? a.vam.Value : 0);

                            sheet.Cells[j, 13].Value = ItemsOfFactor.Sum
                                (a => a.dis.HasValue ? a.dis.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 13].Style.Font.Bold = true;
                            sheet.Cells[j, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 13].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfdis += ItemsOfFactor.Sum
                                (a => a.dis.HasValue ? a.dis.Value : 0);

                            sheet.Cells[j, 14].Value = ItemsOfFactor.Sum
                                (a => a.adis.HasValue ? a.adis.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 14].Style.Font.Bold = true;
                            sheet.Cells[j, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 14].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfadis += ItemsOfFactor.Sum
                                (a => a.adis.HasValue ? a.adis.Value : 0);

                            sheet.Cells[j, 15].Value = ItemsOfFactor.Sum
                                (a => a.tsstam.HasValue ? a.tsstam.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 15].Style.Font.Bold = true;
                            sheet.Cells[j, 15].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                            sheet.Cells[j, 15].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOftsstam += ItemsOfFactor.Sum
                                (a => a.tsstam.HasValue ? a.tsstam.Value : 0);

                            j++;
                        }


                        j++;


                        sheet.Cells[j, 1].Value = "جمع قیمت قبل تخفیف: " + sumOfprdis
                            .ToString("#,0");


                        sheet.Cells[j, 1].Style.Font.Bold = true;
                        sheet.Cells[j, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 1].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 2].Value = "جمع مالیات: " + sumOfvam
                            .ToString("#,0");
                        sheet.Cells[j, 2].Style.Font.Bold = true;
                        sheet.Cells[j, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 2].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 3].Value = "جمع تخفیف: " + sumOfdis
                            .ToString("#,0");
                        sheet.Cells[j, 3].Style.Font.Bold = true;
                        sheet.Cells[j, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 3].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 4].Value = "جمع قیمت با تخفیف: " + sumOfadis
                            .ToString("#,0");
                        sheet.Cells[j, 4].Style.Font.Bold = true;
                        sheet.Cells[j, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 4].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 5].Value = "جمع مبلغ: " + sumOftsstam
                            .ToString("#,0");
                        sheet.Cells[j, 5].Style.Font.Bold = true;
                        sheet.Cells[j, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 5].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 5].Value = "تعداد کل: " + list.Count
                            .ToString("#,0");
                        sheet.Cells[j, 5].Style.Font.Bold = true;
                        sheet.Cells[j, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                        sheet.Cells[j, 5].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                        j++;

                        sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Row(j).Style.Font.Bold = true;
                        sheet.Row(j).Style.Font.Size = 13;
                        sheet.Row(j).Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells["A:P"].AutoFitColumns();
                        xlPackage.Save();
                    }

                    Response.StatusCode = (int)HttpStatusCode.OK;
                    Response.ContentType = "application/json; charset=utf-8";

                    return Json(new
                    {
                        success = true,
                        Data = filename,
                    });
                }
                return Json(new
                {
                    success = false,
                    Data = "",
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public async Task<ActionResult> GetAllInvoiceWithInqFromServerInExcel(SearchParameters searchFilter, string? operationId)
        {
            try
            {
                var list = await BMain.GetAllInvoiceWithInqFromServerAsync(searchFilter, operationId);
                if (list != null)
                {
                    string filename = "گزارش استعلام فاکتورها" + DateTime.Now.Millisecond + ".xlsx";
                    var directory = "~/Resources/FilesData/";
                    string directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, "Resources", "FilesData");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    string filePath = directoryPath + @"/" + filename;
                    var newFile = new FileInfo(filePath);

                    decimal sumOftsstam = 0;
                    decimal sumOfadis = 0;
                    decimal sumOfdis = 0;
                    decimal sumOfvam = 0;
                    decimal sumOfprdis = 0;


                    using (ExcelPackage xlPackage = new ExcelPackage(newFile))
                    {
                        var sheet = xlPackage.Workbook.Worksheets.Add("CardsList");
                        sheet.View.RightToLeft = true;
                        int j = 1;
                        sheet.Cells[j, 1].Value = "شماره فاکتور";
                        sheet.Cells[j, 2].Value = "وضعیت";
                        sheet.Cells[j, 3].Value = "تاریخ ثبت";
                        sheet.Cells[j, 4].Value = "مشتری";
                        sheet.Cells[j, 5].Value = "مبلغ فاکتور(ریال)";
                        sheet.Cells[j, 6].Value = "مبلغ تخفیف";
                        sheet.Cells[j, 7].Value = "مبلغ خالص(ریال)";
                        sheet.Cells[j, 8].Value = "مالیات";
                        sheet.Cells[j, 9].Value = "منبع وضعیت";
                        sheet.Cells[j, 10].Value = "شرح استعلام";

                        sheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkSlateBlue);
                        sheet.Row(1).Style.Font.Bold = true;
                        sheet.Row(1).Style.Font.Size = 13;
                        sheet.Row(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
                        int countofAll = list.Count;
                        int countofSend = 0;
                        foreach (var playObject in list)
                        {
                            var ItemsOfFactor = BMain.GetAllTissInfoItems(Guid.Parse(playObject.InvoiceId), playObject.StoreID.Value,
                                                           playObject.BookerWorkstationID.Value);
                            countofSend++;
                            var message =
                                    "\r\n تعداد : " + countofSend +
                                   " از " + countofAll;

                            var progressClients = string.IsNullOrWhiteSpace(operationId)
                                ? BMain._hubContext.Clients.All
                                : BMain._hubContext.Clients.Group(operationId);
                            await progressClients.SendAsync("SendToTaxExcel", new
                            {
                                operationId,
                                message,
                                processed = countofSend,
                                total = countofAll,
                                succeeded = countofSend,
                                failed = 0,
                                completed = countofSend == countofAll
                            });
                            j++;
                            sheet.Cells[j, 1].Value = playObject.RealInvoiceNumber;
                            sheet.Cells[j, 2].Value = playObject.SendStatStr;
                            sheet.Cells[j, 3].Value = playObject.PersianCreateDate;
                            sheet.Cells[j, 4].Value = playObject.CustomerName;
                            sheet.Cells[j, 5].Value = playObject.TotalPriceWithSeperator;
                            sheet.Cells[j, 6].Value = playObject.TotalDiscount;
                            sheet.Cells[j, 7].Value = playObject.NetPriceWithSeperator;
                            sheet.Cells[j, 8].Value = playObject.TotalTax;
                            sheet.Cells[j, 9].Value = playObject.InquirySource;
                            sheet.Cells[j, 10].Value = playObject.InquiryFailed
                                ? "استعلام ناموفق: " + playObject.InqMessage
                                : playObject.InqMessage;

                            sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Row(j).Style.Font.Bold = true;
                            sheet.Row(j).Style.Font.Size = 13;
                            sheet.Row(j).Style.Font.Color.SetColor(System.Drawing.Color.Black);

                            sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Row(j).Style.Font.Size = 12;
                            sheet.Row(j).Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick; // Set the top border style
                            sheet.Row(j).Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);

                            switch (playObject.fSendState)
                            {
                                case 1:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                                    break;
                                case 2:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepPink);
                                    break;
                                case 3:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    break;
                                case 4:
                                    sheet.Row(j).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LimeGreen);
                                    break;
                            }

                            // Add a new row for ItemsOfFactor
                            j++;
                            sheet.Row(j).Style.Font.Bold = true;
                            sheet.Row(j).Style.Font.Size = 13;

                            sheet.Cells[j, 1, j, 10].Merge = true;
                            sheet.Cells[j, 1].Value = "نتیجه ارسال: " + playObject.fListOfErrorCaption + "(" + playObject.InvoiceDescription + ")";

                            j++;
                            sheet.Cells[j, 1].Value = "اقلام فاکتور";
                            sheet.Cells[j, 2].Value = "نام کالا";
                            sheet.Cells[j, 3].Value = "قیمت(ریال)";
                            sheet.Cells[j, 4].Value = "مقدار";
                            sheet.Cells[j, 5].Value = "قیمت قبل تخفیف(ریال)";
                            sheet.Cells[j, 6].Value = "مالیات";
                            sheet.Cells[j, 7].Value = "تخفیف";
                            sheet.Cells[j, 8].Value = "قیمت با تخفیف(ریال)";
                            sheet.Cells[j, 9].Value = "قابل پرداخت(ریال)";

                            sheet.Cells[j, 1].Style.Font.Bold = true;
                            sheet.Cells[j, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            sheet.Cells[j, 1].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sheet.Row(j).Style.Font.Bold = true;
                            // Display the items of the factor in a formatted manner (e.g., bulleted list or table)
                            foreach (var item in ItemsOfFactor)
                            {
                                j++;
                                sheet.Cells[j, 2].Value = item.sstt;
                                sheet.Cells[j, 3].Value = item.fee?.ToString("#,0");
                                sheet.Cells[j, 4].Value = item.am?.ToString("#,0");
                                sheet.Cells[j, 5].Value = item.prdis?.ToString("#,0");
                                sheet.Cells[j, 6].Value = item.vam?.ToString("#,0");
                                sheet.Cells[j, 7].Value = item.dis?.ToString("#,0");
                                sheet.Cells[j, 8].Value = item.adis?.ToString("#,0");
                                sheet.Cells[j, 9].Value = item.tsstam?.ToString("#,0");
                                // sheet.Cells[j, 2, j, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                                // Apply borders to the item rows
                                // sheet.Cells[j, 2, j, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            }
                            j++;


                            sheet.Cells[j, 5].Value = "جمع قیمت قبل تخفیف: " + ItemsOfFactor.Sum
                                (a => a.prdis.HasValue ? a.prdis.Value : 0)
                                .ToString("#,0");

                            sumOfprdis += ItemsOfFactor.Sum
                                (a => a.prdis.HasValue ? a.prdis.Value : 0);

                            sheet.Cells[j, 5].Style.Font.Bold = true;
                            sheet.Cells[j, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                            sheet.Cells[j, 5].Style.Font.Color.SetColor(System.Drawing.Color.Black);


                            sheet.Cells[j, 6].Value = "جمع مالیات: " + ItemsOfFactor.Sum
                                (a => a.vam.HasValue ? a.vam.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 6].Style.Font.Bold = true;
                            sheet.Cells[j, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                            sheet.Cells[j, 6].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfvam += ItemsOfFactor.Sum
                                (a => a.vam.HasValue ? a.vam.Value : 0);

                            sheet.Cells[j, 7].Value = "جمع تخفیف: " + ItemsOfFactor.Sum
                                (a => a.dis.HasValue ? a.dis.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 7].Style.Font.Bold = true;
                            sheet.Cells[j, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                            sheet.Cells[j, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfdis += ItemsOfFactor.Sum
                                (a => a.dis.HasValue ? a.dis.Value : 0);

                            sheet.Cells[j, 8].Value = "جمع قیمت با تخفیف: " + ItemsOfFactor.Sum
                                (a => a.adis.HasValue ? a.adis.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 8].Style.Font.Bold = true;
                            sheet.Cells[j, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                            sheet.Cells[j, 8].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOfadis += ItemsOfFactor.Sum
                                (a => a.adis.HasValue ? a.adis.Value : 0);

                            sheet.Cells[j, 9].Value = "جمع مبلغ: " + ItemsOfFactor.Sum
                                (a => a.tsstam.HasValue ? a.tsstam.Value : 0)
                                .ToString("#,0");
                            sheet.Cells[j, 9].Style.Font.Bold = true;
                            sheet.Cells[j, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[j, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                            sheet.Cells[j, 9].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            sumOftsstam += ItemsOfFactor.Sum
                                (a => a.tsstam.HasValue ? a.tsstam.Value : 0);

                            j++;
                        }




                        j++;
                        sheet.Row(j).Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Row(j).Style.Font.Bold = true;
                        sheet.Row(j).Style.Font.Size = 13;
                        sheet.Row(j).Style.Font.Color.SetColor(System.Drawing.Color.Black);
                        sheet.Cells[j, 5].Value = "جمع قیمت قبل تخفیف: " + sumOfprdis
                            .ToString("#,0");


                        sheet.Cells[j, 5].Style.Font.Bold = true;
                        sheet.Cells[j, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                        sheet.Cells[j, 5].Style.Font.Color.SetColor(System.Drawing.Color.Black);


                        sheet.Cells[j, 6].Value = "جمع مالیات: " + sumOfvam
                            .ToString("#,0");
                        sheet.Cells[j, 6].Style.Font.Bold = true;
                        sheet.Cells[j, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                        sheet.Cells[j, 6].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 7].Value = "جمع تخفیف: " + sumOfdis
                            .ToString("#,0");
                        sheet.Cells[j, 7].Style.Font.Bold = true;
                        sheet.Cells[j, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                        sheet.Cells[j, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 8].Value = "جمع قیمت با تخفیف: " + sumOfadis
                            .ToString("#,0");
                        sheet.Cells[j, 8].Style.Font.Bold = true;
                        sheet.Cells[j, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                        sheet.Cells[j, 8].Style.Font.Color.SetColor(System.Drawing.Color.Black);

                        sheet.Cells[j, 9].Value = "جمع مبلغ: " + sumOftsstam
                            .ToString("#,0");
                        sheet.Cells[j, 9].Style.Font.Bold = true;
                        sheet.Cells[j, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[j, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MediumPurple);
                        sheet.Cells[j, 9].Style.Font.Color.SetColor(System.Drawing.Color.Black);


                        sheet.Cells["A:P"].AutoFitColumns();
                        xlPackage.Save();
                    }

                    Response.StatusCode = (int)HttpStatusCode.OK;
                    Response.ContentType = "application/json; charset=utf-8";

                    return Json(new
                    {
                        success = true,
                        Data = filename,
                    });
                }
                return Json(new
                {
                    success = false,
                    Data = "",
                });
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogError($"::{MethodBase.GetCurrentMethod().DeclaringType.Name}::GetAllInvoiceWithInqFromServerInExcel::{ex.Message}", ex);
                return Json(new { success = false, message = GetPersianUserError(ex, "تهیه خروجی Excel استعلام") });
            }
        }
        [HttpPost]

        public async Task<ActionResult> GetAllInvoiceWithInqFromServer(SearchParameters searchFilter, string? operationId)
        {
            try
            {
                var list = await BMain.GetAllInvoiceWithInqFromServerAsync(searchFilter, operationId);
                var SendSuccess = list.Where(a => a.fSendState == 4).ToList();

                var result = Json(new
                {
                    success = true,
                    Data = list,

                });

                return result;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogError($"::{MethodBase.GetCurrentMethod().DeclaringType.Name}::GetAllInvoiceWithInqFromServer::{ex.Message}", ex);
                return Json(new { success = false, message = GetPersianUserError(ex, "استعلام فاکتورها") });
            }
        }

        [HttpGet]
        public ActionResult GetStore()
        {
            try
            {
                var stores = BMain.GetStore();
                stores.Insert(0,new DTOStore() {
                Name="همه",ParentID=0,StoreID=-1,TypeID=-1});
                var result = Json(new
                {
                    success = true,
                    Data = stores,
                });

                return result;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public ActionResult GetErrorType()
        {
            try
            {
                var stores = BMain.GetErrorType();

                var result = Json(new
                {
                    success = true,
                    Data = stores,
                });

                return result;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<ActionResult> CheckConnection()
        {
            try
            {
                //var smtpSettings = new SmtpSettings
                //{
                //    Host = "smtp.gmail.com",
                //    Port = 465,
                //    EnableSSL = true,
                //    Username = "arghamnegar2@gmail.com",
                //    Password = "emox zxyk dkzu bzof " // App password from Google (not regular Gmail password)
                //};

                //// Instantiate EmailSender manually
                //var emailSender = new EmailSender(smtpSettings);

                //string toEmail = "office@arghamnegargroup.com";
                // //toEmail = "mahmodifard.bussines@gmail.com";
                //string subject = "گزارش وضعیت مودیان " + BMain.SrvInfo.CustomerName;
                //string message = "گزارش وضعیت فاکتورهای ارسالی(" + BMain.SrvInfo.CustomerName + ") تا تاریخ ::" + MD.PersianDateTime.PersianDateTime.Now;
                //message += "\r\n";
                //message += "\r\na";
                //await emailSender.SendEmailAsync(toEmail, subject, message);

                var checkResult = await BMain.CheckConnectionAsync();

                var result = Json(new
                {
                    success = true,
                    Data = checkResult,

                });

                return result;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
