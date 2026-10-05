using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TaxCollectData.Library.Dto;
using TaxCollectData.Library.Models;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using JsonSerializer = System.Text.Json.JsonSerializer;
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


        private TissServise(IContext context)
        {
            _context = context;
        }


        public async Task<List<TissInvoiceBodyDtoSysDTO>> GetInvoice(string invoiceId, bool isCancel = false)
        {

            Invoice inv = _context.GetLastInvoiceForSend((short)(isCancel == true ? 3 : 1), invoiceId, true);

            if (inv == null)
                return null;

            inv.TissHeader.tins = BMain.SrvInfo.MEMORY_ID;
            var date = inv.TissHeader.TaxIdDate;

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
        public async Task<ResultModel> GetInvoiceStatusByRefrenceAsync(string refId)
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
                        AuthenticationHeaderValue("Bearer", BMain.SrvInfo.FILE_KEY_PRIVATE.Trim());
                    List<string> refs = new List<string>();
                    refs.Add(refId);
                    var jrefs = JsonSerializer.Serialize(refs);


                    var content = new StringContent(jrefs.ToString(), Encoding.UTF8, "application/json");
                    var res = await client.PostAsync("https://portal.tisstsp.ir/api/inquiry/byReferenceId",
                        content);
                    if (res != null)
                    {


                        result.Success = true;
                        // result.Data = packetres;


                        var data =await res.Content.ReadAsStringAsync();

                        List<InquiryByRef> retData = JsonConvert.DeserializeObject<List<InquiryByRef>>(data);
                        result.Status = retData.FirstOrDefault().status.ToString();
                        switch (retData.FirstOrDefault().status.ToString().ToLower())
                        {
                            case "success":
                                //SaveInvoiceInquiryLog(string refId, string status, string uId, string data, string packetType)

                                _context.TaxGovermentInvoiceFinalInsertUpdate(refId, "فاکتور با موفقیت ثبت شده است", 1,
                                    retData.FirstOrDefault().taxId.ToString(), refId);
                                //void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel);
                                result.Message = "فاکتور با موفقیت ثبت شده است";
                                break;
                            case "failed":
                                _context.SaveInvoiceInquiryLog(refId, "failed", retData.FirstOrDefault().taxId,
                                    retData.FirstOrDefault().data.ToString(), "a");

                                //_context.TaxGovermentInvoiceFinalInsertUpdate(refId, "فاکتور با موفقیت ثبت شده است", 1, retData.FirstOrDefault().taxId.ToString(), refId);
                                _context.TaxGovermentInvoiceFinalInsertUpdate(refId,
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message, 0,
                                    retData.FirstOrDefault().taxId.ToString(), refId);

                                //_logger.LogError("TissService GetInvoiceStatusByRefrenceAsync:" + retData.FirstOrDefault().data.error.FirstOrDefault().message);
                                result.Message = "فاکتور به دلیل  :" +
                                    retData.FirstOrDefault().data.error.FirstOrDefault().message
                                    + " ثبت نشده است";

                                break;
                            case "pending":
                                result.Message = "فاکتور هنوز تعیین تکلیف نشده است";

                                _context.TaxGovermentInvoiceFinalInsertUpdate(refId, "فاکتور هنوز تعیین تکلیف نشده است",
                                    2, retData.FirstOrDefault().taxId.ToString(), refId);

                                _context.SaveInvoiceInquiryLog(refId, "Pendding", retData.FirstOrDefault().taxId,
                                    retData.FirstOrDefault().data.ToString(), "a");
                                break;
                            case "gatewaygimeout":
                                result.Message = "gatewaygimeout" + retData.FirstOrDefault().data.error.FirstOrDefault().message;
                                result.Success = false;
                                break;

                            default:
                                result.Message = "نامشخص:" + retData.FirstOrDefault().status.ToString();
                                _context.TaxGovermentInvoiceFinalInsertUpdate(refId, "خطای نامشخص روی داده است", 0, "", refId);
                                result.Success = false;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Data = ex;
                result.Message = ex.Message;
                //_logger.LogError("TisService SendInvoiceAsync:" + ex.ToString());
            }
            return result;
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
        //        NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
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

        //            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
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

        //        NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
        //      LogError($"TissService" +
        //      $"SendInvoiceAsync::" + ex.Message + "::" + ex?.InnerException?.Message, ex);

        //    }
        //    return result;
        //}
        public async Task<ResultModel> SendInvoiceAsync(string invoiceId, bool isCancel = false)
        {
            ResultModel result = new ResultModel();
            try
            {
                var CurrentInvoice = await GetInvoice(invoiceId, isCancel);

                if (CurrentInvoice == null)
                { result.Message = "No Invoice Found"; }
                else
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    var data = JsonSerializer.Serialize(CurrentInvoice, options);
                    data = data.Replace("\r\n", "").Replace("\n", "").Replace("\\", "").Replace("  ", "").Replace("   ", "");

                    NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogInfoMessage($"::TissService::" +
                        $"SendInvoiceAsync:Out: " + data.ToString());

                    NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($"::TissService::" +
                    $"SendInvoiceAsync_2 :IN" + invoiceId.ToString());

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 5, 0);
                        // client.BaseAddress = new Uri(taxSetting.Url);

                        client.DefaultRequestHeaders.Add("X-orgid", BMain.SrvInfo.MEMORY_ID);  //New 
                        var token = BMain.SrvInfo.FILE_KEY_PRIVATE;
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        //var content = new StringContent(invoice.ToString(), Encoding.UTF8, "application/json");
                        //var res = client.PostAsync("api/invoices/async", content).Result; 
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://portal.tisstsp.ir/api/invoices/async")
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

                                _context.CreateSaveInvoiceResult(invoiceId, referenceNumber, r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber);
                                _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceId, r.hasError ? r.data.error.ToString() : "Sent", r.taxId, referenceNumber, isCancel);

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

                            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
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

                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
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
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://portal.tisstsp.ir/api/invoices/async")
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
                    var res = await client.PostAsync("https://portal.tisstsp.ir/api/inquiry/byInvoiceId",
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
                search.StorID = BMain.SrvInfo.StoreId;
                list = (await _context.GetAllInvoice(search));
            }
            catch (Exception ex)
            {
            }
            return list;
        }

        public async Task<ResultModel> SendGroupInvoices(List<Guid> Ids)
        {
            ResultModel result = new ResultModel();
            result.Count = 0;
            try
            {
                foreach (var item in Ids)
                {
                    var res = await SendInvoiceAsync(item.ToString(), false);
                    if (res.Success)
                    {
                        result.Count = result.Count + 1;
                    }
                }
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Data = ex;
                result.Message = ex.Message;
                //_logger.LogError("TissService SendGroupInvoices:" + ex.ToString());
                result.Success = false;
            }
            return result;
        }

    }
    class InquiryByRef
    {
        public object status { get; set; }
        public string invoiceId { get; set; }
        public bool hasError { get; set; }
        public string referenceNumber { get; set; }
        public string uid { get; set; }
        public string taxId { get; set; }
        public string inno { get; set; }

        public string message { get; set; }

        public result data { set; get; }



    }
    class errorDetail
    {
        public string code { set; get; }
        public string message { set; get; }
        public object details { get; set; }
    }
    class result
    {
        public bool success { set; get; }
        public string taxId { set; get; }
        public string packetType { set; get; }
        public string confirmationReferenceId { set; get; }
        public List<errorDetail> error { get; set; }
        public List<errorDetail> warning { get; set; }

    }

}