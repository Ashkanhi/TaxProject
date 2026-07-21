using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
//using Org.BouncyCastle.Ocsp;
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
using TaxWebAppCore8.Models.Rima;
using TaxCollectData.Library.Algorithms;
using TaxCollectData.Library.Providers;
using Org.BouncyCastle.Crypto;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Threading;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;


namespace TaxServiceAndApp.Models
{
    public class RimaServiceCalculateMethod
    {
        private static bool NeedInit = true;
        private readonly IContext _context;
        private static ServerInformationModel ServerInformationModel;
        private static TokenModel TokenModel;
        #region singleTone
        private static RimaServiceCalculateMethod _tisServise = null;
        public static RimaServiceCalculateMethod getInstance(IContext context)
        {
            if (_tisServise == null)
                _tisServise = new RimaServiceCalculateMethod(context);
            return _tisServise;
        }

        public RimaServiceCalculateMethod(IContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var result = await Login();
                if (result != null &&
                    result.status == ApplicantAuthenticationStatus.Success)
                    return true;
                else return false;
                //ServicePointManager.Expect100Continue = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11
                //    | SecurityProtocolType.Tls12;
                //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                //using (var client = new HttpClient())
                //{
                //    client.Timeout = new TimeSpan(0, 5, 0);
                //    client.BaseAddress = new Uri(BMain.SrvInfo.API_URL);

                //    client.DefaultRequestHeaders.Add("X-OrgId", BMain.SrvInfo.MEMORY_ID);  //New 

                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.
                //        AuthenticationHeaderValue("Bearer", BMain.SrvInfo.FILE_KEY_PRIVATE.Trim());

                //    List<string> refs = new List<string>();
                //    refs.Add("test");
                //    var jrefs = JsonSerializer.Serialize(refs);
                //    var content = new StringContent(jrefs.ToString(), Encoding.UTF8, "application/json");
                //    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/invoices/status",
                //        content);
                //    if (res != null)
                //    {
                //        var data = await res.Content.ReadAsStringAsync();

                //        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(data);

                //        return true;
                //    }
                //}
                //return false;

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                   LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
               + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return false;
            }
        }
        List<Invoice> bodyDtoSysDTOs = new List<Invoice>();

        public async Task<InvoiceInputModel> GetInvoice(DTOInvoiceLimitParameter invoice, bool isCancel = false)
        {
            try
            {

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
   LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ "GetInvoice:InvoiceID" + invoice.InvoiceID);
            Invoice inv = _context.GetLastInvoiceForSend(
               (short)(isCancel == true ? 3 : 1), invoice, 2);

                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    var data = JsonSerializer.Serialize(inv, options);

                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJson).
                              LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                          + "Invoice inv :InvoiceID:" + invoice.InvoiceID + "::\r\n" + data);

                }
                catch
                {
                }
                if (inv == null)
                {
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.JobSchedule).
                            LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                        + "inv == null:InvoiceID:" + invoice.InvoiceID);

                    return null;
                }
            bodyDtoSysDTOs.Add(inv);
            //invoiceId = inv.RimaHeader.InvoiceId.ToString();
            DateTime calcTissDocumentDate = inv.Header.InvoiceDate ?? DateTime.Now; // Replace with your actual date
            DateTime tissDocumentDate = calcTissDocumentDate.AddMinutes(-30);

            // Convert to Unix timestamp in milliseconds
            long unixTimestampMilliseconds = (long)(tissDocumentDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);

            //Convert to string
            //Indatim = 1711196764000
            //1711150200000

            var date = inv.Header.InvoiceDate ?? DateTime.Now;
            var taxId = BTaxMain.CreateUniqeTaxID
                (long.Parse(inv.Header.Inno.Replace("-", "")),
                date);

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
               .LogInfoMessage($"::TaxService::" +
                   $"CreateUniqeTaxID::taxId::" + taxId + "invoiceID::" + invoice.InvoiceID);
            TaxIdProvider taxIdProvider = new TaxIdProvider(new VerhoeffAlgorithm());
            string memoryId = BMain.SrvInfo.MEMORY_ID;
            DateTimeOffset now = new DateTimeOffset(inv.Header.InvoiceDate ?? DateTime.Now);
            taxId = taxIdProvider.GenerateTaxId(
            memoryId,
            long.Parse(inv.Header.Inno.Replace("-", "")),
            now.DateTime);


            long indatim = now.ToUnixTimeMilliseconds();

            invoice.TaxGuid = taxId;

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
       .LogInfoMessage($"::TaxService::" +
           $"CreateUniqeTaxID::NewtaxId::" + taxId + "::indatim" + indatim + "::invoiceID::" + invoice.InvoiceID);

            var header = new InvoiceHeaderInputModel()
            {
                taxid = taxId,
                indatim = indatim,// unixTimestampMilliseconds,//inv.RimaHeader.Indatim,
                indati2m = indatim,// unixTimestampMilliseconds,//inv.RimaHeader.Indatim,
                inty = inv.Header.Inty,
                inno = string.Format("{0:X}", inv.Header.Inno.Replace("-", "")).PadLeft(10, '0'),
                irtaxid =/* string.IsNullOrEmpty(inv.Header.Irtaxid) ? inv.Header.irtaxid :*/ inv.Header.Irtaxid,
                inp = inv.Header.Inp ?? 0,
                ins = inv.Header.Ins ?? 0,
                tins = BMain.SrvInfo.TINS,
                tob = inv.Header.Tob,
                bid = inv.Header.Bid,
                tinb = inv.Header.Tinb,
                sbc = inv.Header.Sbc,
                bpc = inv.Header.Bbc,
                bbc = inv.Header.Bbc,
                ft = null,// inv.Header.Ft ?? 0,
                bpn = inv.Header.Bpn,
                scln = inv.Header.Scln,
                scc = inv.Header.Scc,
                crn = inv.Header.Crn,
                billid = inv.Header.Billid,
                tprdis = (long?)inv.Header.Tprdis ?? 0,
                tdis = (long?)inv.Header.Tdis ?? 0,
                tadis = (long?)inv.Header.Tadis ?? 0,
                tvam = (long?)inv.Header.Tvam ?? 0,
                todam = inv.Header.Todam == null ? 0 : (long?)inv.Header.Todam,
                tbill = (long?)inv.Header.Tbill ?? 0,
                setm = inv.Header.Setm ?? 0,
                cap = (long?)inv.Header.Cap ?? 0,
                insp = (long?)inv.Header.Insp ?? 0,
                tvop = (long?)inv.Header.Tvop ?? 0,
                tax17 = (long?)inv.Header.Tax17 ?? 0,
                cdcn = inv.Header.Cdcn,
                cdcd = null,// inv.Header.Cdcd??0,
                tonw = null,// inv.Header.Tonw??0,
                torv = null,//  (long?)inv.Header.Torv,
                tocv = null,//  inv.Header.Tocv,

            };
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
         .LogInfoMessage($"::TaxService::" +
             $"header = new InvoiceHeaderInputModel()-Succes::NewtaxId::" + taxId + "::indatim" + indatim + "::invoiceID::" + invoice.InvoiceID);

                InvoicePaymentInputModel paymentDto = new InvoicePaymentInputModel()
            {
                pmt = 3,
            };

            var invoiceDtoWrapper = new InvoiceInputModel()
            {
                header = header,
                payments = new List<InvoicePaymentInputModel> { paymentDto },
                body = inv.Rows
            };

            return invoiceDtoWrapper;

            }
            catch (Exception ex)
            {
                var logger = ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment);
                logger.LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}:: Error: {ex.Message}", ex);
                return null;

            }
        }
        private object GetGroupingInvoice(string invoiceId, bool isCancel)
        {
            List<TissInvoiceBodyDtoSysDTO> list = new List<TissInvoiceBodyDtoSysDTO>();
            try
            {

                var invs = _context.GetGroupOfInvoiceForSend((short)(isCancel == true ? 3 : 1),
                    invoiceId, true);

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

        public async Task<bool> SignUp()
        {
            SignUpInputModel registerUserRequest = new SignUpInputModel()
            {

                password = BMain.SrvInfo.PASSWORD,
                username = BMain.SrvInfo.CertificateName,
                fiscalId = BMain.SrvInfo.MEMORY_ID,
                economicId = BMain.SrvInfo.TINS
            };

            var jregisterUserRequest = JsonSerializer.Serialize(registerUserRequest);


            ResultModel result = new ResultModel();
            try
            {
                // Combine the serialized invoice data into a JSON array string


                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 5, 0);
                    client.DefaultRequestHeaders.Add("X-orgid", BMain.SrvInfo.MEMORY_ID);

                    var content = new StringContent(jregisterUserRequest, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/Auth/SignUp",
                        content);
                    //HttpResponseMessage res = await client.SendAsync(request);
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " StatusCode ::" + res.StatusCode);
                    if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        var resultOfSend = await res.Content.ReadAsStringAsync();
                        result.Success = false;
                        result.Message = "ارتباط با سامانه مودیان برقرار نشد";

                        return false;
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

            return false;

        }
        public async Task<ApplicantAccessTokenApplicantAuthenticationStatusResult> Login()
        {
            SignInInputModel loginUserRequest = new SignInInputModel()
            {
                password = BMain.SrvInfo.PASSWORD,
                username = BMain.SrvInfo.CertificateName
            };

            var jloginUserRequest = JsonSerializer.Serialize(loginUserRequest);


            ResultModel result = new ResultModel();
            try
            {
                // Combine the serialized invoice data into a JSON array string


                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 5, 0);
                    client.DefaultRequestHeaders.Add("X-orgid", BMain.SrvInfo.MEMORY_ID);
                    //var token = BMain.SrvInfo.FILE_KEY_PRIVATE.Trim().Replace("\r\n", "");


                    /// DefaultRequestHeaders.Authorization = Token Login from Login
                    /// /auth/login
                    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var content = new StringContent(jloginUserRequest, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/Auth/SignIn",
                        content);
                    //HttpResponseMessage res = await client.SendAsync(request);
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " StatusCode ::" + res.StatusCode);
                    if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                    {
                        var resultofSend = await res.Content.ReadAsStringAsync();
                        var retData = JsonConvert.DeserializeObject<ApplicantAccessTokenApplicantAuthenticationStatusResult>(resultofSend);
                        if (retData != null && retData.isSuccess)
                        {
                            BMain.SrvInfo.FILE_KEY_PRIVATE = retData.data.token;
                            return retData;
                        }
                        else
                        {
                            //Send Error
                        }
                    }
                    else if (res.StatusCode == HttpStatusCode.NotFound
                        || res.StatusCode == HttpStatusCode.Unauthorized
                        || res.StatusCode == HttpStatusCode.BadRequest
                        )
                    {
                        var signup = await SignUp();
                        if (signup)
                        {
                            res = await client.PostAsync(BMain.SrvInfo.API_URL + "/Auth/SignIn",
                        content);
                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                         LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                     + " StatusCode ::" + res.StatusCode);
                            if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                            {
                                var resultofSend = await res.Content.ReadAsStringAsync();
                                var retData = JsonConvert.DeserializeObject<ApplicantAccessTokenApplicantAuthenticationStatusResult>(resultofSend);
                                if (retData != null)
                                {
                                    BMain.SrvInfo.FILE_KEY_PRIVATE = retData.data.token;
                                    return retData;
                                }
                            }

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
            //Send Error
            return null;
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
            //            ResultModel result = new ResultModel();
            //            try
            //            {
            //                var currcttax = _context.GetTaxHistory(Guid.Parse(invoiceID));
            //                ServicePointManager.Expect100Continue = true;
            //                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11
            //                    | SecurityProtocolType.Tls12;
            //                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            //                using (var client = new HttpClient())
            //                {
            //                    client.Timeout = new TimeSpan(0, 5, 0);
            //                    client.BaseAddress = new Uri(BMain.SrvInfo.API_URL);

            //                    client.DefaultRequestHeaders.Add("X-OrgId", BMain.SrvInfo.MEMORY_ID);  //New 

            //                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.
            //                        AuthenticationHeaderValue("Bearer", BMain.SrvInfo.FILE_KEY_PRIVATE.Trim());
            //                    List<string> refs = new List<string>();
            //                    refs.Add(currcttax.ReferenceNumber);
            //                    var jrefs = JsonSerializer.Serialize(refs);


            //                    var content = new StringContent(jrefs.ToString(), Encoding.UTF8, "application/json");
            //                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "inquiry/byReferenceId",
            //                        content);
            //                    if (res != null)
            //                    {

            //                        result.Success = true;
            //                        // result.Data = packetres;

            //                        var data = await res.Content.ReadAsStringAsync();

            //                        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(data);
            //                        var inquiry = retData.FirstOrDefault();
            //                        result.Status = retData.FirstOrDefault().status.ToString();
            //                        //_context.SaveInvoiceInquiryLog(currcttax.ReferenceNumber,
            //                        //           retData.FirstOrDefault().status.ToString(),
            //                        //           retData.FirstOrDefault()?.taxId,
            //                        //            retData.FirstOrDefault()?.data.ToString(),
            //                        //            "a");

            //                        // await SaveInvoiceInquiryLog(invoiceID, retData.da.FirstOrDefault());

            //                        //switch (inquiry.status.ToString().ToLower())
            //                        //{
            //                        //    case "success":
            //                        //        await HandleSuccessAsync(model, inquiry);
            //                        //        break;

            //                        //    case "failed":
            //                        //        await HandleFailedAsync(model, inquiry);
            //                        //        break;

            //                        //    case "pending":
            //                        //        await HandlePendingAsync(model, inquiry);
            //                        //        break;

            //                        //    case "gatewaytimeout":
            //                        //        result.Message = $"Gateway Timeout: {inquiry.data.error.FirstOrDefault()?.message}";
            //                        //        result.Success = false;
            //                        //        break;

            //                        //    default:
            //                        //        using (var npsContext = new NPSGeneralContext())
            //                        //        {
            //                        //            var errror = new errorDetail()
            //                        //            {
            //                        //                code = "0000",
            //                        //                details = "فاکتور پیدا نشد!",
            //                        //                message = "فاکتور پیدا نشد!"
            //                        //            };
            //                        //            await _context.LogErrorsAsync(npsContext, new List<errorDetail> { errror });
            //                        //            var taxhistory = await
            //                        //            npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower());
            //                        //            if (taxhistory == null)
            //                        //            {
            //                        //                taxhistory = new tTaxHistoryStateParallel();
            //                        //                taxhistory.ReferenceNumber = model.fTaxHistoryReferenceNumber;
            //                        //                taxhistory.fInvoiceID = model.InvoiceId;
            //                        //            }
            //                        //            {
            //                        //                taxhistory.fListOfErrorCaption =
            //                        //                    string.Join('-', "فاکتور پیدا نشد!");
            //                        //                taxhistory.fSendState = 3;

            //                        //                taxhistory.fListOfErrorCode =
            //                        //                    string.Join('-', "فاکتور پیدا نشد!");
            //                        //                npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

            //                        //            }
            //                        //            await npsContext.SaveChangesAsync();
            //                        //        }
            //                        //        result.Message = $"Unknown Status: {inquiry.status}";
            //                        //        model.InqMessage = "فاکتور پیدا نشد!";

            //                        //        _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "Unknown Error", 0, "", model.fTaxHistoryReferenceNumber);
            //                        //        result.Success = false;
            //                        //        break;
            //                        switch (retData.FirstOrDefault().status.ToString().ToLower())
            //                        {
            //                            case "success":

            //                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID, "SUCCESS", 1,
            //                                    retData.FirstOrDefault().uid.ToString(), currcttax.ReferenceNumber);

            //                                using (var npsContext = new NPSGeneralContext())
            //                                {
            //                                    var maxLastOrder = await npsContext.
            //                                        tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToString().ToLower()
            //                                        == invoiceID.ToLower()).
            //                                        OrderByDescending(a => a.fID).FirstOrDefaultAsync();

            //                                    tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
            //                                    {
            //                                        fInvoiceID = Guid.Parse(invoiceID),
            //                                        fLastSendDate = DateTime.Now,
            //                                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
            //                                        fReferencedID = currcttax.ReferenceNumber,
            //                                        fStatus = 4,
            //                                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
            //                                    };
            //                                    var taxhistory = await
            //                                      npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
            //                                      .ToString().ToLower() == invoiceID.ToLower());
            //                                    if (taxhistory == null)
            //                                    {
            //                                        taxhistory = new tTaxHistoryStateParallel();
            //                                        taxhistory.ReferenceNumber = tSendInvoice.fReferencedID;
            //                                        taxhistory.fInvoiceID = Guid.Parse(invoiceID);
            //                                    }
            //                                    if (taxhistory != null)
            //                                    {
            //                                        taxhistory.fListOfErrorCaption = "ثبت موفق";
            //                                        taxhistory.fListOfErrorCode = "ثبت موفق";
            //                                        taxhistory.fSendState = 4;
            //                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

            //                                    }
            //                                    await npsContext.SaveChangesAsync();
            //                                    npsContext.tSendInvoiceTaxTissLogs.Add(tSendInvoice);
            //                                    await npsContext.SaveChangesAsync();
            //                                }


            //                                //void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel);
            //                                result.Message = "فاکتور با موفقیت ثبت شده است";
            //                                {
            //                                    var message = @"صورتحساب فاقد خطا بود و با موفقیت در کارپوشه ثبت شد.";
            //                                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);
            //                                }
            //                                break;
            //                            case "failed":

            //                                //_context.TaxGovermentInvoiceFinalInsertUpdate(refId, "فاکتور با موفقیت ثبت شده است", 1, retData.FirstOrDefault().taxId.ToString(), refId);
            //                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID,
            //                                    "FAILED", 0,
            //                                    retData.FirstOrDefault().uid.ToString(), currcttax.ReferenceNumber);

            //                                using (var npsContext = new NPSGeneralContext())
            //                                {
            //                                    try
            //                                    {

            //                                        await _context.LogErrorsAsync(npsContext, inquiry.data.error);
            //                                        var maxLastOrder = await
            //                                                npsContext.tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToString().ToLower() == invoiceID.ToLower()).
            //                                                OrderByDescending(a => a.fID).FirstOrDefaultAsync();

            //                                        tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
            //                                        {
            //                                            fInvoiceID = Guid.Parse(invoiceID),
            //                                            fLastSendDate = DateTime.Now,
            //                                            fLogUID = ClsTools.ClsTools.CreateUniqueID(),
            //                                            fReferencedID = currcttax.ReferenceNumber,
            //                                            fStatus = 3,
            //                                            fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
            //                                        };
            //                                        npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, tSendInvoice);
            //                                        await npsContext.SaveChangesAsync();
            //                                        foreach (var error in inquiry.data.error)
            //                                        {
            //                                            try
            //                                            {
            //                                                var existingError = await npsContext.tTisTaxErrorTypes
            //                                                    .FirstOrDefaultAsync(e => e.fType == 1 && e.fCode.ToLower().Trim() == error.code.ToLower().Trim());

            //                                                if (existingError == null)
            //                                                {
            //                                                    var newError = new tTisTaxErrorType
            //                                                    {
            //                                                        fCaption = error.message,
            //                                                        fDescription = error.message,
            //                                                        fCode = error.code,
            //                                                        fType = 1,
            //                                                    };
            //                                                    npsContext.tTisTaxErrorTypes.AddOrUpdate(e => e.fID, newError);
            //                                                    await npsContext.SaveChangesAsync();
            //                                                    existingError = await npsContext.tTisTaxErrorTypes
            //                                                    .FirstOrDefaultAsync(e => e.fType == 1 && e.fCode.ToLower().Trim() == error.code.ToLower().Trim());
            //                                                }

            //                                                var tErrorslog = new tErrorsOfLog()
            //                                                {
            //                                                    fCaption = error.message,
            //                                                    fErrorCode = error.code,
            //                                                    fErrorType = existingError.fID,
            //                                                    fLogUID = ClsTools.ClsTools.CreateUniqueID(),
            //                                                    fSendLogUID = tSendInvoice.fLogUID,
            //                                                    fInvoiceId = invoiceID,

            //                                                };
            //                                                npsContext.tErrorsOfLogs.AddOrUpdate(e => e.fLogUID, tErrorslog);
            //                                                await npsContext.SaveChangesAsync();
            //                                            }
            //                                            catch (Exception ex)
            //                                            {
            //                                                Console.WriteLine($"Error logging error code {error.code}: {ex.Message}");
            //                                            }
            //                                        }
            //                                        var taxhistory = await
            //                                            npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
            //                                            .ToString().ToLower() == invoiceID.ToLower());
            //                                        if (taxhistory == null)
            //                                        {
            //                                            taxhistory = new tTaxHistoryStateParallel();
            //                                            taxhistory.ReferenceNumber = tSendInvoice.fReferencedID;
            //                                            taxhistory.fInvoiceID = Guid.Parse(invoiceID);
            //                                        }
            //                                        {
            //                                            taxhistory.fListOfErrorCaption =
            //                                                string.Join('-', inquiry.data.error.Select(a => a.message).ToArray());
            //                                            taxhistory.fSendState = 3;

            //                                            taxhistory.fListOfErrorCode =
            //                                                string.Join('-', inquiry.data.error.Select(a => a.code).ToArray());
            //                                            npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

            //                                        }
            //                                        await npsContext.SaveChangesAsync();
            //                                    }
            //                                    catch (Exception er)
            //                                    {

            //                                        Console.WriteLine(er.Message);
            //                                    }
            //                                }

            //                                //_logger.LogError("TissService GetInvoiceStatusByRefrenceAsync:" + retData.FirstOrDefault().data.error.FirstOrDefault().message);
            //                                result.Message = "فاکتور به دلیل  :" +
            //                                    retData.FirstOrDefault().data.error.FirstOrDefault().message
            //                                    + " ثبت نشده است ";
            //                                {
            //                                    var message = "فاکتور به دلیل  :" +
            //                                    retData.FirstOrDefault().data.error.FirstOrDefault().message
            //                                    + " ثبت نشده است ";
            //                                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
            //                                }
            //                                break;
            //                            case "pending":
            //                                result.Message = "فاکتور هنوز تعیین تکلیف نشده است";

            //                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID, "PENDING",
            //                                    2, retData.FirstOrDefault().uid.ToString(), currcttax.ReferenceNumber);



            //                                using (var npsContext = new NPSGeneralContext())
            //                                {
            //                                    var maxLastOrder = await npsContext.tSendInvoiceTaxTissLogs.
            //                                        Where(a => a.fInvoiceID.ToString().ToLower() == invoiceID.ToLower()).OrderByDescending(a => a.fID)
            //                                        .FirstOrDefaultAsync();

            //                                    tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
            //                                    {
            //                                        fInvoiceID = Guid.Parse(invoiceID),
            //                                        fLastSendDate = DateTime.Now,
            //                                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
            //                                        fReferencedID = currcttax.ReferenceNumber,
            //                                        fStatus = 2,
            //                                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
            //                                    };
            //                                    npsContext.tSendInvoiceTaxTissLogs.Add(tSendInvoice);
            //                                    var taxhistory = await
            //                                       npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
            //                                       .ToString().ToLower() == invoiceID.ToLower());
            //                                    if (taxhistory != null)
            //                                    {
            //                                        taxhistory.fListOfErrorCaption = "درحال بررسی";
            //                                        taxhistory.fListOfErrorCode = "درحال بررسی";
            //                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

            //                                    }
            //                                    if (taxhistory == null)
            //                                    {
            //                                        taxhistory = new tTaxHistoryStateParallel();
            //                                        taxhistory.ReferenceNumber = tSendInvoice.fReferencedID;
            //                                        taxhistory.fInvoiceID = Guid.Parse(invoiceID);
            //                                    }
            //                                    {
            //                                        taxhistory.fSendState = 2;

            //                                        taxhistory.fListOfErrorCaption = "درحال بررسی";
            //                                        taxhistory.fListOfErrorCode = "درحال بررسی";
            //                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

            //                                    }
            //                                    await npsContext.SaveChangesAsync();
            //                                }

            //                                {
            //                                    var message = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
            //                                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
            //                                }
            //                                break;
            //                            case "gatewaygimeout":
            //                                result.Message = "gatewaygimeout" + retData.FirstOrDefault().data.error.FirstOrDefault().message;
            //                                result.Success = false;

            //                                break;


            //                            default:
            //                                using (var npsContext = new NPSGeneralContext())
            //                                {
            //                                    var errror = new errorDetail()
            //                                    {
            //                                        code = "0000",
            //                                        details = "فاکتور پیدا نشد!",
            //                                        message = "فاکتور پیدا نشد!"
            //                                    };
            //                                    await _context.LogErrorsAsync(npsContext, new List<errorDetail> { errror });
            //                                    var taxhistory = await
            //                                    npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
            //                                    .ToString().ToLower() == invoiceID.ToLower());
            //                                    if (taxhistory == null)
            //                                    {
            //                                        taxhistory = new tTaxHistoryStateParallel();
            //                                        taxhistory.ReferenceNumber = currcttax.ReferenceNumber;
            //                                        taxhistory.fInvoiceID = Guid.Parse(invoiceID);
            //                                    }
            //                                    {
            //                                        taxhistory.fListOfErrorCaption =
            //                                            string.Join('-', "فاکتور پیدا نشد!");
            //                                        taxhistory.fSendState = 3;

            //                                        taxhistory.fListOfErrorCode =
            //                                            string.Join('-', "فاکتور پیدا نشد!");
            //                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

            //                                    }
            //                                    await npsContext.SaveChangesAsync();
            //                                }
            //                                result.Message = $"Unknown Status: {inquiry.status}";


            //                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID, "Unknown Error", 0, "", currcttax.ReferenceNumber);
            //                                result.Success = false;
            //                                break;
            //                        }



            //                    }
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            //.LogError($"::TissService::" +
            //$"PostAsync:model " + ex.Message + ex.InnerException?.Message, ex);
            //                result.Success = false;
            //                result.Data = ex;
            //                result.Message = ex.Message;
            //                //_logger.LogError("TisService SendInvoiceAsync:" + ex.ToString());
            //            }
            //            return result;
        }
        public async Task<List<GeneralInvoiceHeaderViewModel>> 
            GetAllInvoiceByInqFromServer(SearchParameters search)
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

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                var login = await Login();
                if (login != null)

                    using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) })
                    {
                        client.BaseAddress = new Uri(BMain.SrvInfo.API_URL);
                        client.DefaultRequestHeaders.Add("X-OrgId", BMain.SrvInfo.MEMORY_ID); // New header
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BMain.SrvInfo.FILE_KEY_PRIVATE.Trim());

                        var references = models.Where(a => a.fSendState == 2).Select(m => m.fTaxHistoryReferenceNumber).ToList();
                        InqueryInvoiceBulk inqueryInvoice = new InqueryInvoiceBulk();
                        inqueryInvoice.ids = references;
                        var jsonRefs = JsonSerializer.Serialize(inqueryInvoice);

                        var content = new StringContent(jsonRefs, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync($"{BMain.SrvInfo.API_URL}/Invoice/Inquiry", content);
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
                            var inquiries = JsonConvert.DeserializeObject<InvoiceInquiryViewModelIListResult>(responseData);
                            int countofAll = inquiries.data.Count;
                            int countofSend = 0;
                            using (var npsContext = new NPSGeneralContext())

                                foreach (var model in models)
                                {
                                    string status = "";

                                    var inquiry = inquiries.data.FirstOrDefault(a => a.id == Guid.Parse(model.fTaxHistoryReferenceNumber));
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

                                        model.RimaResultModelInquiry = inquiry;
                                        result.Status = inquiry.status.ToString();
                                        var GuidID = Guid.Parse(model.InvoiceId);

                                        await SaveInvoiceInquiryLog(model.fTaxHistoryReferenceNumber, inquiry);

                                        switch (inquiry.status.ToString())
                                        {
                                            case "SUCCESS":
                                            case "success":
                                                status = "ثبت موفق";
                                                await HandleSuccessAsync(model, inquiry, npsContext);
                                                break;
                                            case "RIMA_VALIDATION_FAILED":
                                            case "ORG_FAILED":
                                            case "failed":
                                                status = "خطا";
                                                await HandleFailedAsync(model, inquiry, npsContext);
                                                break;
                                            case "RIMA_UNKNOWN_FAILURE":
                                            case "RIMA_IN_QUEUE":
                                            case "RIMA_PROCESSING_FROM_QUEUE":
                                            case "RIMA_FAILED_ON_PERSIST":
                                            case "RIMA_UNKNOWN_QUEUE_FAILURE":
                                            case "RIMA_VALIDATION_FAILED_BY_SYSTEM":
                                            case "RIMA_CALCULATION_FAILED":
                                            case "RIMA_SUBMISSION_FAILED":
                                            case "RIMA_UNKNOWN_SUBMISSION_FAILURE":
                                            case "RIMA_SUBMISSION_PROCESSING":
                                            case "SENT_TO_ORG":
                                            case "ORG_IN_PROGRESS":
                                            case "pending":
                                                status = "در حال بررسی";
                                                await HandlePendingAsync(model, inquiry);
                                                break;

                                            case "gatewaytimeout":
                                                result.Message = $"Gateway Timeout: {inquiry.data.error.FirstOrDefault()?.message}";
                                                result.Success = false;
                                                break;

                                            case "ORG_NOT_FOUND":
                                            default:
                                                status = "فاکتور پیدا نشد";
                                                {
                                                    var errror = new errorDetail()
                                                    {
                                                        code = "0000",
                                                        details = "فاکتور پیدا نشد!",
                                                        message = "فاکتور پیدا نشد!"
                                                    };
                                                    await _context.LogErrorsAsync(npsContext, new List<errorDetail> { errror });

                                                    var taxhistory = await
                                                    npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
                                                    == GuidID);
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
                else
                {
                    //SendError
                    var message = "ارتباط با سامانه مودیان برقرار نشد";

                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);

                    result.Success = false;
                    result.Message = "ارتباط با سامانه مودیان برقرار نشد";
                    return null;
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


        private async Task SaveInvoiceInquiryLog(string referenceNumber, InvoiceInquiryViewModel inquiry)
        {
            _context.SaveInvoiceInquiryLog(
                referenceNumber,
                inquiry.status.ToString(),
                inquiry?.id.ToString(),
                inquiry?.data?.ToString(),
                "a"
            );
        }
        private async Task HandleSuccessAsync(GeneralInvoiceHeaderViewModel model, InvoiceInquiryViewModel inquiry, NPSGeneralContext npsContext)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)

            .LogInfoMessage($"::TissService::" +
                       $"HandleSuccessAsync:Out: " + model.InvoiceId.ToString() + "::");
            var GuidID = Guid.Parse(model.InvoiceId);

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "SUCCESS",
                1, inquiry.id.ToString(), model.fTaxHistoryReferenceNumber);


            {
                var maxLastOrder = await npsContext.
                                       tSendInvoiceTaxTissLogs.Where
                                       (a => a.fInvoiceID
                                       == GuidID).
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

                var taxhistory = await
                  npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
                  == GuidID);
                if (taxhistory == null)
                {
                    taxhistory = new tTaxHistoryStateParallel();
                    taxhistory.ReferenceNumber = maxLastOrder.fReferencedID;
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

        private async Task HandleFailedAsync(GeneralInvoiceHeaderViewModel model, InvoiceInquiryViewModel inquiry, NPSGeneralContext npsContext)
        {
            var errorMessages = string.Join("::", inquiry.data.error.Select(e => e.message));
            var warningMessages = string.Join("::", inquiry.data.warning.Select(w => w.message));
            var GuidID = Guid.Parse(model.InvoiceId);

            {
                try
                {

                    await _context.LogErrorsAsync(npsContext, inquiry.data.error.Select(a => new errorDetail()
                    {
                        code = a.code,
                        details = a.detail,
                        message = a.message
                    }).ToList());

                    var maxLastOrder = await npsContext.
                                     tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID == GuidID).
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
                    foreach (var error in inquiry.data.error)
                    {
                        try
                        {
                            var existingError = await npsContext.tTisTaxErrorTypes
                                .FirstOrDefaultAsync(e => e.fType == 1 && e.fCaption.ToLower().Trim() == error.message.ToLower().Trim());

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
                                .FirstOrDefaultAsync(e => e.fType == 1 && e.fCaption.ToLower().Trim() == error.message.ToLower().Trim());
                            }

                            var tErrorslog = new tErrorsOfLog()
                            {
                                fCaption = error.message,
                                fErrorCode = error.code ?? error.message,
                                fErrorType = existingError.fID,
                                fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                fSendLogUID = maxLastOrder.fLogUID,
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
                        npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
                         == GuidID);
                    if (taxhistory == null)
                    {
                        taxhistory = new tTaxHistoryStateParallel();
                        taxhistory.ReferenceNumber = maxLastOrder.fReferencedID;
                        taxhistory.fInvoiceID = Guid.Parse(model.InvoiceId);
                    }
                    {
                        taxhistory.fListOfErrorCaption =
                            string.Join('-', inquiry.data.error.Select(a => a.message).ToArray());
                        taxhistory.fSendState = 3;

                        taxhistory.fListOfErrorCode =
                            string.Join('-', inquiry.data.error.Select(a => a.code).ToArray()) ?? "-";
                        if (string.IsNullOrEmpty(taxhistory.fListOfErrorCode))
                            taxhistory.fListOfErrorCode = "-";
                        if (string.IsNullOrEmpty(taxhistory.fListOfErrorCaption))
                            taxhistory.fListOfErrorCaption = "-";

                        if (string.IsNullOrEmpty(taxhistory.fTaxID))
                            taxhistory.fTaxID = model.fTaxID;
                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                    }
                    await npsContext.SaveChangesAsync();

                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
    .LogWarningMessage($"::TissService::" +
               $"HandleFailed::inquiry.data.error.FirstOrDefault()::" + inquiry.data.error.FirstOrDefault()?.message);
                }
                catch (Exception ex)
                {
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
        .LogError($"::TissService::" +
                   $"HandleFailed" + ex.Message + ex.InnerException?.Message, ex);

                    Console.WriteLine(ex.Message);
                }
            }

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "FAILED", 0, inquiry.id.ToString(), model.fTaxHistoryReferenceNumber);

            model.InqMessage = $"فاکتور به دلیل این خطاها ثبت نگردید : {errorMessages}";
        }

        private async Task HandlePendingAsync(GeneralInvoiceHeaderViewModel model, InvoiceInquiryViewModel inquiry)
        {
            var GuidID = Guid.Parse(model.InvoiceId);

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "PENDING", 2, inquiry.id.ToString(), model.fTaxHistoryReferenceNumber);
            using (var npsContext = new NPSGeneralContext())
            {
                var maxLastOrder = await npsContext.tSendInvoiceTaxTissLogs.
                    Where(a => a.fInvoiceID == GuidID).OrderByDescending(a => a.fID)
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
                   npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
                    == GuidID);
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
                                    retData.FirstOrDefault().uid.ToString(), InvoiceID);
                                //void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel);
                                result.Message = "فاکتور با موفقیت ثبت شده است";
                                break;
                            case "failed":
                                _context.SaveInvoiceInquiryLog(InvoiceID, "failed", retData.FirstOrDefault().uid,
                                    retData.FirstOrDefault().data.ToString(), "a");

                                //_context.TaxGovermentInvoiceFinalInsertUpdate(refId, "فاکتور با موفقیت ثبت شده است", 1, retData.FirstOrDefault().taxId.ToString(), refId);
                                _context.TaxGovermentInvoiceFinalInsertUpdate(InvoiceID,
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message, 0,
                                    retData.FirstOrDefault().uid.ToString(), InvoiceID);

                                //_logger.LogError("TissService GetInvoiceStatusByRefrenceAsync:" + retData.FirstOrDefault().data.error.FirstOrDefault().message);
                                result.Message = "فاکتور به دلیل  :" +
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message
                                    + " ثبت نشده است";

                                break;
                            case "pending":
                                result.Message = "فاکتور هنوز تعیین تکلیف نشده است";

                                _context.TaxGovermentInvoiceFinalInsertUpdate(InvoiceID, "فاکتور هنوز تعیین تکلیف نشده است",
                                    2, retData.FirstOrDefault().uid.ToString(), InvoiceID);

                                _context.SaveInvoiceInquiryLog(InvoiceID, "Pendding", retData.FirstOrDefault().uid,
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

        public static SemaphoreSlim _SemaphoreSlimForLimitSendForGrouping
            = new SemaphoreSlim(10);
        public static int countofSendForGrouping;
        public static int countofAllForGrouping;
        public static int countofSentForGrouping; // شمارش تجمعی فاکتورهای «ارسال‌شده» برای نمایش پیشرفت «X از کل»
        public const int MaxBatchSizeInBytes = 2 * 1024 * 1024;
        public const int MaxCurrentCount = 10;
        public async Task<ResultModel> SendGroupInvoicesForGroupingAsync(List<DTOInvoiceLimitParameter> ids)
        {
            var result = new ResultModel { Count = 0, Success = false };
//            INFO - BaranDB :ExecCommandToObject: Exec dbo.Usp_TaxCartable_DetailInfo N'3feb3182-9d49-4aa3-88f9-01926f9316a4',N'2',N'10'
//2026 - 04 - 13 13:04:18,441[8] ERROR - BaranDB:< SendGroupInvoicesForGroupingAsync > d__34::Error: Object reference not set to an instance of an object.
//2026 - 04 - 13 13:04:18,442[8] ERROR - Object reference not set to an instance of an object.
//System.NullReferenceException: Object reference not set to an instance of an object.
//   at TaxServiceAndApp.Models.RimaServiceCalculateMethod.GetInvoice(DTOInvoiceLimitParameter invoice, Boolean isCancel) in D:\4Ashkan\TaxGovermentService\TaxWebAppCore8\BI\RimaServiceCalculateMethod.cs:line 139
//   at TaxServiceAndApp.Models.RimaServiceCalculateMethod.GetSerializedInvoiceAsync(DTOInvoiceLimitParameter invoice, Boolean isCancel) in D:\4Ashkan\TaxGovermentService\TaxWebAppCore8\BI\RimaServiceCalculateMethod.cs:line 1766
//   at TaxServiceAndApp.Models.RimaServiceCalculateMethod.SendGroupInvoicesForGroupingAsync(List`1 ids) in D:\4Ashkan\TaxGovermentService\TaxWebAppCore8\BI\RimaServiceCalculateMethod.cs:line 1651

            // --- متغیرهای محلی برای جلوگیری از state sharing در concurrent calls ---
            var invoiceMetadata = new Dictionary<Guid, DTOInvoiceLimitParameter>();
            foreach (var item in ids)
            {
                if (Guid.TryParse(item.InvoiceID, out var gId))
                {
                    // اگر تکراری وجود دارد، نادیده می‌گیریم تا خطا ندهد
                    if (!invoiceMetadata.ContainsKey(gId))
                    {
                        invoiceMetadata.Add(gId, item);
                    }
                }
            }
            var countOfAll = ids.Count;
            var countOfAllForGrouping = ids.Count;
            var countOfSendForGrouping = 0;
            var countOfSend = 0;
            // شمارنده تجمعی ارسال برای نمایش پیشرفت «X از کل» (پیام SignalR سراسری/Clients.All است)
            countofSentForGrouping = 0;
            countofAllForGrouping = ids.Count;
            var idsSnapshot = ids;
            try
            {
                var logger = ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment);
                logger.LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}:: SendGroupInvoicesForGroupingAsync :: Start");

                // مرحله ۱: سریالایز کردن فاکتورهای معتبر
                var invoiceDataMap = new Dictionary<Guid, string>();

                using (var npsContext = new NPSGeneralContext())
                {
                    foreach (var item in ids)
                    {
                        if (!Guid.TryParse(item.InvoiceID, out var invoiceGuid))
                        {
                            logger.LogInfoMessage($"Invalid InvoiceID: {item.InvoiceID}");
                            countOfAll--;
                            continue;
                        }

                        var sendLogs = await npsContext.tSendInvoiceTaxTissLogs
                            .Where(a => a.fInvoiceID == invoiceGuid)
                            .ToListAsync();

                        bool isStatusValid = !sendLogs.Any(a => a.fStatus == 2 || a.fStatus == 4) &&
                                             (item.Status == 0 || item.Status == 1 || item.Status == 3);

                        if (isStatusValid)
                        {
                            var serializedInvoice = await GetSerializedInvoiceAsync(item, false);
                            if (serializedInvoice != null)
                            {
                                invoiceDataMap[item.InvoiceGuidID] = serializedInvoice;
                                if (invoiceMetadata.TryGetValue(item.InvoiceGuidID, out var metaItem))
                                {
                                    // اینجا به زور مقدار جدید را داخل دیکشنری می‌نویسیم
                                    metaItem.TaxGuid = item.TaxGuid;

                                    // لاگ برای اطمینان بیشتر (اختیاری)
                                    // logger.LogInfoMessage($"Explicitly set TaxGuid for {item.InvoiceGuidID}: {item.TaxGuid}");
                                }
                            }
                            else
                            {
                                countOfAll--;
                            }
                        }
                        else
                        {
                            countOfAll--;
                        }
                    }
                }

                if (invoiceDataMap.Count == 0)
                {
                    logger.LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}:: آیتم‌های بارگذاری نمی‌شود");
                    result.Message = "آیتم‌ها بارگذاری نمی‌شود";
                    return result;
                }

                // مرحله ۲: ایجاد batch‌ها بر اساس حجم بایت و تعداد
                var batches = new List<List<Guid>>();
                var currentBatch = new List<Guid>();
                int currentBatchSize = 0;
                int currentSendCount = 0;

                foreach (var id in invoiceDataMap.Keys)
                {
                    var invoiceSize = Encoding.UTF8.GetByteCount(invoiceDataMap[id]);

                    if ((currentBatchSize + invoiceSize > MaxBatchSizeInBytes) || currentSendCount >= MaxCurrentCount)
                    {
                        if (currentBatch.Count > 0)
                        {
                            batches.Add(currentBatch);
                            currentBatch = new List<Guid>();
                            currentBatchSize = 0;
                            currentSendCount = 0;
                        }
                    }

                    currentBatch.Add(id);
                    currentBatchSize += invoiceSize;
                    currentSendCount++;
                }

                if (currentBatch.Count > 0)
                {
                    batches.Add(currentBatch);
                }

                // مرحله ۳: اجرای همزمان batch‌ها با محدودیت 20 تسک
                using var semaphore = new SemaphoreSlim(20, 20); // initialCount: 20, maxCount: 20
                var tasks = new List<Task>();

                foreach (var batch in batches)
                {
                    // Capture batch and map for closure safety
                    var capturedBatch = batch;
                    tasks.Add(SendBatchWithThrottlingAsync(capturedBatch, invoiceDataMap, invoiceMetadata, semaphore));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                result.Success = true;
                result.Count = invoiceDataMap.Count;
            }
            catch (Exception ex)
            {
                var logger = ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment);
                logger.LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}:: Error: {ex.Message}", ex);
                result.Message = ex.Message;
                result.Data = ex;
                result.Success = false;
            }

            return result;
        }

        // --- متد کمکی برای مدیریت semaphore بدون Task.Run ---
        private async Task SendBatchWithThrottlingAsync(
            List<Guid> batch,
            Dictionary<Guid, string> invoiceDataMap,
            Dictionary<Guid, DTOInvoiceLimitParameter> invoiceMetadata, 
            SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await SendBatchAsync(batch, invoiceDataMap, invoiceMetadata).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<string> GetSerializedInvoiceAsync(DTOInvoiceLimitParameter invoice, bool isCancel)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ " ExecCommandToObject ::GetSerializedInvoiceAsync");
            var Date1 = DateTime.Now;
            var CurrentInvoice = await GetInvoice(invoice, isCancel);
            var timespan = (DateTime.Now - Date1).TotalSeconds;
            if (CurrentInvoice == null) return null;
            if (CurrentInvoice.body.Any(a => string.IsNullOrEmpty(a.sstid)))
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ " CurrentInvoice.body.Any(a => string.IsNullOrEmpty(a.sstid)) ::GetSerializedInvoiceAsync");
                var GuidID = Guid.Parse(invoice.InvoiceID);
                using (var npsContext = new NPSGeneralContext())
                {
                    var taxhistory = await
                       npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
                        == GuidID);

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

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ "!! CurrentInvoice.body.Any(a => string.IsNullOrEmpty(a.sstid)) NOT NULL ::GetSerializedInvoiceAsync");

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var data = JsonSerializer.Serialize(CurrentInvoice, options);
            try
            {

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJson).
                          LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                      + "TaxJson:InvoiceID:" + invoice.InvoiceID + ":inno:" + CurrentInvoice.header.inno + "::\r\n" + data);

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
                var message = @"فاکتور به شماره: " + CurrentInvoice.header.inno +
                                    " و مبلغ :" + CurrentInvoice.header.tprdis +
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
        public async Task SendBatchAsync(List<Guid> batch, Dictionary<Guid, string> invoiceDataMap,
            Dictionary<Guid, DTOInvoiceLimitParameter> invoiceMetadata)
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
                    await SendInvoiceBatchAsync(invoiceBatchData, invoiceMetadata);
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
        public async Task<ResultModel> SendInvoiceBatchAsync
            (Dictionary<Guid, string> invoiceBatchData,
            Dictionary<Guid, DTOInvoiceLimitParameter> invoiceMetadata)
        {
            ResultModel result = new ResultModel();
            try
            {
                // Combine the serialized invoice data into a JSON array string
                var combinedData = "[" + string.Join(",", invoiceBatchData.Values) + "]";
                var login = await Login();
                if (login != null)
                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 5, 0);
                        client.DefaultRequestHeaders.Add("X-orgid", BMain.SrvInfo.MEMORY_ID);
                        var token = BMain.SrvInfo.FILE_KEY_PRIVATE?.Trim().Replace("\r\n", "") ?? "";

                        /// DefaultRequestHeaders.Authorization = Token Login from Login
                        /// /auth/login
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var content = new StringContent(combinedData, Encoding.UTF8, "application/json");
                        var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/Invoice/Send",
                            content);
                        //HttpResponseMessage res = await client.SendAsync(request);
                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                     LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                 + " StatusCode ::" + res.StatusCode);
                        if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                        {
                            var resultofSend = await res.Content.ReadAsStringAsync();
                            InvoiceRegisterViewModelResultListResult retData = JsonConvert.DeserializeObject<InvoiceRegisterViewModelResultListResult>(resultofSend);
                            var countofSendTiss = 0;
                            // Iterate over the results and associate them with the correct invoiceId
                            var countofAllsend = retData.data.Count();
                            using (var context = new NPSGeneralContext())
                                foreach (var r in retData.data)
                                {
                                    try
                                    {
                                        countofSendTiss++;
                                        var referenceNumber = r.data.id;
                                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                            LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                        + " Send ::" + referenceNumber);
                                        var currentFactor = bodyDtoSysDTOs.FirstOrDefault
                                            (a => a.Header.Inno == r.data.inno);
                                        //var invoicejson = invoiceBatchData.FirstOrDefault(a => a.Key == currentFactor.Header.InvoiceId).Value;

                                        var invoiceId = invoiceBatchData.FirstOrDefault(x => x.Value.Contains(r.data.inno)).Key;
                                        var invoicejson = invoiceBatchData.FirstOrDefault(x => x.Value.Contains(r.data.inno)).Value;
                                        try
                                        {
                                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                                                      LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                                  + "TaxJson:InvoiceID:" + invoiceId + ":taxId:"
                                                  + r.data.id + ":referenceNumber:" + r.data.id + "::\r\n" + invoicejson);

                                        }
                                        catch
                                        {
                                        }

                                        if (invoiceId != Guid.Empty)
                                        {
                                            if (invoiceMetadata.TryGetValue(invoiceId, out var currentItemObject))
                                            {

                                                _context.CreateSaveInvoiceResult(invoiceId.ToString(), referenceNumber?.ToString(), "Sent", currentItemObject.TaxGuid, referenceNumber?.ToString());
                                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceId.ToString(), "Sent", currentItemObject.TaxGuid, referenceNumber?.ToString(), false);

                                                {
                                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                                                      LogInfoMessage($"tTaxHistoryStateParallels.FirstOrDefaultAsync-------- IN");
                                                    var updatetax = await context.tTaxHistoryStateParallels.FirstOrDefaultAsync
                                                        (a => a.fInvoiceID == invoiceId);

                                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                                                      LogInfoMessage($"tTaxHistoryStateParallels.FirstOrDefaultAsync-------- OUT");
                                                    if (updatetax == null)
                                                    {
                                                        updatetax = new tTaxHistoryStateParallel()
                                                        {
                                                            fInvoiceID = invoiceId,
                                                            fLastSendDate = DateTime.Now,
                                                            fListOfErrorCaption = "در حال بررسی",
                                                            fListOfErrorCode = "در حال بررسی",
                                                            fSendState = 2,
                                                            ReferenceNumber = referenceNumber?.ToString()
                                                        };
                                                    }
                                                    updatetax.ReferenceNumber = referenceNumber?.ToString();
                                                    updatetax.fLastSendDate = DateTime.Now;
                                                    updatetax.fSendState = 2;
                                                    updatetax.fTaxID = currentItemObject.TaxGuid;
                                                    updatetax.fListOfErrorCaption = "در حال بررسی";
                                                    updatetax.fListOfErrorCode = "در حال بررسی";
                                                    context.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, updatetax);
                                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                                              LogInfoMessage($"SaveChangesAsync-------- in");
                                                    await context.SaveChangesAsync();

                                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                    LogInfoMessage($"SaveChangesAsync-------- OUT");

                                                    var maxLastOrder = await context.
                                  tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID == invoiceId).
                                  OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                    LogInfoMessage($"maxLastOrder = await context.-------- OUT");
                                                    tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                                                    {
                                                        fInvoiceID = updatetax.fInvoiceID,
                                                        fLastSendDate = DateTime.Now,
                                                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                                        fReferencedID = referenceNumber?.ToString(),
                                                        fStatus = 2,
                                                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1)),
                                                        fTaxID = currentItemObject.TaxGuid
                                                    };
                                                    context.tSendInvoiceTaxTissLogs.AddOrUpdate(tSendInvoice);

                                                    await context.SaveChangesAsync();


                                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                LogInfoMessage($"maxLastOrder = await context---SaveChangesAsync - 1978+++.-------- OUT");
                                                    _ = BMain.ScheduleOperationAsync(invoiceId.ToString());
                                                }

                                                Interlocked.Increment(ref countofSentForGrouping); // شمارش تجمعی کل فاکتورهای ارسال‌شده
                                                var message = @"فاکتور به شماره: " + r.data.inno +
                                                   " با موفقیت ثبت گردید. "
                                                   + "\r\n تعداد ارسال‌شده : " + countofSentForGrouping +
                                                   " از " + countofAllForGrouping;


                                                await BMain._hubContext.Clients.All.SendAsync("SendToTaxState", message);
                                            }
                                        }

                                        result.Success = true;
                                        result.Message = "با موفقیت ثبت شد.";
                                    }
                                    catch (Exception ef)
                                    {
                                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + "  ::" + ef.Message + "::" + ef.InnerException?.Message, ef);
                                    }
                                }
                        }
                        else
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            };

                            var data = JsonSerializer.Serialize(content, options);
                            var resultofRima = JsonSerializer.Serialize(res, options);
                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.SaveResult).
                    LogWarningMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " StatusCode BadReq---->::" + data); 
                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.SaveResult).
                    LogWarningMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " StatusCode resultofRima---->::" + resultofRima); 
                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.SaveResult).
                    LogWarningMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " StatusCode BadReq----combinedData>::" + combinedData);
                            var resultOfSend = await res.Content.ReadAsStringAsync();
                            result.Success = false;
                            result.Message = "ارتباط با سامانه مودیان برقرار نشد";
                        }
                    }
                else
                {
                    //SendError
                    var message = "ارتباط با سامانه مودیان برقرار نشد";
                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
                    result.Success = false;
                    result.Message = "ارتباط با سامانه مودیان برقرار نشد";
                    return result;
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


    public class ResultSignUp
    {
        public bool isSuccess { get; set; }
        public object messages { get; set; }

    }
    public class SignUpInputModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string fiscalId { get; set; }
        public string economicId { get; set; }
    }
    //public class LoginUserResponse
    //{
    //    public string token { get; set; }
    //    public Int64 expiresIn { get; set; }

    //}
    public class SignInInputModel
    {
        public string username { get; set; }
        public string password { get; set; }

    }
    public class ApplicantAccessTokenApplicantAuthenticationStatusResult
    {
        public bool isSuccess { get; set; }
        public List<string>? messages { get; set; }
        public ApplicantAccessToken? data { get; set; }
        public ApplicantAuthenticationStatus status { get; set; }
    }
    public class ApplicantAccessToken
    {
        public string? token { get; set; }
        public string? expiresIn { get; set; }

    }

    public enum ApplicantAuthenticationStatus
    {
        Success = 200,
        BadRequest = 400,
        Forbidden = 403,
        InternalServerError = 500
    }

}
public class InvoiceRegisterViewModelResultListResult
{
    public bool isSuccess { get; set; }
    public List<string>? messages { get; set; }
    public List<InvoiceRegisterViewModelResult>? data { get; set; }
}

public class InvoiceRegisterViewModelResult
{
    public bool isSuccess { get; set; }
    public List<string>? messages { get; set; }
    public InvoiceRegisterViewModel? data { get; set; }
}

public class InvoiceRegisterViewModel
{
    public Guid? id { get; set; }
    public string? inno { get; set; }
}
public class InvoiceHeaderInputModel
{
    public string? taxid { get; set; }
    public long? indatim { get; set; }
    public long? indati2m { get; set; }
    public int? inty { get; set; }
    public string? inno { get; set; }
    public string? irtaxid { get; set; }
    public int? inp { get; set; }
    public int? ins { get; set; }
    public string? tins { get; set; }
    public int? tob { get; set; }
    public string? bid { get; set; }
    public string? tinb { get; set; }
    public string? sbc { get; set; }
    public string? bpc { get; set; }
    public string? bbc { get; set; }
    public int? ft { get; set; }
    public string? bpn { get; set; }
    public string? scln { get; set; }
    public string? scc { get; set; }
    public string? crn { get; set; }
    public string? billid { get; set; }
    public double? tprdis { get; set; }
    public double? tdis { get; set; }
    public double? tadis { get; set; }
    public double? tvam { get; set; }
    public double? todam { get; set; }
    public double? tbill { get; set; }
    public int? setm { get; set; }
    public double? cap { get; set; }
    public double? insp { get; set; }
    public double? tvop { get; set; }
    public double? tax17 { get; set; }
    public string? cdcn { get; set; }
    public int? cdcd { get; set; }
    public double? tonw { get; set; }
    public double? torv { get; set; }
    public double? tocv { get; set; }
    public string? tinc { get; set; }
}
public class InvoiceItemInputModel
{
    public string? sstid { get; set; }
    public string? sstt { get; set; }
    public string? mu { get; set; }
    public double? am { get; set; }
    public double? fee { get; set; }
    public double? cfee { get; set; }
    public string? cut { get; set; }
    public double? exr { get; set; }
    public double? prdis { get; set; }
    public double? dis { get; set; }
    public double? adis { get; set; }
    public double? vra { get; set; }
    public double? vam { get; set; }
    public string? odt { get; set; }
    public double? odr { get; set; }
    public double? odam { get; set; }
    public string? olt { get; set; }
    public double? olr { get; set; }
    public double? olam { get; set; }
    public double? consfee { get; set; }
    public double? spro { get; set; }
    public double? bros { get; set; }
    public double? tcpbs { get; set; }
    public double? cop { get; set; }
    public double? vop { get; set; }
    public string? bsrn { get; set; }
    public double? tsstam { get; set; }
    public double? nw { get; set; }
    public double? ssrv { get; set; }
    public double? sscv { get; set; }
    public double? cui { get; set; }
}
public class InvoicePaymentInputModel
{
    public string? iinn { get; set; }
    public string? acn { get; set; }
    public string? trmn { get; set; }
    public string? trn { get; set; }
    public string? pcn { get; set; }
    public string? pid { get; set; }
    public long? pdt { get; set; }
    public int? pmt { get; set; }
    public long? pv { get; set; }
}
public class InvoiceInputModel
{
    public InvoiceHeaderInputModel? header { get; set; }
    public List<BodyItemDto>? body { get; set; }
    public List<InvoicePaymentInputModel>? payments { get; set; }
}

public class InvoiceEntityIListResult
{
    public bool isSuccess { get; set; }
    public List<string> messages { get; set; } // Assuming messages are strings; adjust type as necessary
    //public List<invoiceEntity> data { get; set; } // List of invoiceEntity objects
}

public class InvoiceInquiryViewModelIListResult
{
    public bool isSuccess { get; set; }
    public List<string> messages { get; set; } = new List<string>();
    public List<InvoiceInquiryViewModel> data { get; set; } = new List<InvoiceInquiryViewModel>();
}

public class InvoiceInquiryViewModel
{
    public Guid id { get; set; }
    public string status { get; set; }
    public InvoiceInquiryData data { get; set; }
}

public class InvoiceInquiryData
{
    public List<InvoiceInquiryLog> error { get; set; } = new List<InvoiceInquiryLog>();
    public List<InvoiceInquiryLog> warning { get; set; } = new List<InvoiceInquiryLog>();
}

public class InvoiceInquiryLog
{
    public string code { get; set; }
    public string message { get; set; }
    public List<string> detail { get; set; } = new List<string>();
}

public class InqueryInvoiceBulk
{
    // Property to hold a list of nullable GUIDs
    public List<string> ids { get; set; }

    // Constructor
    public InqueryInvoiceBulk()
    {
        ids = new List<string>();
    }
}