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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;


namespace TaxServiceAndApp.Models
{
    public class RimaService
    {
        private static bool NeedInit = true;
        private readonly IContext _context;
        private static ServerInformationModel ServerInformationModel;
        private static TokenModel TokenModel;
        #region singleTone
        private static RimaService _tisServise = null;
        public static RimaService getInstance(IContext context)
        {
            if (_tisServise == null)
                _tisServise = new RimaService(context);
            return _tisServise;
        }


        public RimaService(IContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                await Login();
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
                    refs.Add("test");
                    var jrefs = JsonSerializer.Serialize(refs);
                    var content = new StringContent(jrefs.ToString(), Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/invoices/status",
                        content);
                    if (res != null)
                    {
                        var data = await res.Content.ReadAsStringAsync();

                        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(data);

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

        public async Task<InvoiceDtoWrapper> GetInvoice(DTOInvoiceLimitParameter invoice, bool isCancel = false)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
   LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ "GetInvoice:InvoiceID" + invoice.InvoiceID);
            Invoice inv = _context.GetLastInvoiceForSend(
               (short)(isCancel == true ? 3 : 1), invoice, 2);
            if (inv == null)
                return null;

            //invoiceId = inv.Header.InvoiceId.ToString();
            DateTime calcTissDocumentDate = inv.Header.InvoiceDate ?? DateTime.Now; // Replace with your actual date
            DateTime tissDocumentDate = calcTissDocumentDate.AddMinutes(-30);

            // Convert to Unix timestamp in milliseconds
            long unixTimestampMilliseconds = (long)(tissDocumentDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);

            // Convert to string
            //Indatim = 1711196764000
            //1711150200000

            var date = inv.Header.InvoiceDate ?? DateTime.Now;
            var taxId = BTaxMain.CreateUniqeTaxID(long.Parse(inv.Header.Inno.Replace("-", "")), date);
            invoice.TaxGuid = taxId;



            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
               .LogInfoMessage($"::TaxService::" +
                   $"CreateUniqeTaxID::taxId::" + taxId + "invoiceID::" + invoice.InvoiceID);
            TaxIdProvider taxIdProvider = new TaxIdProvider(new VerhoeffAlgorithm());
            string memoryId = BMain.SrvInfo.MEMORY_ID;
            Random random = new Random();

            DateTimeOffset now = new DateTimeOffset(inv.Header.InvoiceDate ?? DateTime.Now);

            taxId = taxIdProvider.GenerateTaxId(
            memoryId,
            long.Parse(inv.Header.Inno.Replace("-", "")),
            now.DateTime);

            string inno = long.Parse(inv.Header.Inno.Replace("-", ""))
            .ToString("X")
            .PadLeft(10, '0');
            long indatim = now.ToUnixTimeMilliseconds();
            invoice.TaxGuid = taxId;
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
       .LogInfoMessage($"::TaxService::" +
           $"CreateUniqeTaxID::NewtaxId::" + taxId + "::indatim::" + indatim + "::invoiceID::" + invoice.InvoiceID);

            var header = new HeaderDto()
            {
                taxid = taxId,
                indatim = indatim,// unixTimestampMilliseconds,//inv.Header.Indatim,
                indati2m = indatim,// unixTimestampMilliseconds,//inv.Header.Indatim,
                inty = inv.Header.Inty,
                inno = string.Format("{0:X}", inv.Header.Inno.Replace("-", "")).PadLeft(10, '0'),
                irtaxid = inv.Header.Irtaxid,
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

            PaymentItemDto paymentDto = new PaymentItemDto()
            {
                pmt = 3,

            };
            var invoiceDto = new InvoiceDTOForWrapper()
            {
                header = header,
                body = inv.Rows,
                payments = new List<PaymentItemDto>()
            };
            List<PaymentItemDto> payments = new List<PaymentItemDto>();
            payments.Add(paymentDto);
            var invoiceDtoWrapper = new InvoiceDtoWrapper()
            {
                invoice = invoiceDto,
                fiscalId = BMain.SrvInfo.MEMORY_ID,
                uid = invoice.InvoiceID
            };

            return invoiceDtoWrapper;

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

        public async Task<bool> SignUp()
        {
            RegisterUserRequest registerUserRequest = new RegisterUserRequest()
            {
                firstName = BMain.SrvInfo.CustomerName,
                lastName = BMain.SrvInfo.CustomerName,
                password = BMain.SrvInfo.PASSWORD,
                username = BMain.SrvInfo.CertificateName
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
                    var token = BMain.SrvInfo.FILE_KEY_PRIVATE.Trim().Replace("\r\n", "");


                    /// DefaultRequestHeaders.Authorization = Token Login from Login
                    /// /auth/login
                    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var content = new StringContent(jregisterUserRequest, Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/auth/signup",
                        content);
                    //HttpResponseMessage res = await client.SendAsync(request);
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " StatusCode ::" + res.StatusCode);
                    if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                    {


                        var resultofSend = await res.Content.ReadAsStringAsync();
                        var retData = JsonConvert.DeserializeObject<RimaUser>(resultofSend);
                        if (retData != null)
                            return retData.enabled;
                        else return false;
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

            return false;

        }
        public async Task<LoginUserResponse> Login()
        {
            LoginUserRequest loginUserRequest = new LoginUserRequest()
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
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/auth/login",
                        content);
                    //HttpResponseMessage res = await client.SendAsync(request);
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " StatusCode ::" + res.StatusCode);
                    if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                    {
                        var resultofSend = await res.Content.ReadAsStringAsync();
                        var retData = JsonConvert.DeserializeObject<LoginUserResponse>(resultofSend);
                        if (retData != null)
                        {
                            BMain.SrvInfo.FILE_KEY_PRIVATE = retData.token;
                            return retData;
                        }
                        else
                        {
                            //Send Error
                        }

                    }
                    else if (res.StatusCode == HttpStatusCode.NotFound
                        || res.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        var signup = await SignUp();
                        if (signup)
                        {
                            res = await client.PostAsync(BMain.SrvInfo.API_URL + "/auth/login",
                        content);
                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                         LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                     + " StatusCode ::" + res.StatusCode);
                            if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                            {
                                var resultofSend = await res.Content.ReadAsStringAsync();
                                var retData = JsonConvert.DeserializeObject<LoginUserResponse>(resultofSend);
                                if (retData != null)
                                {
                                    BMain.SrvInfo.FILE_KEY_PRIVATE = retData.token;
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
                
                var currcttax = _context.GetTaxHistory(Guid.Parse(invoiceID));
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
                    refs.Add(currcttax.ReferenceNumber);
                    var jrefs = JsonSerializer.Serialize(refs);


                    var content = new StringContent(jrefs.ToString(), Encoding.UTF8, "application/json");
                    var res = await client.PostAsync(BMain.SrvInfo.API_URL + "inquiry/byReferenceId",
                        content);
                    if (res != null)
                    {

                        result.Success = true;
                        // result.Data = packetres;

                        var data = await res.Content.ReadAsStringAsync();

                        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(data);
                        var inquiry = retData.FirstOrDefault();
                        result.Status = retData.FirstOrDefault().status.ToString();
                        //_context.SaveInvoiceInquiryLog(currcttax.ReferenceNumber,
                        //           retData.FirstOrDefault().status.ToString(),
                        //           retData.FirstOrDefault()?.taxId,
                        //            retData.FirstOrDefault()?.data.ToString(),
                        //            "a");

                        await SaveInvoiceInquiryLog(invoiceID, retData.FirstOrDefault());

                        //switch (inquiry.status.ToString().ToLower())
                        //{
                        //    case "success":
                        //        await HandleSuccessAsync(model, inquiry);
                        //        break;

                        //    case "failed":
                        //        await HandleFailedAsync(model, inquiry);
                        //        break;

                        //    case "pending":
                        //        await HandlePendingAsync(model, inquiry);
                        //        break;

                        //    case "gatewaytimeout":
                        //        result.Message = $"Gateway Timeout: {inquiry.data.error.FirstOrDefault()?.message}";
                        //        result.Success = false;
                        //        break;

                        //    default:
                        //        using (var npsContext = new NPSGeneralContext())
                        //        {
                        //            var errror = new errorDetail()
                        //            {
                        //                code = "0000",
                        //                details = "فاکتور پیدا نشد!",
                        //                message = "فاکتور پیدا نشد!"
                        //            };
                        //            await _context.LogErrorsAsync(npsContext, new List<errorDetail> { errror });
                        //            var taxhistory = await
                        //            npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower());
                        //            if (taxhistory == null)
                        //            {
                        //                taxhistory = new tTaxHistoryStateParallel();
                        //                taxhistory.ReferenceNumber = model.fTaxHistoryReferenceNumber;
                        //                taxhistory.fInvoiceID = model.InvoiceId;
                        //            }
                        //            {
                        //                taxhistory.fListOfErrorCaption =
                        //                    string.Join('-', "فاکتور پیدا نشد!");
                        //                taxhistory.fSendState = 3;

                        //                taxhistory.fListOfErrorCode =
                        //                    string.Join('-', "فاکتور پیدا نشد!");
                        //                npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                        //            }
                        //            await npsContext.SaveChangesAsync();
                        //        }
                        //        result.Message = $"Unknown Status: {inquiry.status}";
                        //        model.InqMessage = "فاکتور پیدا نشد!";

                        //        _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "Unknown Error", 0, "", model.fTaxHistoryReferenceNumber);
                        //        result.Success = false;
                        //        break;
                        var invoiceIDGuid = Guid.Parse(invoiceID);

                        switch (retData.FirstOrDefault().status.ToString().ToLower())
                        {
                            case "success":

                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID, "SUCCESS", 1,
                                    retData.FirstOrDefault().uid.ToString(), currcttax.ReferenceNumber);
                                using (var npsContext = new NPSGeneralContext())
                                {
                                    var GuidID = Guid.Parse(invoiceID);
                                    var maxLastOrder = await npsContext.
                                        tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID == GuidID).
                                        OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                                    tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                                    {
                                        fInvoiceID = Guid.Parse(invoiceID),
                                        fLastSendDate = DateTime.Now,
                                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                        fReferencedID = currcttax.ReferenceNumber,
                                        fStatus = 4,
                                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                                    };
                                    var taxhistory = await
                                      npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == invoiceIDGuid);
                                    if (taxhistory == null)
                                    {
                                        taxhistory = new tTaxHistoryStateParallel();
                                        taxhistory.ReferenceNumber = tSendInvoice.fReferencedID;
                                        taxhistory.fInvoiceID = Guid.Parse(invoiceID);
                                    }
                                    if (taxhistory != null)
                                    {
                                        taxhistory.fListOfErrorCaption = "ثبت موفق";
                                        taxhistory.fListOfErrorCode = "ثبت موفق";
                                        taxhistory.fSendState = 4;
                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                                    }
                                    await npsContext.SaveChangesAsync();
                                    npsContext.tSendInvoiceTaxTissLogs.Add(tSendInvoice);
                                    await npsContext.SaveChangesAsync();
                                }


                                //void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel);
                                result.Message = "فاکتور با موفقیت ثبت شده است";
                                {
                                    var message = @"صورتحساب فاقد خطا بود و با موفقیت در کارپوشه ثبت شد.";
                                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);
                                }
                                break;
                            case "failed":

                                //_context.TaxGovermentInvoiceFinalInsertUpdate(refId, "فاکتور با موفقیت ثبت شده است", 1, retData.FirstOrDefault().taxId.ToString(), refId);
                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID,
                                    "FAILED", 0,
                                    retData.FirstOrDefault().uid.ToString(), currcttax.ReferenceNumber);

                                using (var npsContext = new NPSGeneralContext())
                                {
                                    try
                                    {

                                        await _context.LogErrorsAsync(npsContext, inquiry.data.error);
                                        var maxLastOrder = await
                                                npsContext.tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID== invoiceIDGuid).
                                                OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                                        tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                                        {
                                            fInvoiceID = Guid.Parse(invoiceID),
                                            fLastSendDate = DateTime.Now,
                                            fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                            fReferencedID = currcttax.ReferenceNumber,
                                            fStatus = 3,
                                            fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                                        };
                                        npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, tSendInvoice);
                                        await npsContext.SaveChangesAsync();
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
                                                    fSendLogUID = tSendInvoice.fLogUID,
                                                    fInvoiceId = invoiceID,

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
                                            npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == invoiceIDGuid);
                                        if (taxhistory == null)
                                        {
                                            taxhistory = new tTaxHistoryStateParallel();
                                            taxhistory.ReferenceNumber = tSendInvoice.fReferencedID;
                                            taxhistory.fInvoiceID = Guid.Parse(invoiceID);
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

                                //_logger.LogError("TissService GetInvoiceStatusByRefrenceAsync:" + retData.FirstOrDefault().data.error.FirstOrDefault().message);
                                result.Message = "فاکتور به دلیل  :" +
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message
                                    + " ثبت نشده است ";
                                {
                                    var message = "فاکتور به دلیل  :" +
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message
                                    + " ثبت نشده است ";
                                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
                                }
                                break;
                            case "pending":
                                result.Message = "فاکتور هنوز تعیین تکلیف نشده است";

                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID, "PENDING",
                                    2, retData.FirstOrDefault().uid.ToString(), currcttax.ReferenceNumber);



                                using (var npsContext = new NPSGeneralContext())
                                {
                                    var maxLastOrder = await npsContext.tSendInvoiceTaxTissLogs.
                                        Where(a => a.fInvoiceID== invoiceIDGuid).OrderByDescending(a => a.fID)
                                        .FirstOrDefaultAsync();

                                    tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                                    {
                                        fInvoiceID = Guid.Parse(invoiceID),
                                        fLastSendDate = DateTime.Now,
                                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                        fReferencedID = currcttax.ReferenceNumber,
                                        fStatus = 2,
                                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
                                    };
                                    npsContext.tSendInvoiceTaxTissLogs.Add(tSendInvoice);
                                    var taxhistory = await
                                       npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == invoiceIDGuid);
                                    if (taxhistory != null)
                                    {
                                        taxhistory.fListOfErrorCaption = "درحال بررسی";
                                        taxhistory.fListOfErrorCode = "درحال بررسی";
                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                                    }
                                    if (taxhistory == null)
                                    {
                                        taxhistory = new tTaxHistoryStateParallel();
                                        taxhistory.ReferenceNumber = tSendInvoice.fReferencedID;
                                        taxhistory.fInvoiceID = Guid.Parse(invoiceID);
                                    }
                                    {
                                        taxhistory.fSendState = 2;

                                        taxhistory.fListOfErrorCaption = "درحال بررسی";
                                        taxhistory.fListOfErrorCode = "درحال بررسی";
                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                                    }
                                    await npsContext.SaveChangesAsync();
                                }

                                {
                                    var message = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
                                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
                                }
                                break;
                            case "gatewaygimeout":
                                result.Message = "gatewaygimeout" + retData.FirstOrDefault().data.error.FirstOrDefault().message;
                                result.Success = false;

                                break;


                            default:
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
                                    npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == invoiceIDGuid);
                                    if (taxhistory == null)
                                    {
                                        taxhistory = new tTaxHistoryStateParallel();
                                        taxhistory.ReferenceNumber = currcttax.ReferenceNumber;
                                        taxhistory.fInvoiceID = Guid.Parse(invoiceID);
                                    }
                                    {
                                        taxhistory.fListOfErrorCaption =
                                            string.Join('-', "فاکتور پیدا نشد!");
                                        taxhistory.fSendState = 3;

                                        taxhistory.fListOfErrorCode =
                                            string.Join('-', "فاکتور پیدا نشد!");
                                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

                                    }
                                    await npsContext.SaveChangesAsync();
                                }
                                result.Message = $"Unknown Status: {inquiry.status}";


                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceID, "Unknown Error", 0, "", currcttax.ReferenceNumber);
                                result.Success = false;
                                break;
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
                        var jsonRefs = JsonSerializer.Serialize(references);

                        var content = new StringContent(jsonRefs, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync($"{BMain.SrvInfo.API_URL}/invoices/status", content);
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
                                                var GuidID = Guid.Parse(model.InvoiceId);
                                                var taxhistory = await
                                               
                                                npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == GuidID);
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

        private async Task SaveInvoiceInquiryLog(string referenceNumber, InquiryByRef inquiry)
        {
            _context.SaveInvoiceInquiryLog(
                referenceNumber,
                inquiry.status.ToString(),
                inquiry?.uid,
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

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "SUCCESS", 1, inquiry.uid.ToString(), model.fTaxHistoryReferenceNumber);
            var invoiceIDGuid = Guid.Parse(model.InvoiceId);

            using (var npsContext = new NPSGeneralContext())
            {
                var maxLastOrder = await npsContext.
                                       tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID== invoiceIDGuid).
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
                var GuidID = Guid.Parse(model.InvoiceId);
                var taxhistory = await
                  npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == GuidID);
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

        private async Task HandleFailedAsync(GeneralInvoiceHeaderViewModel model, InquiryByRef inquiry)
        {
            var errorMessages = string.Join("::", inquiry.data.error.Select(e => e.message));
            var warningMessages = string.Join("::", inquiry.data.warning.Select(w => w.message));
            var invoiceIDGuid = Guid.Parse(model.InvoiceId);

            using (var npsContext = new NPSGeneralContext())
            {
                try
                {

                    await _context.LogErrorsAsync(npsContext, inquiry.data.error);
                    var maxLastOrder = await npsContext.
                                     tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID== invoiceIDGuid).
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
                    var GuidID = Guid.Parse(model.InvoiceId);
                    var taxhistory = await
                        npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == GuidID);
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

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "FAILED", 0, inquiry.uid.ToString(), model.fTaxHistoryReferenceNumber);

            model.InqMessage = $"فاکتور به دلیل این خطاها ثبت نگردید : {errorMessages}";
        }

        private async Task HandlePendingAsync(GeneralInvoiceHeaderViewModel model, InquiryByRef inquiry)
        {

            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "PENDING", 2, inquiry.uid.ToString(), model.fTaxHistoryReferenceNumber);
            var invoiceIDGuid = Guid.Parse(model.InvoiceId);

            using (var npsContext = new NPSGeneralContext())
            {
                var maxLastOrder = await npsContext.tSendInvoiceTaxTissLogs.
                    Where(a => a.fInvoiceID == invoiceIDGuid).OrderByDescending(a => a.fID)
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
                   npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID== invoiceIDGuid);
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
        public const int MaxBatchSizeInBytes = 2 * 1024 * 1024;
        public const int MaxCurrentCount = 99;
        List<DTOInvoiceLimitParameter> _Ids = new List<DTOInvoiceLimitParameter>();
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
                    using (var npsContext = new NPSGeneralContext())
                    {
                        _Ids = Ids;
                        foreach (var item in Ids)
                        {
                            Guid itemIDGuid = Guid.Parse(item.InvoiceID);
                            var resultofSendTTLog = await npsContext.tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID
                            == itemIDGuid).ToListAsync();

                            if (
                                !resultofSendTTLog.Any(a => a.fStatus == 2 || a.fStatus == 4) &&
                                 (item.Status == 1 || item.Status == 3 || item.Status == 0)
                                 )
                            {
                                var serializedInvoice = await GetSerializedInvoiceAsync(item, false);
                                if (serializedInvoice != null)
                                {
                                    invoiceDataMap[item.InvoiceGuidID] = serializedInvoice;
                                }
                            }
                            else
                            {
                                var parallel = await npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID
                                == itemIDGuid);
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

        // Helper method to serialize the invoice and return the data as a string
        public async Task<string> GetSerializedInvoiceAsync(DTOInvoiceLimitParameter invoice, bool isCancel)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
+ " ExecCommandToObject ::GetSerializedInvoiceAsync");
            var Date1 = DateTime.Now;
            var CurrentInvoice = await GetInvoice(invoice, isCancel);
            var timespan = (DateTime.Now - Date1).TotalSeconds;
            if (CurrentInvoice == null) return null;
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
                      + "TaxJson:InvoiceID:" + invoice.InvoiceID + ":inno:" + CurrentInvoice.invoice.header.inno + "::\r\n" + data);

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
                var message = @"فاکتور به شماره: " + CurrentInvoice.invoice.header.inno +
                                    " و مبلغ :" + CurrentInvoice.invoice.header.tprdis +
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
                var login = await Login();
                if (login != null)
                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 5, 0);
                        client.DefaultRequestHeaders.Add("X-orgid", BMain.SrvInfo.MEMORY_ID);
                        var token = BMain.SrvInfo.FILE_KEY_PRIVATE.Trim().Replace("\r\n", "");


                        /// DefaultRequestHeaders.Authorization = Token Login from Login
                        /// /auth/login
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var content = new StringContent(combinedData, Encoding.UTF8, "application/json");
                        var res = await client.PostAsync(BMain.SrvInfo.API_URL + "/invoices/send",
                            content);
                        //HttpResponseMessage res = await client.SendAsync(request);
                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                     LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                 + " StatusCode ::" + res.StatusCode);
                        if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted || res.IsSuccessStatusCode)
                        {
                            var resultofSend = await res.Content.ReadAsStringAsync();
                            List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(resultofSend);
                            var countofAllsend = retData.Count();
                            var countofSendTiss = 0;
                            // Iterate over the results and associate them with the correct invoiceId
                            foreach (var r in retData)
                            {
                                countofSendTiss++;
                                var referenceNumber = r.referenceNumber;
                                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " Send ::" + referenceNumber);
                                var invoiceId = invoiceBatchData.FirstOrDefault(x => x.Value.Contains(r.uid)).Key;
                                var invoicejson = invoiceBatchData.FirstOrDefault(x => x.Value.Contains(r.uid)).Value;

                                try
                                {
                                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJsonResultOfTiss).
                                              LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                          + "TaxJson:InvoiceID:" + invoiceId + ":taxId:"
                                          + r.uid + ":referenceNumber:" + r.referenceNumber + "::\r\n" + invoicejson);

                                }
                                catch
                                {
                                }


                                if (invoiceId != Guid.Empty)
                                {
                                    var currentItemObject = _Ids.FirstOrDefault(a => a.InvoiceGuidID == invoiceId);

                                    _context.CreateSaveInvoiceResult(invoiceId.ToString(), referenceNumber, r.hasError ? r.data.error.ToString() : "Sent", currentItemObject.TaxGuid, referenceNumber);
                                    _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceId.ToString(), r.hasError ? r.data.error.ToString() : "Sent", currentItemObject.TaxGuid, referenceNumber, false);
                                    using (var context = new NPSGeneralContext())
                                    {
                                        var updatetax = await context.tTaxHistoryStateParallels.FirstOrDefaultAsync
                                            (a => a.fInvoiceID == invoiceId);
                                        if (updatetax == null)
                                        {
                                            updatetax = new tTaxHistoryStateParallel()
                                            {
                                                fInvoiceID = Guid.Parse(invoiceId.ToString()),
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
                                        updatetax.fTaxID = currentItemObject.TaxGuid;
                                        updatetax.fListOfErrorCaption = "در حال بررسی";
                                        updatetax.fListOfErrorCode = "در حال بررسی";
                                        context.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, updatetax);
                                        await context.SaveChangesAsync();
                                        var maxLastOrder = await context.
                      tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID == invoiceId).
                      OrderByDescending(a => a.fID).FirstOrDefaultAsync();

                                        tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
                                        {
                                            fInvoiceID = invoiceId,
                                            fLastSendDate = DateTime.Now,
                                            fLogUID = ClsTools.ClsTools.CreateUniqueID(),
                                            fReferencedID = referenceNumber,
                                            fStatus = 2,
                                            fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1)),
                                            fTaxID = currentItemObject.TaxGuid
                                        };
                                        context.tSendInvoiceTaxTissLogs.AddOrUpdate(tSendInvoice);

                                        await context.SaveChangesAsync();
                                        _ = BMain.ScheduleOperationAsync(invoiceId.ToString());
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

    public class RimaUser
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        public bool accountNonExpired { get; set; }
        public bool credentialsNonExpired { get; set; }
        public bool accountNonLocked { get; set; }
        public bool enabled { get; set; }
        public List<GrantedAuthority> authorities { get; set; }
    }
    public class GrantedAuthority
    {
        public string authority { get; set; }

    }
    public class RegisterUserRequest
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
    public class LoginUserResponse
    {
        public string token { get; set; }
        public Int64 expiresIn { get; set; }

    }
    public class LoginUserRequest
    {
        public string username { get; set; }
        public string password { get; set; }

    }

}