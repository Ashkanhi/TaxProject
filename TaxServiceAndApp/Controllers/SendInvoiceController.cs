
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Reflection;
using TaxServiceAndApp.Models;
using System.Web.Mvc;
using System.Threading.Tasks;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models.SharedDTO;
using System;
using System.Collections.Generic;
using NPSTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace TaxServiceAndApp.Controllers
{
    public class SendInvoiceController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetInvoiceStatusByDateRangeAsync(string fromdate, string todate)
        {
            try
            {
                var res = await BMain.GetInvoiceStatusByDateRangeAsync(fromdate, todate);
                var result = Json(new
                {
                    success = true,
                    res = res,
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = Int32.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetInvoiceStatusByRefrenceAsync(string refId)
        {
            try
            {
                ResultModel res = await BMain.GetInvoiceStatusByRefrenceAsync(refId);

                var result = Json(new
                {
                    success = true,
                    res = res,
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = Int32.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetGroupingInvoiceStatusByRefrenceAsync(List<string> ids)
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
                }, JsonRequestBehavior.AllowGet);

                result.MaxJsonLength = Int32.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                var data = new { ex, success = false };
                return Json(data);
            }
        }
        [HttpPost]
        public async Task<ActionResult> SendInvoiceAsync(SendRequest request)
        {
            try
            {
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogInfoMessage($"::SendController::" +
                    $"SendInvoice:IN:request.invoiceId::" + request.invoiceId);

                var res = await BMain.SendInvoiceAsync(request.invoiceId);
                var result = Json(new
                {
                    success = true,
                    Data = res,
                }, JsonRequestBehavior.AllowGet);
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                   .LogInfoMessage($"::SendController::" +
                   $"SendInvoice:OUT:request.invoiceId::" + request.invoiceId
                   + res.ToString()
                   );
                result.MaxJsonLength = Int32.MaxValue;
                return result;

            }
            catch (Exception ex)
            {
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                    LogError($"::{MethodBase.GetCurrentMethod().DeclaringType.Name}::" +
                    $"SendController::ID::"
                    + "SendInvoice::" + ex.Message + "::" + ex?.InnerException?.Message, ex);

                var data = new { ex, success = false };
                return Json(data);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SendGroupingInvoiceAsync(List<string> stringList)
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";

                List<Guid> idList = stringList.Select(Guid.Parse).ToList();

                if (idList != null && idList.Count > 0)
                {
                    var res = await BMain.SendGroupInvoicesAsync(idList);
                    if (res.Success == true)
                    {
                        var result = Json(new
                        {
                            success = true,
                            res = res,
                            countOfSend = res.Count
                        }, JsonRequestBehavior.AllowGet);

                        result.MaxJsonLength = Int32.MaxValue;
                        return result;
                    }
                    else
                    {
                        var data = new { res, message = res.Message, success = false };
                        return Json(data);
                    }
                }
                else
                {
                    var data = new { message = "فاکتوری انتخاب نشده است", success = false };
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
        public async Task<ActionResult> SendInvoiceDataAsync(SendRequest request)
        {
            try
            {
                var res = await BMain.SendInvoiceAsync(request.invoiceId, request.invoice);
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
        public async Task<ActionResult> GetInvoiceAsync(string invoiceId)
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
        public async Task<ActionResult> GetInvoiceByIDAsync(string invoiceID)
        {
            try
            {
                var res = await BMain.GetInvoiceByID(invoiceID);
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
        public async Task<ActionResult> CancelInvoiceAsync(string invoiceId)
        {
            try
            {

                var res = await BMain.SendInvoiceAsync(invoiceId, true);
                var data = new { res, success = true };
                return Json(data);
            }
            catch (Exception ex)
            {

                var data = new { ex, success = false };
                return Json(data);
            }
        }

        public async Task<ActionResult> SendCancelInvoiceAsync()
        {
            //Logger<TaxService> loger = new Logger<TaxService>(new LoggerFactory());
            //IService _Service = new TaxService(_Context, loger);
            var res = await BMain.SendInvoiceAsync("");
            return Json(res);
        }

        /// <summary>
        /// متد اصلی فیلتر برای بارگذاری داده در دیتاتیبل
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetAllInvoiceAsync(SearchParameters search)
        {
            try
            {
                var list = await BMain.GetAllInvoiceAsync(search);
                var SendSuccess = list.Where(a => a.SendStat == 4).ToList();
                return Json(new { success = true, list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
