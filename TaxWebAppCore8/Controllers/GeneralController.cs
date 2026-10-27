using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NPSDTOModels.DBDTO.Request;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using TaxCollectData.Library.Abstraction.Clients;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI.MessagingService;

namespace TaxWebAppCore8.Controllers
{
    public class GeneralController : Controller
    {


        public GeneralController(IHubContext<RequestsHub> hubContext)
        {
            BMain._hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);
            return Ok("Message sent to RabbitMQ");
        }
        public ActionResult InquryIndex()
        {
            if (NewTaxController.IsNewTax) return RedirectToAction("Inquiry", "NewTax");
            return View("~/Views/Home/InquryMainPage.cshtml");
        }

        // GET: General
        [HttpPost]
        public async Task<ActionResult> GetEntityCommentByID(Guid ID)
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";

                var result = await BGeneral.GetEntityCommentByIDAsync(ID);
                JsonResult js = Json(new
                {
                    success = true,
                    Data = result
                });

                return js;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogError("GetEntityCommentByEntityID()", ex);
                return Json(new { success = false, responseText = "عملیات ناموفق در سمت سرور" });

            }
        }
        [HttpPost]
        public async Task<ActionResult> DeleteEntityCommentByID(Guid ID)
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";

                var result = await BGeneral.DeleteEntityCommentByIDAsync(ID);
                JsonResult js = Json(new
                {
                    success = true,
                    Data = result
                });

                return js;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogError("GetEntityCommentByEntityID()", ex);
                return Json(new { success = false, responseText = "عملیات ناموفق در سمت سرور" });

            }
        }

        [HttpPost]
        public async Task<ActionResult> GetEntityCommentByUserID()
        {

            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";
                var userUID = default(Guid);// بعدا سیستم لاگین نوشته شد باید شناسه 
                var result = await BGeneral.GetEntityCommentByUserIDAsync(userUID);
                JsonResult js = Json(new
                {
                    success = true,
                    Data = result
                });


                return js;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogError("GetEntityCommentByEntityID()", ex);
                return Json(new { success = false, responseText = "عملیات ناموفق در سمت سرور" });

            }
        }
        [HttpPost]
        public async Task<ActionResult> AddEntityComment(DTOEntityComment comment)
        {
            try
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                Response.ContentType = "application/json; charset=utf-8";
                if (comment.fPersonUniqueID == default(Guid))
                    comment.fPersonUniqueID = default(Guid);

                var result = await BGeneral.AddEntityCommentAsync(comment);
                JsonResult js = Json(new
                {
                    success = true,
                    Data = result
                });


                return js;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogError("AddEntityCommentAsync()", ex);
                return Json(new { success = false, responseText = "عملیات ناموفق در سمت سرور" });

            }
        }

    }
}
