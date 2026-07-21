using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TaxCollectData.Library.Dto;
using TaxCollectData.Library.Models;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using TaxWebAppCore8.Models.SharedDTO;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;
using static ClsTools.NPSEnum;
using static NPSDTOModels.EnumsAndConsts.ClsEnums;
using static System.Runtime.InteropServices.JavaScript.JSType;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Data;
using System.Data.Entity;
using System.Reflection;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using TaxCollectData.Library.Algorithms;
using TaxCollectData.Library.Providers;
namespace TaxServiceAndApp.Models
{
    public class TissServise
    {
        private static bool NeedInit = true;
        private readonly IContext _context;
        private static ServerInformationModel ServerInformationModel;
        private static TokenModel TokenModel;
        #region singleTone
        private static TissServise _tisServise = null;
        public static TissServise getInstance(IContext context)
        {
            if (_tisServise == null)
                _tisServise = new TissServise(context);
            return _tisServise;
        }


        public TissServise(IContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11
                    | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 5, 0);
                    client.BaseAddress = new Uri(BMain.SrvInfo.API_URL);

                    client.DefaultRequestHeaders.Add("X-OrgId", BMain.SrvInfo.MEMORY_ID);  //New 

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.
                        AuthenticationHeaderValue("Bearer", BMain.SrvInfo.FILE_KEY_PRIVATE.Trim());
                    List<string> refs = new List<string>();
                    refs.Add("");
                    var jrefs = JsonSerializer.Serialize(refs);


                    var content = new StringContent(jrefs.ToString(), Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "inquiry/byReferenceId",
                        content);
                    if (res != null)
                    {



                        var data = await res.Content.ReadAsStringAsync();

                        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(data);
                        var status = retData.FirstOrDefault().status.ToString();



                        return true;
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                   LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
               + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return false;
            }
        }
        List<Invoice> lstStosFactor = new List<Invoice>();
        public async Task<List<TissInvoiceBodyDtoSysDTO>> GetInvoice(DTOInvoiceLimitParameter invoice, bool isCancel = false)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
   LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ " ExecCommandToObject ::GetInvoice");
            Invoice inv = _context.GetLastInvoiceForSend((short)(isCancel == true ? 3 : 1), invoice, 0);
            lstStosFactor.Add(inv);
            if (inv == null)
                return null;

            inv.TissHeader.tins = BMain.SrvInfo.MEMORY_ID;
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
  LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ ":: inv.TisHeader ::inno::" + inv.TissHeader.inno);
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ " ::inv.TisHeader ::inno::" + inv.TissHeader.taxid);

            var date = inv.TissHeader.InvoiceDate ?? DateTime.Now;
            var taxId = BTaxMain.CreateUniqeTaxID(long.Parse(inv.TissHeader.inno.ToString().Replace("-", "")), date);
            invoice.TaxGuid = taxId;

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
               .LogInfoMessage($"::TaxService::" +
                   $"CreateUniqeTaxID::taxId::" + taxId + "::invoiceID::" + invoice.InvoiceID + "::inv.TissHeader.InvoiceDate==" + date.ToString());

            TaxIdProvider taxIdProvider = new TaxIdProvider(new VerhoeffAlgorithm());
            string memoryId = BMain.SrvInfo.MEMORY_ID;
            Random random = new Random();
            DateTimeOffset now = new DateTimeOffset(inv.TissHeader.InvoiceDate ?? DateTime.Now);
            taxId = taxIdProvider.GenerateTaxId(
            memoryId,
            long.Parse(inv.TissHeader.inno.ToString().Replace("-", "")),
            now.DateTime);

            string inno = long.Parse(inv.TissHeader.inno.ToString().Replace("-", ""))
            .ToString("X")
            .PadLeft(10, '0');
            long indatim = now.ToUnixTimeMilliseconds();
            invoice.TaxGuid = taxId;

            inv.TissHeader.indatim = indatim.ToString();
            inv.TissHeader.Indati2m = (int?)indatim;
            inv.TissHeader.taxid = taxId.ToString();
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
      .LogInfoMessage($"::TaxService::" +
          $"CreateUniqeTaxID::NewtaxId::" + taxId + "::indatim" + indatim + "::invoiceID::" + invoice.InvoiceID);


            TissPaymentDto_H pt = new TissPaymentDto_H();
            var paymentDto = new PaymentItemDto()
            {
                pmt = pt.Pmt,
                acn = pt.Acn,
                iinn = pt.Iinn,
                pcn = pt.Pcn,
                pdt = pt.Pdt,
                pid = pt.Pid,
                pv = pt.Pv,
                trmn = pt.Trmn,
                trn = pt.Trn
            };


            List<PaymentItemDto> payments = new List<PaymentItemDto>();
            payments.Add(paymentDto);
            var invoiceDto = new TissInvoiceBodyDtoSysDTO()
            {
                retry = true,
                header = inv.TissHeader,
                body = inv.TisRows,
                payments = payments
            };
            List<TissInvoiceBodyDtoSysDTO> list = new List<TissInvoiceBodyDtoSysDTO>();
            list.Add(invoiceDto);
            return list;

        }
        private object GetGroupingInvoice(string invoiceId, bool isCancel)
        {
            List<TissInvoiceBodyDtoSysDTO> list = new List<TissInvoiceBodyDtoSysDTO>();
            try
            {

                var invs = _context.GetGroupOfInvoiceForSend((short)(isCancel == true ? 3 : 1), invoiceId, true);

                if (invs == null)
                    return null;

                foreach (var inv in invs)
                {


                    inv.TissHeader.tins = BMain.SrvInfo.TINS;
                    var date = inv.TissHeader.TaxIdDate;


                    //PaymentDto pt = new PaymentDto() { Pmt = 3, Pv = 0 };
                    //var paymentDto = new
                    //    {
                    //    pmt = pt.Pmt,
                    //    acn = pt.Acn,
                    //    iinn = pt.Iinn,
                    //    pcn = pt.Pcn,
                    //    pdt = pt.Pdt,
                    //    pid = pt.Pid,
                    //    pv = pt.Pv,
                    //    trmn = pt.Trmn,
                    //    trn = pt.Trn


                    //    };

                    TissPaymentDto_H pt = new TissPaymentDto_H();
                    var paymentDto = new PaymentItemDto()
                    {
                        pmt = pt.Pmt,
                        acn = pt.Acn,
                        iinn = pt.Iinn,
                        pcn = pt.Pcn,
                        pdt = pt.Pdt,
                        pid = pt.Pid,
                        pv = pt.Pv,
                        trmn = pt.Trmn,
                        trn = pt.Trn

                    };
                    List<PaymentItemDto> payments = new List<PaymentItemDto>();
                    payments.Add(paymentDto);
                    var invoiceDto = new TissInvoiceBodyDtoSysDTO()
                    {
                        retry = true,
                        header = inv.TissHeader,
                        body = inv.TisRows,
                        payments = payments
                    };

                    list.Add(invoiceDto);
                }
            }
            catch (Exception ex)
            {

            }
            return list;
        }

        #endregion
        public async Task Init()
        {
            if (NeedInit)
            {

                NeedInit = false;
            }
        }
        public async Task<List<InquiryResultModel>> GetInvoiceStatusByDateRange(string fromdate, string todate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Original 2
        /// </summary>
        /// <param name="refId"></param>
        /// <returns></returns>
        public async Task<ResultModel> GetInvoiceStatusByRefrenceAsync(string invoiceID)
        {
            ResultModel result = new ResultModel();
            try
            {

                await GetAllInvoiceByInqFromServer(new SearchParameters() { InvoiceID = Guid.Parse(invoiceID) });

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogError($"::TissService::" +
$"PostAsync:model " + ex.Message + ex.InnerException?.Message, ex);
                result.Success = false;
                result.Data = ex;
                result.Message = ex.Message;
                //_logger.LogError("TisService SendInvoiceAsync:" + ex.ToString());
            }
            return result;
        }
        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoiceByInqFromServer(SearchParameters search)
        {
            List<GeneralInvoiceHeaderViewModel> list = new List<GeneralInvoiceHeaderViewModel>();
            try
            {
                //search.StorID = BMain.SrvInfo.StoreId;
                list = (await _context.GetAllInvoice(search));

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogInfoMessage($"::TissService::" +
$"PostAsync:model::" + list.Count.ToString());

                list = list.Where(a => !string.IsNullOrEmpty(a.ReferenceNumber) ||
                !string.IsNullOrEmpty(a.fTaxHistoryReferenceNumber)
                || !string.IsNullOrEmpty(a.taxReferenceNumber)).ToList();

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogInfoMessage($"::TissService::" +
$"PostAsync:model::" + list.Count.ToString());

                if (list != null && list.Any() && list.Count > 0)
                    list = await GetInvoiceStatusByReferenceListEnhanced(list);
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogError($"::TissService::" +
$"PostAsync:model " + ex.Message + ex.InnerException?.Message, ex);

            }
            return list;

        }
        /// <summary>
        /// دریافت استعلام
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>


        public async Task<List<GeneralInvoiceHeaderViewModel>>
            GetInvoiceStatusByReferenceListEnhanced(List<GeneralInvoiceHeaderViewModel> models)
        {
            ResultModel result = new ResultModel();

            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogInfoMessage($"::TissService::" +
                       $"GetInvoiceStatusByReferenceListEnhanced:Out: ");

                // Reusing HttpClient (consider using IHttpClientFactory in production)
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
                {
                    client.BaseAddress = new Uri(BMain.SrvInfo.API_URL);
                    client.DefaultRequestHeaders.Add("X-OrgId", BMain.SrvInfo.MEMORY_ID); // New header
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BMain.SrvInfo.FILE_KEY_PRIVATE.Trim());

                    var references = models.Where(a => a.fSendState == 2).Select(m => m.fTaxHistoryReferenceNumber).ToList();
                    var jsonRefs = JsonSerializer.Serialize(references);

                    var content = new StringContent(jsonRefs, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{BMain.SrvInfo.API_URL}inquiry/byReferenceId", content);
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
          .LogInfoMessage($"::TissService::" +
                     $"PostAsync:StatusCode: " + response.StatusCode);

                    if (response != null && response.IsSuccessStatusCode)
                    {
                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
          .LogInfoMessage($"::TissService::" +
                     $"PostAsync:response != null && response.IsSuccessStatusCode: ");
                        result.Success = true;
                        var responseData = await response.Content.ReadAsStringAsync();
                        var inquiries = JsonConvert.DeserializeObject<List<InquiryByRef>>(responseData);
                        int countofAll = inquiries.Count;
                        int countofSend = 0;
                        foreach (var model in models)
                        {
                            string status = "";

                            var inquiry = inquiries.FirstOrDefault(a => a.referenceNumber == model.fTaxHistoryReferenceNumber);
                            if (inquiry == null)
                            {
                                model.InqMessage = model.fListOfErrorCaption;
                                status = model.SendStatStr;
                            }
                            else
                            {
                                //var model = models.FirstOrDefault(m => m.fTaxHistoryReferenceNumber == inquiry.referenceNumber);
                                //if (model == null) continue;
                                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
             .LogInfoMessage($"::TissService::" +
                        $"PostAsync:model " + inquiry.status);

                                model.ResultModelInquiryByRef = inquiry;
                                result.Status = inquiry.status.ToString();

                                await SaveInvoiceInquiryLog(model.fTaxHistoryReferenceNumber, inquiry);

                                switch (inquiry.status.ToString().ToLower())
                                {
                                    case "success":
                                        status = "ثبت موفق";
                                        await HandleSuccessAsync(model, inquiry);
                                        break;
                                    case "failed":
                                        status = "خطا";
                                        await HandleFailedAsync(model, inquiry);
                                        break;
                                    case "pending":
                                        status = "در حال بررسی";
                                        await HandlePendingAsync(model, inquiry);
                                        break;
                                    case "gatewaytimeout":
                                        result.Message = $"Gateway Timeout: {inquiry.data.error.FirstOrDefault()?.message}";
                                        result.Success = false;
                                        break;
                                    default:
                                        status = "فاکتور پیدا نشد";
                                        using (var npsContext = new NPSGeneralContext())
                                        {
                                            var errror = new errorDetail()
                                            {
                                                code = "0000",
                                                details = "فاکتور پیدا نشد!",
                                                message = "فاکتور پیدا نشد!"
                                            };
                                            await _context.LogErrorsAsync(npsContext, new List<errorDetail> { errror });
                                            var taxhistory = await
                                            npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToString().ToLower() == model.InvoiceId.ToLower());
                                            if (taxhistory == null)
                                            {
                                                taxhistory = new tTaxHistoryStateParallel();
                                                taxhistory.ReferenceNumber = model.fTaxHistoryReferenceNumber;
                                                taxhistory.fInvoiceID = Guid.Parse(model.InvoiceId);
                                            }
                                            {
                                                taxhistory.fListOfErrorCaption =
                                                    string.Join('-', "فاکتور پیدا نشد!");
                                                //taxhistory.fSendState = 1;

                                                taxhistory.fListOfErrorCode =
                                                    string.Join('-', "فاکتور پیدا نشد!");
                                                npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                                            }
                                            await npsContext.SaveChangesAsync();
                                        }
                                        result.Message = $"Unknown Status: {inquiry.status}";
                                        model.InqMessage = "فاکتور پیدا نشد!";

                                        _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "Unknown Error", 0, "", model.fTaxHistoryReferenceNumber);
                                        result.Success = false;
                                        break;
                                }
                            }

                            var message = "فاکتور شماره " + model.InvoiceNumberSt + " با موفقیت احراز شد. \r\n" +
                                       "وضعیت ثبت: " + status + "\r\n " +
                                           "\r\n تعداد : " + countofSend +
                                          " از " + countofAll;

                            await BMain._hubContext.Clients.All.SendAsync("SendToTaxInq", message);
                            countofSend++;
                        }
                    }
                    else
                    {
                        foreach (var item in models)
                        {
                            if (item.fSendState != 2)
                            {
                                item.InqMessage = item.fListOfErrorCaption;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
         .LogError($"::TissService::" +
                    $"PostAsync:model " + ex.Message + ex.InnerException?.Message, ex);

                result.Success = false;
                result.Data = ex;
                result.Message = ex.Message;
            }

            return models;
        }

        private async Task SaveInvoiceInquiryLog(string referenceNumber, InquiryByRef inquiry)
        {
            _context.SaveInvoiceInquiryLog(
                referenceNumber,
                inquiry.status.ToString(),
                inquiry?.taxId,
                inquiry?.data?.ToString(),
                "a"
            );
        }

        private async Task HandleSuccessAsync(GeneralInvoiceHeaderViewModel model, InquiryByRef inquiry)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogInfoMessage($"::TissService::" +
                       $"HandleSuccessAsync:Out: " + model.InvoiceId.ToString() + "::" + inquiry
                       .message);

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "SUCCESS", 1, inquiry.taxId.ToString(), model.fTaxHistoryReferenceNumber);

            using (var npsContext = new NPSGeneralContext())
            {
                var maxLastOrder = await npsContext.
                                      tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToString().ToLower() == model.InvoiceId.ToLower()).
                                      OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                if (maxLastOrder != null)
                {
                    maxLastOrder.fStatus = 4;
                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(maxLastOrder);
                    await npsContext.SaveChangesAsync();

                }
                else
                {
                    maxLastOrder = new tSendInvoiceTaxTissLog()
                    {
                        fInvoiceID = Guid.Parse(model.InvoiceId),
                        fLastSendDate = DateTime.Now,
                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                        fReferencedID = model.fTaxHistoryReferenceNumber,
                        fStatus = 4,
                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                    };
                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, maxLastOrder);
                    await npsContext.SaveChangesAsync();
                }
                //var maxLastOrder = await npsContext.
                //    tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower()).
                //    OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                //tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                //{
                //    fInvoiceID = model.InvoiceId,
                //    fLastSendDate = DateTime.Now,
                //    fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                //    fReferencedID = model.fTaxHistoryReferenceNumber,
                //    fStatus = (short)InquiryStatus.Success,
                //    fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                //};
                var taxhistory = await
                  npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToString().ToLower() == model.InvoiceId.ToLower());
                if (taxhistory == null)
                {
                    taxhistory = new tTaxHistoryStateParallel();
                    taxhistory.ReferenceNumber = model.fTaxHistoryReferenceNumber;
                    taxhistory.fInvoiceID = Guid.Parse(model.InvoiceId);
                }
                if (taxhistory != null)
                {
                    taxhistory.fListOfErrorCaption = "ثبت موفق";
                    taxhistory.fListOfErrorCode = "ثبت موفق";
                    taxhistory.fSendState = 4;
                    npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                }
                await npsContext.SaveChangesAsync();
            }

            model.InqMessage = "فاکتور با موفقیت ثبت شده است.";
        }

        private async Task HandleFailedAsync(GeneralInvoiceHeaderViewModel model, InquiryByRef inquiry)
        {
            var errorMessages = string.Join("::", inquiry.data.error.Select(e => e.message));
            var warningMessages = string.Join("::", inquiry.data.warning.Select(w => w.message));

            using (var npsContext = new NPSGeneralContext())
            {
                try
                {

                    await _context.LogErrorsAsync(npsContext, inquiry.data.error);
                    var maxLastOrder = await npsContext.
                                     tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToString().ToLower() == model.InvoiceId.ToLower()).
                                     OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                    if (maxLastOrder != null)
                    {
                        maxLastOrder.fStatus = 3;
                        npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(maxLastOrder);
                        await npsContext.SaveChangesAsync();

                    }
                    else
                    {
                        maxLastOrder = new tSendInvoiceTaxTissLog()
                        {
                            fInvoiceID = Guid.Parse(model.InvoiceId),
                            fLastSendDate = DateTime.Now,
                            fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                            fReferencedID = model.fTaxHistoryReferenceNumber,
                            fStatus = 3,
                            fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                        };
                        npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, maxLastOrder);
                        await npsContext.SaveChangesAsync();
                    }
                    //var maxLastOrder = await
                    //        npsContext.tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower()).
                    //        OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                    //tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                    //{
                    //    fInvoiceID = model.InvoiceId,
                    //    fLastSendDate = DateTime.Now,
                    //    fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                    //    fReferencedID = model.fTaxHistoryReferenceNumber,
                    //    fStatus = (short)InquiryStatus.Failed,
                    //    fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                    //};
                    //npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, tSendInvoice);
                    //await npsContext.SaveChangesAsync();
                    foreach (var error in inquiry.data.error)
                    {
                        try
                        {
                            var existingError = await npsContext.tTisTaxErrorTypes
                                .FirstOrDefaultAsync(e => e.fType == 1 && e.fCode.ToLower().Trim() == error.code.ToLower().Trim());

                            if (existingError == null)
                            {
                                var newError = new tTisTaxErrorType
                                {
                                    fCaption = error.message,
                                    fDescription = error.message,
                                    fCode = error.code,
                                    fType = 1,
                                };
                                npsContext.tTisTaxErrorTypes.AddOrUpdate(e => e.fID, newError);
                                await npsContext.SaveChangesAsync();
                                existingError = await npsContext.tTisTaxErrorTypes
                                .FirstOrDefaultAsync(e => e.fType == 1 && e.fCode.ToLower().Trim() == error.code.ToLower().Trim());
                            }

                            var tErrorslog = new tErrorsOfLog()
                            {
                                fCaption = error.message,
                                fErrorCode = error.code,
                                fErrorType = existingError.fID,
                                fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                fSendLogUID = maxLastOrder?.fLogUID,
                                fInvoiceId = model.InvoiceId,

                            };
                            npsContext.tErrorsOfLogs.AddOrUpdate(e => e.fLogUID, tErrorslog);
                            await npsContext.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error logging error code {error.code}: {ex.Message}");
                        }
                    }
                    var taxhistory = await
                        npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToString().ToLower() == model.InvoiceId.ToLower());
                    if (taxhistory == null)
                    {
                        taxhistory = new tTaxHistoryStateParallel();
                        taxhistory.ReferenceNumber = model.fTaxHistoryReferenceNumber;
                        taxhistory.fInvoiceID = Guid.Parse(model.InvoiceId);
                    }
                    {
                        taxhistory.fListOfErrorCaption =
                            string.Join('-', inquiry.data.error.Select(a => a.message).ToArray());
                        taxhistory.fSendState = 3;

                        taxhistory.fListOfErrorCode =
                            string.Join('-', inquiry.data.error.Select(a => a.code).ToArray());
                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                    }
                    await npsContext.SaveChangesAsync();
                }
                catch (Exception er)
                {

                    Console.WriteLine(er.Message);
                }
            }

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "FAILED", 0, inquiry.taxId.ToString(), model.fTaxHistoryReferenceNumber);

            model.InqMessage = $"فاکتور به دلیل این خطاها ثبت نگردید : {errorMessages}";
        }


        private async Task HandlePendingAsync(GeneralInvoiceHeaderViewModel model, InquiryByRef inquiry)
        {
            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "PENDING", 2, inquiry.taxId.ToString(), model.fTaxHistoryReferenceNumber);
            using (var npsContext = new NPSGeneralContext())
            {
                var maxLastOrder = await npsContext.tSendInvoiceTaxTissLogs.
                    Where(a => a.fInvoiceID.ToString().ToLower() == model.InvoiceId.ToLower()).OrderByDescending(a => a.fID)
                    .FirstOrDefaultAsync();

                if (maxLastOrder != null)
                {
                    maxLastOrder.fStatus = 2;
                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(maxLastOrder);
                    await npsContext.SaveChangesAsync();

                }
                else
                {
                    maxLastOrder = new tSendInvoiceTaxTissLog()
                    {
                        fInvoiceID = Guid.Parse(model.InvoiceId),
                        fLastSendDate = DateTime.Now,
                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                        fReferencedID = model.fTaxHistoryReferenceNumber,
                        fStatus = 2,
                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                    };
                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, maxLastOrder);
                    await npsContext.SaveChangesAsync();
                }

                var taxhistory = await
                   npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToString().ToLower() == model.InvoiceId.ToLower());
                if (taxhistory != null)
                {
                    taxhistory.fListOfErrorCaption = "درحال بررسی";
                    taxhistory.fListOfErrorCode = "درحال بررسی";
                    npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                }
                if (taxhistory == null)
                {
                    taxhistory = new tTaxHistoryStateParallel();
                    taxhistory.ReferenceNumber = maxLastOrder.fReferencedID;
                    taxhistory.fInvoiceID = Guid.Parse(model.InvoiceId);
                }
                {
                    taxhistory.fSendState = 2;

                    taxhistory.fListOfErrorCaption = "درحال بررسی";
                    taxhistory.fListOfErrorCode = "درحال بررسی";
                    npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                }
                await npsContext.SaveChangesAsync();
            }
            model.InqMessage = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
        }

        public Task GetNotFinishedLastRefrenceIdAsync()
        {
            throw new NotImplementedException();
        }



        //public async Task<ResultModel> SendInvoiceAsync(string invoiceId, bool isCancel = false)
        //{
        //    ResultModel result = new ResultModel();
        //    try
        //    {

        //        var CurrentInvoice = await GetInvoice(invoiceId, isCancel);
        //        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
        //        .LogInfoMessage($"::TissService::" +
        //            $"SendInvoiceAsync:IN:request.invoiceId::" + CurrentInvoice.ToString());

        //        if (CurrentInvoice == null)
        //        { result.Message = "No Invoice Found"; }
        //        else
        //        {
        //            var options = new JsonSerializerOptions
        //            {
        //                WriteIndented = true,
        //                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //            };
        //            var data = JsonSerializer.Serialize(CurrentInvoice, options);
        //            data = data.Replace("\r\n", "").Replace("\n", "").Replace("\\", "").Replace("  ", "").Replace("   ", "");
        //            SaveInvoiceJsonToLog(data);

        //            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
        //            .LogInfoMessage($"::TissService::" +
        //                $"SendInvoiceAsync:Out: " + data.ToString());

        //            result = await SendInvoiceAsync(invoiceId, data, isCancel);
        //        }

        //    }
        //    catch (Exception ex)
        //    {



        //        result.Message = ex.Message;
        //        result.Success = false;
        //        result.Data = ex;

        //        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
        //      LogError($"TissService" +
        //      $"SendInvoiceAsync::" + ex.Message + "::" + ex?.InnerException?.Message, ex);

        //    }
        //    return result;
        //}
        public async Task<ResultModel> SendInvoiceAsync(DTOInvoiceLimitParameter invoice, bool isCancel = false)
        {
            ResultModel result = new ResultModel();
            try
            {
                var CurrentInvoice = await GetInvoice(invoice, isCancel);

                if (CurrentInvoice == null)
                { result.Message = "No Invoice Found"; }
                else
                {
                    result.TissBreif = CurrentInvoice.FirstOrDefault();

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    var data = JsonSerializer.Serialize(CurrentInvoice, options);
                    data = data.Replace("\r\n", "").Replace("\n", "").Replace("\\", "").Replace("  ", "").Replace("   ", "");

                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogInfoMessage($"::TissService::" +
                        $"SendInvoiceAsync:Out: " + data.ToString());

                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($"::TissService::" +
                    $"SendInvoiceAsync_2 :IN" + invoice.InvoiceID.ToString());

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 5, 0);
                        // Client.BaseAddress = new Uri(taxSetting.Url);

                        client.DefaultRequestHeaders.Add("X-orgid", BMain.SrvInfo.MEMORY_ID);  //New
                        var token = BMain.SrvInfo.FILE_KEY_PRIVATE.Trim().Replace("\r\n", "");
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        //var content = new StringContent(invoice.ToString(), Encoding.UTF8, "application/json");
                        //var res = client.PostAsync("api/invoices/async", content).Result; 
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BMain.SrvInfo.API_URL + "invoices/async")
                        {
                            Content = new StringContent(data, null, "application/json")
                        };

                        HttpResponseMessage res = await client.SendAsync(request);
                        if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                        {
                            var resultofSend = await res.Content.ReadAsStringAsync();
                            List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(resultofSend);

                            foreach (var r in retData)
                            {
                                var referenceNumber = r.referenceNumber;

                                _context.CreateSaveInvoiceResult(invoice.InvoiceID, referenceNumber, r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber);
                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoice.InvoiceID, r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber, isCancel);
                                using (var context = new NPSGeneralContext())
                                {
                                    var updatetax = await context.tTaxHistoryStateParallels.FirstOrDefaultAsync
                                        (a => a.fInvoiceID.ToString().ToLower() == invoice.InvoiceID.ToString().ToLower());
                                    if (updatetax == null)
                                    {
                                        updatetax = new tTaxHistoryStateParallel()
                                        {
                                            fInvoiceID = Guid.Parse(invoice.InvoiceID),
                                            fLastSendDate = DateTime.Now,
                                            fListOfErrorCaption = "در حال بررسی",
                                            fListOfErrorCode = "در حال بررسی",
                                            fSendState = 2,
                                            ReferenceNumber = referenceNumber
                                        };

                                    }
                                    updatetax.ReferenceNumber = referenceNumber;
                                    updatetax.fLastSendDate = DateTime.Now;
                                    updatetax.fSendState = 2;
                                    updatetax.fListOfErrorCaption = "در حال بررسی";
                                    updatetax.fListOfErrorCode = "در حال بررسی";
                                    context.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, updatetax);
                                    await context.SaveChangesAsync();
                                }
                                result.Success = true;
                                result.Message = "با موفقیت ثبت شد.";
                                // result.Data = packetres;
                            }
                        }
                        else
                        {
                            var resultOfSend = await res.Content.ReadAsStringAsync();
                            result.Success = false;
                            result.Message = "ارتباط با سامانه مودیان برقرار نشد";

                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                        .LogInfoMessage($"::TissService::" +
                        $"SendInvoiceAsync_2 :IN" + resultOfSend.ToString());

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Data = ex;
                result.Message = ex.Message;
                //_logger.LogError("TaxService SendInvoiceAsync:" + ex.ToString());

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
        LogError($"TissService" +
            $"SendInvoiceAsync_2::" + ex.Message + "::" + ex?.InnerException?.Message, ex);


            }
            return result;
        }

        public async Task<ResultModel> SendGroupInvoices2(List<Guid> Ids)
        {
            ResultModel result = new ResultModel();
            result.Count = 0;
            try
            {
                var idsST = "";
                foreach (var item in Ids)
                {
                    idsST += item.ToString();

                }
                var invoices = GetGroupingInvoice(idsST, false);
                if (invoices != null)
                {

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    var data = JsonSerializer.Serialize(invoices, options);
                    data = data.Replace("\r\n", "").Replace("\n", "").Replace("\\", "").Replace("  ", "").Replace("   ", "");
                    //SaveInvoiceJsonToLog(data);

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 5, 0);
                        // client.BaseAddress = new Uri(taxSetting.Url);

                        client.DefaultRequestHeaders.Add("X-OrgId", BMain.SrvInfo.MEMORY_ID);  //New 
                        var token = BMain.SrvInfo.FILE_KEY_PRIVATE;
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        //var content = new StringContent(invoice.ToString(), Encoding.UTF8, "application/json");
                        //var res = client.PostAsync("api/invoices/async", content).Result;
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BMain.SrvInfo.API_URL + "invoices/async")
                        {
                            Content = new StringContent(data.ToString(), null, "application/json")
                        };

                        HttpResponseMessage res = await client.SendAsync(request);
                        if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                        {


                            var rdata = await res.Content.ReadAsStringAsync();
                            List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(rdata);

                            foreach (var r in retData)
                            {

                                var referenceNumber = r.referenceNumber;




                                //_context.CreateSaveInvoiceResult(invoiceId, referenceNumber, r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber);
                                //_context.TaxGovermentInvoiceFinalInsertUpdate(invoiceId, r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber, isCancel);

                                result.Success = true;
                                result.Message = "با موفقیت ثبت شد.";
                                // result.Data = packetres;
                            }

                        }
                        else
                        {

                            result.Success = false;
                            result.Message = "ارتباط با سامانه مودیان برقرار نشد";

                        }

                    }



                }
                else
                    result.Message = "No Invoice Found"; result.Success = false;

                result.Success = true;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

                result.Data = ex;
                result.Message = ex.Message;
                //_logger.LogError("TissService SendGroupInvoices:" + ex.ToString());
                result.Success = false;
            }
            return result;
        }

        /// <summary>
        /// Original 1
        /// </summary>
        /// <param name="InvoiceID"></param>
        /// <returns></returns>
        public async Task<ResultModel> GetInvoiceByID(string InvoiceID)
        {
            ResultModel result = new ResultModel();
            try
            {

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11
                    | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 5, 0);
                    client.BaseAddress = new Uri(BMain.SrvInfo.API_URL);

                    client.DefaultRequestHeaders.Add("X-OrgId", BMain.SrvInfo.MEMORY_ID);  //New 

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.
                        AuthenticationHeaderValue("Bearer", BMain.SrvInfo.FILE_KEY_PRIVATE);
                    List<string> refs = new List<string>();
                    refs.Add(InvoiceID);
                    var jrefs = JsonSerializer.Serialize(refs);


                    var content = new StringContent(jrefs.ToString(), Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "inquiry/byInvoiceId",
                        content);
                    if (res != null)
                    {


                        result.Success = true;
                        // result.Data = packetres;


                        var data = await res.Content.ReadAsStringAsync();

                        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(data);
                        result.Status = retData.FirstOrDefault().status.ToString();
                        switch (retData.FirstOrDefault().status.ToString().ToLower())
                        {
                            case "success":
                                //SaveInvoiceInquiryLog(string refId, string status, string uId, string data, string packetType)

                                _context.TaxGovermentInvoiceFinalInsertUpdate(InvoiceID, "فاکتور با موفقیت ثبت شده است", 1,
                                    retData.FirstOrDefault().taxId.ToString(), InvoiceID);
                                //void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel);
                                result.Message = "فاکتور با موفقیت ثبت شده است";
                                break;
                            case "failed":
                                _context.SaveInvoiceInquiryLog(InvoiceID, "failed", retData.FirstOrDefault().taxId,
                                    retData.FirstOrDefault().data.ToString(), "a");

                                //_context.TaxGovermentInvoiceFinalInsertUpdate(refId, "فاکتور با موفقیت ثبت شده است", 1, retData.FirstOrDefault().taxId.ToString(), refId);
                                _context.TaxGovermentInvoiceFinalInsertUpdate(InvoiceID,
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message, 0,
                                    retData.FirstOrDefault().taxId.ToString(), InvoiceID);

                                //_logger.LogError("TissService GetInvoiceStatusByRefrenceAsync:" + retData.FirstOrDefault().data.error.FirstOrDefault().message);
                                result.Message = "فاکتور به دلیل  :" +
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message
                                    + " ثبت نشده است";

                                break;
                            case "pending":
                                result.Message = "فاکتور هنوز تعیین تکلیف نشده است";

                                _context.TaxGovermentInvoiceFinalInsertUpdate(InvoiceID, "فاکتور هنوز تعیین تکلیف نشده است",
                                    2, retData.FirstOrDefault().taxId.ToString(), InvoiceID);

                                _context.SaveInvoiceInquiryLog(InvoiceID, "Pendding", retData.FirstOrDefault().taxId,
                                    retData.FirstOrDefault().data.ToString(), "a");
                                break;
                            case "gatewaygimeout":
                                result.Message = "gatewaygimeout" + retData.FirstOrDefault().data.error.FirstOrDefault().message;
                                result.Success = false;
                                break;

                            default:
                                result.Message = "نامشخص:" + retData.FirstOrDefault().status.ToString();
                                _context.TaxGovermentInvoiceFinalInsertUpdate(InvoiceID, "خطای نامشخص روی داده است", 0, "", InvoiceID);
                                result.Success = false;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

                result.Success = false;
                result.Data = ex;
                result.Message = ex.Message;
                //_logger.LogError("TisService SendInvoiceAsync:" + ex.ToString());
            }
            return result;
        }

        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search)
        {
            List<GeneralInvoiceHeaderViewModel> list = new List<GeneralInvoiceHeaderViewModel>();
            try
            {
                //.StorID = BMain.SrvInfo.StoreId;
                list = (await _context.GetAllInvoice(search));
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);
            }
            return list;
        }
        public static SemaphoreSlim _SemaphoreSlimForLimitSend = new SemaphoreSlim(10);
        public int countofSend;
        public int countofAll;

        public async Task<ResultModel> SendGroupInvoices(List<DTOInvoiceLimitParameter> invoices)
        {
            ResultModel result = new ResultModel();
            result.Count = 0;

            try
            {
                countofSend = 0;
                countofAll = invoices.Count;
                var tasks = new List<Task>();

                foreach (var item in invoices)
                {
                    try
                    {
                        await _SemaphoreSlimForLimitSend.WaitAsync();

                        await Task.Run(async () =>
                       {
                           try
                           {
                               await DoSendPertoPerAsync(item);
                           }
                           finally
                           {
                               _SemaphoreSlimForLimitSend.Release();
                           }
                       });
                    }
                    finally
                    {
                        _SemaphoreSlimForLimitSend.Release();
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

                result.Data = ex;
                result.Message = ex.Message;
                // _logger.LogError("TissService SendGroupInvoices:" + ex.ToString());
                result.Success = false;
            }

            return result;
        }

        public async Task DoSendPertoPerAsync(DTOInvoiceLimitParameter item)
        {
            try
            {
                Interlocked.Increment(ref countofSend); // Safely increment count

                var res = await SendInvoiceAsync(item, false);
                if (res.Success)
                {
                    var message = @"ارسال فاکتور به شماره: " + res.TissBreif.header.inno +
                        " و مبلغ :" + res.TissBreif.header.tprdis +
                        " با موفقیت ارسال گردید. "
                        + "\r\n تعداد ارسالی: " + countofSend +
                        " از " + countofAll;
                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);
                }
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

            }
        }

        public static SemaphoreSlim _SemaphoreSlimForLimitSendForGrouping
            = new SemaphoreSlim(10);
        public static int countofSendForGrouping;
        public static int countofAllForGrouping;
        public const int MaxBatchSizeInBytes = 2 * 1024 * 1024;
        public const int MaxCurrentCount = 1500;

        public async Task<ResultModel> SendGroupInvoicesForGroupingAsync(List<DTOInvoiceLimitParameter> Ids)
        {
            try
            {
                ResultModel result = new ResultModel();
                result.Count = 0;

                try
                {
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                       LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                   + " SendGroupInvoicesForGroupingAsync ::");
                    countofSendForGrouping = 0;
                    countofAllForGrouping = Ids.Count;

                    var tasks = new List<Task>();

                    // Step 1: Pre-fetch and serialize all invoices once
                    Dictionary<Guid, string> invoiceDataMap = new Dictionary<Guid, string>();
                    countofAll = Ids.Count();
                    countofSend = 0;
                    foreach (var item in Ids)
                    {
                        using (var npsContext = new NPSGeneralContext())
                        {
                            var resultodLastSent = await npsContext.TaxGovermentInvoiceInfoes.CountAsync(a => a.InvoiceId == item.InvoiceGuidID && a.Status == "Sent");
                            var resultodfinalLastSent = await npsContext.TaxGovermentInvoiceFinals.CountAsync
                                (a => a.InvoiceId == item.InvoiceID);
                            var resultofSendTTLog = await npsContext.tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToString().ToLower() == item.InvoiceID.ToLower()).ToListAsync();
                            if (
                                //!resultofSendTTLog.Any(a => a.fStatus == 2 || a.fStatus == 4) &&
                                //!(resultodLastSent > 0 && resultodLastSent > resultodfinalLastSent) &&
                                (item.Status == 1 || item.Status == 3 || item.Status == 0)
                                )
                            {
                                var serializedInvoice = await GetSerializedInvoiceAsync(item, false);
                                if (serializedInvoice != null)
                                {
                                    invoiceDataMap[item.InvoiceGuidID] = serializedInvoice;
                                }
                                else
                                {
                                    countofAll = countofAll - 1;
                                }
                            }
                            else
                            {
                                var parallel = await npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToString().ToLower() == item.InvoiceID.ToLower());
                                if (!string.IsNullOrEmpty(parallel.ReferenceNumber))
                                    parallel.fSendState = 2;
                                await npsContext.SaveChangesAsync();
                                countofAll = countofAll - 1;
                            }
                        }
                    }

                    // Step 2: Create batches based on the size limit (2MB)
                    List<Guid> currentBatch = new List<Guid>();

                    int currentBatchSize = 0;
                    int currentSendCount = 0;

                    if (invoiceDataMap.Count == 0)
                    {
                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                       LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                   + " ExecCommandToObject ::آیتم های بارگذاری نمی شود");
                        result.Data = null;
                        result.Message = "آیتم ها بارگذاری نمی شود";
                        result.Success = false;
                        return result;
                    }
                    foreach (var id in invoiceDataMap.Keys)
                    {
                        var invoiceSize = Encoding.UTF8.GetByteCount(invoiceDataMap[id]);

                        // Check if adding this invoice exceeds the batch size limit
                        if ((invoiceSize + currentBatchSize > MaxBatchSizeInBytes) ||
                           currentSendCount > MaxCurrentCount)
                        {
                            // Send the current batch when size exceeds the limit
                            tasks.Add(SendBatchAsync(currentBatch, invoiceDataMap));

                            // Reset the batch
                            currentBatch = new List<Guid>();
                            currentBatchSize = 0;
                            currentSendCount = 0;
                        }

                        // Add the current invoice to the batch
                        currentBatch.Add(id);
                        currentBatchSize += invoiceSize;
                        currentSendCount += 1;
                    }

                    // Send any remaining batch
                    if (currentBatch.Any())
                    {
                        tasks.Add(SendBatchAsync(currentBatch, invoiceDataMap));
                    }

                    // Await all tasks to complete
                    await Task.WhenAll(tasks);
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                       LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                   + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                    result.Data = ex;
                    result.Message = ex.Message;
                    result.Success = false;
                }

                return result;
            }
            catch (Exception ex2)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                  LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
              + " ExecCommandToObject ::" + ex2.Message + "::" + ex2?.InnerException?.Message, ex2);
                ResultModel result = new ResultModel();

                result.Data = ex2;
                result.Message = ex2.Message;
                result.Success = false;
                return result;
            }
        }
        List<TissInvoiceBodyDtoSysDTO> bodyDtoSysDTOs = new List<TissInvoiceBodyDtoSysDTO>();
        // Helper method to serialize the invoice and return the data as a string
        public async Task<string> GetSerializedInvoiceAsync(DTOInvoiceLimitParameter invoice, bool isCancel)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ " ExecCommandToObject ::GetSerializedInvoiceAsync");
            var Date1 = DateTime.Now;
            var CurrentInvoice = await GetInvoice(invoice, isCancel);
            var timespan = (DateTime.Now - Date1).TotalSeconds;
            if (CurrentInvoice == null || CurrentInvoice.FirstOrDefault() == null) return null;
            if (CurrentInvoice.FirstOrDefault().body.Any(a => string.IsNullOrEmpty(a.sstid)))
            {
                using (var npsContext = new NPSGeneralContext())
                {
                    var taxhistory = await
                       npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
                       .ToString().ToLower() == invoice.InvoiceID.ToLower());

                    {
                        taxhistory.fListOfErrorCaption =
                            "یکی از اقلام شناسه کالا ندارد!";
                        taxhistory.fSendState = 3;

                        taxhistory.fListOfErrorCode = "یکی از اقلام شناسه کالا ندارد!";
                        if (string.IsNullOrEmpty(taxhistory.fListOfErrorCode))
                            taxhistory.fListOfErrorCode = "-";
                        if (string.IsNullOrEmpty(taxhistory.fListOfErrorCaption))
                            taxhistory.fListOfErrorCaption = "-";

                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);
                        await npsContext.SaveChangesAsync();
                    }
                }

                return null;
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            bodyDtoSysDTOs.Add(CurrentInvoice.FirstOrDefault());
            var data = JsonSerializer.Serialize(CurrentInvoice, options);

            try
            {

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJson).
                          LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                      + "TaxJson:InvoiceID:" + invoice.InvoiceID + ":inno:" + CurrentInvoice.FirstOrDefault().header.inno + "::\r\n" + data);

            }
            catch
            {
            }


            data = data.Replace("\r\n", "").Replace("\n", "").Replace("\\", "").Replace("  ", "").Replace("   ", "");
            if (data.StartsWith("[") && data.EndsWith("]"))
            {
                data = data.Substring(1, data.Length - 2);


            }
            try
            {

                Interlocked.Increment(ref countofSend); // Safely increment count
                var message = @"فاکتور به شماره: " + CurrentInvoice.First().header.inno +
                                    " و مبلغ :" + CurrentInvoice.First().header.tprdis +
                                    " با موفقیت بارگذاری گردید. "
                                    + "\r\n تعداد : " + countofSend +
                                    " از " + countofAll +
                                    " :: زمان صرف شده: " + timespan.ToString() + " ثانیه";

                await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);


            }




            return data;
        }

        // Send a batch of invoices based on their serialized data
        public async Task SendBatchAsync(List<Guid> batch, Dictionary<Guid, string> invoiceDataMap)
        {
            await _SemaphoreSlimForLimitSendForGrouping.WaitAsync();
            try
            {
                // Gather the invoice data for the batch
                //var invoiceDataList = new List<string>();
                var invoiceBatchData = new Dictionary<Guid, string>();
                foreach (var id in batch)
                {
                    if (invoiceDataMap.TryGetValue(id, out var serializedInvoice))
                    {
                        //invoiceDataList.Add(serializedInvoice);
                        invoiceBatchData[id] = serializedInvoice;
                    }
                }

                if (invoiceBatchData.Any())
                {
                    // Send the batch of invoices to the API
                    // await SendInvoiceBatchAsync(invoiceDataList);
                    await SendInvoiceBatchAsync(invoiceBatchData);
                }
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

            }
            finally
            {
                _SemaphoreSlimForLimitSendForGrouping.Release();
            }
        }

        // Method to send a batch of serialized invoice data
        public async Task<ResultModel> SendInvoiceBatchAsync(Dictionary<Guid, string> invoiceBatchData)
        {
            ResultModel result = new ResultModel();
            try
            {
                // Combine the serialized invoice data into a JSON array string
                var combinedData = "[" + string.Join(",", invoiceBatchData.Values) + "]";

                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 5, 0);
                    client.DefaultRequestHeaders.Add("X-orgid", BMain.SrvInfo.MEMORY_ID);
                    var token = BMain.SrvInfo.FILE_KEY_PRIVATE.Trim().Replace("\r\n", "");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BMain.SrvInfo.API_URL + "invoices/async")
                    {
                        Content = new StringContent(combinedData, null, "application/json")
                    };

                    HttpResponseMessage res = await client.SendAsync(request);
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " StatusCode ::" + res.StatusCode);
                    if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                    {


                        var resultofSend = await res.Content.ReadAsStringAsync();
                        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(resultofSend);
                        var countofAllsend = retData.Count();
                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                                           LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                       + "countofAllsend::" + countofAllsend);
                        var countofSendTiss = 0;
                        // Iterate over the results and associate them with the correct invoiceId
                        foreach (var r in retData)
                        {
                            countofSendTiss++;
                            var referenceNumber = r.referenceNumber;
                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
            + " Send ::" + referenceNumber);
                            var invoiceId = invoiceBatchData.FirstOrDefault(x => x.Value.Contains(r.inno)).Key;
                            var invoicejson = invoiceBatchData.FirstOrDefault(x => x.Value.Contains(r.inno)).Value;
                            var currentdto = bodyDtoSysDTOs.FirstOrDefault(a => a.header.inno.ToString() == r.inno);

                            try
                            {
                                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                                          LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                      + "TaxJson:InvoiceID:" + invoiceId + ":taxId:"
                                      + r.taxId + ":referenceNumber:" + r.referenceNumber + "::\r\n" + invoicejson);
                            }
                            catch
                            {
                            }
                            if (currentdto != null)
                            {
                                invoiceId = Guid.Parse(currentdto.header.InvoiceId);
                                _context.CreateSaveInvoiceResult(invoiceId.ToString(), referenceNumber, r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber);
                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceId.ToString(), r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber, false);

                                using (var context = new NPSGeneralContext())
                                {
                                    var updatetax = await context.tTaxHistoryStateParallels.FirstOrDefaultAsync
                                        (a => a.fInvoiceID == invoiceId);
                                    if (updatetax == null)
                                    {
                                        updatetax = new tTaxHistoryStateParallel()
                                        {
                                            fInvoiceID = invoiceId,
                                            fLastSendDate = DateTime.Now,
                                            fListOfErrorCaption = @"در حال بررسی",
                                            fListOfErrorCode = @"در حال بررسی",
                                            fSendState = 2,
                                            ReferenceNumber = referenceNumber,
                                            fTaxID = r.taxId
                                        };
                                    }

                                    updatetax.ReferenceNumber = referenceNumber;
                                    updatetax.fLastSendDate = DateTime.Now;
                                    updatetax.fSendState = 2;
                                    updatetax.fTaxID = r.taxId;
                                    updatetax.fListOfErrorCaption = @"در حال بررسی";
                                    updatetax.fListOfErrorCode = @"در حال بررسی";

                                    context.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, updatetax);

                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.
                                        ArghamCompany_TaxGoverment).
                                        LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                    + "updatetax::" + currentdto.header.inno + "::" + countofSendTiss + "::" + invoiceId + ":taxId:"
                                    + r.taxId + ":referenceNumber:" + r.referenceNumber + "::");

                                    await context.SaveChangesAsync();

                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.
                                       ArghamCompany_TaxGoverment).
                                       LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                   + "updatetaxSave::" + currentdto.header.inno + "::" + countofSendTiss + "::" + invoiceId + ":taxId:"
                                   + r.taxId + ":referenceNumber:" + r.referenceNumber + "::");

                                    var maxLastOrder = await context.
                   tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToString().ToLower() == invoiceId.ToString().ToLower()).
                   OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                                    tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                                    {
                                        fInvoiceID = Guid.Parse(invoiceId.ToString()),
                                        fLastSendDate = DateTime.Now,
                                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                        fReferencedID = referenceNumber,
                                        fStatus = 2,
                                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1)),
                                        fTaxID = r.taxId
                                    };

                                    context.tSendInvoiceTaxTissLogs.AddOrUpdate(tSendInvoice);

                                    await context.SaveChangesAsync();
                                    _ = BMain.ScheduleOperationAsync(invoiceId.ToString());

                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.
                                      ArghamCompany_TaxGoverment).
                                      LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                  + "_ = BMain.ScheduleOperationAsync(invoiceId.ToString());::" + countofSendTiss + "::" + invoiceId + ":taxId:"
                                  + r.taxId + ":referenceNumber:" + r.referenceNumber + "::");

                                }

                                var message = @"فاکتور به شماره: " + r.inno +
                                   " با موفقیت ثبت گردید. "
                                   + "\r\n تعداد : " + countofSendTiss +
                                   " از " + countofAllsend;

                                await BMain._hubContext.Clients.All.SendAsync("SendToTaxState", message);
                            }

                            result.Success = true;
                            result.Message = "با موفقیت ثبت شد.";
                        }
                    }
                    else
                    {
                        var resultOfSend = await res.Content.ReadAsStringAsync();
                        result.Success = false;
                        result.Message = "ارتباط با سامانه مودیان برقرار نشد";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Data = ex;
                result.Message = ex.Message;
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
              LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
          + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

            }

            return result;
        }

    }
    public class InquiryByRef
    {
        public object status { get; set; }
        public string invoiceId { get; set; }
        public bool hasError { get; set; }
        public string referenceNumber { get; set; }
        public string errorCode { get; set; }
        public string errorDetail { get; set; }
        public string uid { get; set; }
        public string fiscalId { get; set; }
        public string taxId { get; set; }
        public string inno { get; set; }

        public string message { get; set; }

        public result data { set; get; }
    }
    public class errorDetail
    {
        public string code { set; get; }
        public string message { set; get; }
        public object details { get; set; }
    }
    public class result
    {
        public bool success { set; get; }
        public string taxId { set; get; }
        public string packetType { set; get; }
        public string confirmationReferenceId { set; get; }
        public List<errorDetail> error { get; set; }
        public List<errorDetail> warning { get; set; }

    }

}