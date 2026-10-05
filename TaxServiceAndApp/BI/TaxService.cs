using MD.PersianDateTime;
using Newtonsoft.Json;
using NPSTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TaxCollectData.Library.Abstraction.Clients;
using TaxCollectData.Library.Abstraction.Cryptography;
using TaxCollectData.Library.Dto;
using TaxCollectData.Library.Factories;
using TaxCollectData.Library.Models;
using TaxCollectData.Library.Properties;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;

namespace TaxServiceAndApp.BI
{
    public class TaxService
    {
        private static TaxService _taxService = null;
        private Pkcs8SignatoryFactory pkcs8SignatoryFactory;
        private TaxProperties properties;
        private EncryptorFactory encryptorFactory;
        private static TaxApiFactory taxApiFactory;
        private ISignatory signatory;
        private static ITaxPublicApi publicApi;
        private IEncryptor encryptor;


        private static bool NeedInit = true;
        private readonly IContext _context;

        private static ServerInformationModel ServerInformationModel;
        private static TokenModel TokenModel;
        #region Singleton


        public static ITaxApi TaxApiObject;
        public static ILowLevelTaxApi LowLevelApi;
        public static TaxService getInstance(IContext context)
        {

            if (_taxService == null)
            {
                _taxService = new TaxService(context);

            }

            return _taxService;

        }
        private TaxService(IContext context)
        {
            _context = context;

        }
        #endregion
        #region Logs
        private void StartInitLog() => NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage($"Start Init Log Tax Service");
        private void EndInitLog() => NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage($"End Init Tax Service");

        private void TaxSettingCountLog(int count) => NPSTools.Logger.NPSLogger.
            GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage(($"Get TaxSetting Finished,Count {0}", count).ToString());

        private void GetServerInformationLog() => NPSTools.Logger.NPSLogger.
            GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage("Get Tax Server Information");

        private void MakeInvoiceDtoLog()
            => NPSTools.Logger.NPSLogger.
            GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage("Make Invoice Dto");

        private void GenerateTaxIdLog() => NPSTools.Logger.NPSLogger.
            GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage("Generate Tax Id Log");

        private void GetInvoiceStatusByRefrenceLog() => NPSTools.Logger.NPSLogger.
            GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage("Get Invoice Status By Refrence Log");


        private void SaveInvoiceJsonToLog(object obj)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                // string
                // Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dto.txt");

                PersianCalendar pc = new PersianCalendar();

                int year = pc.GetYear(DateTime.Now);
                int month = pc.GetMonth(DateTime.Now);
                int day = pc.GetDayOfMonth(DateTime.Now);
                int hour = pc.GetHour(DateTime.Now);
                int minute = pc.GetMinute(DateTime.Now);

                StringBuilder sb = new StringBuilder();

                sb.Append(year.ToString());
                sb.Append("_");
                sb.Append(month.ToString());
                sb.Append("_");
                sb.Append(day.ToString());
                sb.Append("_T_");
                sb.Append(hour.ToString());
                sb.Append("_");
                sb.Append(minute.ToString());


                try
                {

                    /*
                    // Create a subdirectory named "jasonlogs" within the base directory
                    string subFolderPath = Path.Combine(basePath, "jasonlogs");
                    // Ensure the subdirectory exists
                    Directory.CreateDirectory(subFolderPath);

                    // Now, combine the subfolder path with your file name
                    string filePath = Path.Combine(subFolderPath, $"JasonLog_{sb.ToString()}.txt");

                    // Write the JSON content to the file
                    File.WriteAllText(filePath, json.ToString());
                    */


                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string jasonlogsPath = Path.Combine(basePath, "jasonlogs");

                    // Ensure the subdirectory exists
                    Directory.CreateDirectory(jasonlogsPath);

                    File.WriteAllText($"{jasonlogsPath}\\JasonLog_{sb.ToString()}.txt", json.ToString());

                    //File.WriteAllText($"Logs\\JasonLog_{sb.ToString()}.txt", (string?)JsonConvert.DeserializeObject(json));
                    //File.WriteAllText($"{basePath}\\JasonLog_{sb.ToString()}.txt", (string?)JsonConvert.DeserializeObject(json));


                    //File.WriteAllText($"Logs\\DTOS.txt", DateTime.Now.ToString() + "AAA" + "_" + json);
                    // File.WriteAllText($"Logs\\DTOS.txt", DateTime.Now.ToString() + "AAA" + "_" + json);
                }
                catch (Exception ex)
                {
                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
                    string jasonlogsPath = Path.Combine(basePath, "jasonlogs");
                    // Ensure the subdirectory exists
                    Directory.CreateDirectory(jasonlogsPath);

                    string filePath = Path.Combine(basePath, "GetError.txt");

                    File.WriteAllLines(filePath, new string[] { ex.Message });
                }



                NPSTools.Logger.NPSLogger.
           GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage("DTOJson" + "::" + json);

                // _logger.LogDebug("DTOJson", json);
            }
            catch (Exception er)
            {
                NPSTools.Logger.NPSLogger.
          GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
               .LogError("DTOJson" + "::" + er.Message, er);
                // _logger.LogError("DTOJson");

            }

        }
        #endregion

        public async Task Init()
        {
            if (BMain.SrvInfo.TokenType == "CertificateFile")
                TaxInitWithFileCertificate();
            else
                TaxInitWithTokenCertificate();

            NeedInit = false;
            //}
        }
        private void TaxInitWithFileCertificate()
        {
            StartInitLog();

            pkcs8SignatoryFactory = new Pkcs8SignatoryFactory();
            properties = new TaxProperties(BMain.SrvInfo.MEMORY_ID);
            encryptorFactory = new EncryptorFactory();
            taxApiFactory = new TaxApiFactory(BMain.SrvInfo.API_URL, properties);

            signatory = pkcs8SignatoryFactory.Create(
             BMain.SrvInfo.FILE_KEY_PRIVATE,
             BMain.SrvInfo.FILE_CERTIFICATE);

            publicApi = taxApiFactory.CreatePublicApi(signatory);
            encryptor = encryptorFactory.Create(publicApi);
            TaxApiObject = taxApiFactory.CreateApi(signatory, encryptor);
            LowLevelApi = taxApiFactory.CreateLowLevelApi(signatory);

            EndInitLog();

        }
        private void TaxInitWithTokenCertificate()
        {
            StartInitLog();
            Pkcs11SignatoryFactory pkcs11SignatoryFactory = new Pkcs11SignatoryFactory();
            EncryptorFactory encryptorFactory = new EncryptorFactory();
            properties = new TaxProperties(BMain.SrvInfo.MEMORY_ID);
            taxApiFactory = new TaxApiFactory(BMain.SrvInfo.API_URL, properties);
            ISignatory signatory = pkcs11SignatoryFactory
             .Create(BMain.SrvInfo.CertificateName, BMain.SrvInfo.LIBRARY_PATH, BMain.SrvInfo.PASSWORD);

            ITaxPublicApi publicApi = taxApiFactory.CreatePublicApi(signatory);
            IEncryptor encryptor = encryptorFactory.Create(publicApi);
            TaxApiObject = taxApiFactory.CreateApi(signatory, encryptor);
            LowLevelApi = taxApiFactory.CreateLowLevelApi(signatory);

            EndInitLog();
        }
        private string GenerateTaxId(long InvoiceNumber, DateTime createDate, bool IsCancel = false)
        {

            var id = BTaxMain.CreateUniqeTaxID(InvoiceNumber, createDate);

            return id;
        }
        public async Task<InvoiceDto> GetInvoice(string invoiceId, bool isCancel)
        {

            Invoice inv = _context.GetLastInvoiceForSend(
                (short)(isCancel == true ? 3 : 1), invoiceId, false);
            if (inv == null)
                return null;

            invoiceId = inv.Header.InvoiceId.ToString();
            var date = inv.Header.TaxIdDate;
            var taxId = GenerateTaxId(long.Parse(inv.Header.Inno.Replace("-", "")), date, isCancel); //(isCancel == true ? inv.Header.Irtaxid : GenerateTaxId(long.Parse(inv.Header.Inno.Replace("-", "")), date, isCancel));
            var header = new HeaderDto()
            {
                taxid = taxId,
                indatim = inv.Header.Indatim,
                indati2m = inv.Header.Indatim,
                inty = inv.Header.Inty,
                inno = string.Format("{0:X}", inv.Header.Inno.Replace("-", "")).PadLeft(10, '0'),
                irtaxid = inv.Header.Irtaxid,
                inp = inv.Header.Inp,
                ins = inv.Header.Ins,
                tins = BMain.SrvInfo.TINS,
                tob = inv.Header.Tob,
                bid = inv.Header.Bid,
                tinb = inv.Header.Tinb,
                sbc = inv.Header.Sbc,
                bpc = inv.Header.Bbc,
                bbc = inv.Header.Bbc,
                ft = inv.Header.Ft,
                bpn = inv.Header.Bpn,
                scln = inv.Header.Scln,
                scc = inv.Header.Scc,
                crn = inv.Header.Crn,
                billid = inv.Header.Billid,
                tprdis = (long?)inv.Header.Tprdis,
                tdis = (long?)inv.Header.Tdis,
                tadis = (long?)inv.Header.Tadis,
                tvam = (long?)inv.Header.Tvam,
                todam = (long?)inv.Header.Todam,
                tbill = (long?)inv.Header.Tbill,
                setm = inv.Header.Setm,
                cap = (long?)inv.Header.Cap,
                insp = (long?)inv.Header.Insp,
                tvop = (long?)inv.Header.Tvop,
                tax17 = (long?)inv.Header.Tax17,
                cdcn = inv.Header.Cdcn,
                cdcd = inv.Header.Cdcd,
                tonw = inv.Header.Tonw,
                torv = (long?)inv.Header.Torv,
                tocv = inv.Header.Tocv
            };
            var body = new List<BodyItemDto>();

            try
            {
                inv.Rows.ForEach(r =>
                {
                    body.Add(new BodyItemDto()
                    {
                        sstid = r.sstid,
                        sstt = r.sstt,
                        mu = r.mu,
                        am = r.am,
                        fee = r.fee,
                        cfee = r.cfee,
                        // Cut = string.IsNullOrEmpty(r.Cut) ? "" : r.Cut,
                        cut = r.cut ?? r.cut,
                        exr = r.exr,
                        prdis = r.prdis,
                        dis = r.dis,
                        adis = r.adis,
                        vra = r.vra,
                        vam = r.vam,
                        odt = r.odt,
                        odr = r.odr,
                        odam = r.odam,
                        olt = r.olt,
                        olr = r.olr,
                        olam = r.olam,
                        consfee = r.consfee,
                        spro = r.spro,
                        bros = r.bros,
                        tcpbs = r.tcpbs,
                        cop = r.cop,
                        vop = r.vop,
                        bsrn = r.bsrn,
                        tsstam = r.tsstam,
                        nw = r.nw,
                        ssrv = r.ssrv,
                        sscv = r.sscv
                    });
                });

            }
            catch (Exception ex)
            {

            }
            PaymentItemDto paymentDto = new PaymentItemDto()
            {
                pmt = 3,

            };
            var invoiceDto = new InvoiceDto()
            {
                Header = header,
                Body = body,
                Payments = new List<PaymentItemDto>()
            };

            return invoiceDto;

        }

        public async Task<GeneralInvoiceViewModel> GetInvoice(string InvoiceID)
        {
            return await _context.GetInvoice(InvoiceID);
        }
        public async Task<ResultModel> SendInvoiceAsync(string invoiceId, bool isCancel = false)
        {
            ResultModel result = new ResultModel();
            try
            {
                var CurrentInvoice = await GetInvoice(invoiceId, isCancel);

                if (CurrentInvoice == null || CurrentInvoice.Header == null)
                {
                    result.Message = "No Invoice Found";
                }
                else
                {
                    var allTax = CurrentInvoice.Body.Sum(a => a.vam);
                    if (allTax != CurrentInvoice.Header.tvam)
                    {
                        result.Success = false;
                        result.Message = "مبلغ مالیات کل با جمع مبالغ مالیات نمی خواند";
                    }
                }

                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                 .LogInfoMessage($"::TaxService::" +
                     $"SendInvoice:IN:request.invoiceId::" + CurrentInvoice.ToString());

                SaveInvoiceJsonToLog(CurrentInvoice);

                var SendInvoicesResult =// await TaxApiService.Instance.TaxApis.SendInvoicesAsync(new List<InvoiceDto>() { nInvoice }, null);
               await TaxApiObject.SendInvoicesAsync(new List<InvoiceDto>() { CurrentInvoice });
                if (SendInvoicesResult != null && SendInvoicesResult.Any())
                {
                    // if (SendInvoicesResult..Result != null)
                    //{
                    //    var packetres = SendInvoicesResult.Body.Result.First();


                    //    _context.CreateSaveInvoiceResult(invoiceId, packetres.ReferenceNumber, packetres.ErrorDetail ?? "Sent", nInvoice.Header.taxid, packetres.Uid);
                    //    _context.TaxGovermentInvoiceFinalInsertUpdate(invoiceId, packetres.ErrorDetail ?? "Sent", nInvoice.Header.taxid, packetres.ReferenceNumber, isCancel);

                    //    result.success = true;
                    //    result.data = packetres;
                    //    result.message = "ثبت با موفقیت انجام شد";
                    //}
                    //else
                    //{
                    //    result.success = false;
                    //    result.data = SendInvoicesResult.Body;
                    //    result.message = "ثبت انجام نشد";
                    //}

                }
                else
                {
                    result.Success = false;
                    result.Message = "ارتباط با سامانه مودیان برقرار نشد";
                    result.Data = SendInvoicesResult;

                }

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Data = ex;
                result.Message = "exception errore:" + ex.Message;
                NPSTools.Logger.NPSLogger.
GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogError("TaxService SendInvoiceAsync:" + "::" + ex.ToString(), ex);
            }
            return result;
        }
        public async Task<ResultModel> GetInvoiceStatusByRefrenceAsync(string refId = "")
        {
            List<InquiryResultModel> inquiryResultModels = new List<InquiryResultModel>();
            ResultModel result = new ResultModel();
            try
            {
                //GetServerInformation();
                //GetToken();
                List<string> ids = new List<string>();
                ids.Add(refId);
                InquiryByReferenceNumberDto byReferenceNumberDto = new InquiryByReferenceNumberDto(ids);
                DateTime date1 = DateTime.Now;

                inquiryResultModels = await TaxApiObject.InquiryByReferenceIdAsync(byReferenceNumberDto);
                var InquiryByReferenceIdAsynctimProcess = (DateTime.Now - date1).TotalMilliseconds;
                result.Data = inquiryResultModels;

                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogInfoMessage($"::TaxService::" +
            $"InquiryByReferenceIdAsync:IN:refId::" + refId.ToString() +
            "-TimeProccess Of InquiryByReferenceIdAsync::" + InquiryByReferenceIdAsynctimProcess);

                if (inquiryResultModels == null || inquiryResultModels.Count == 0)
                    return new ResultModel() { Message = "ارتباط برقرار نشد", Status = "no thing" };

                var ParsResult = inquiryResultModels[0].ParseInquiryResult();

                if (inquiryResultModels[0].ReferenceNumber != null)
                {
                    result.Status = inquiryResultModels[0].Status;

                    _context.SaveInvoiceInquiryLog(refId,
                        inquiryResultModels[0].Status,
                        inquiryResultModels[0].Uid,
                        inquiryResultModels[0]?.Data == null ? "" :
                        inquiryResultModels[0]?.Data.ToString() ?? "",
                        inquiryResultModels[0].PacketType);

                    switch (result.Status.ToUpper())
                    {
                        case "SUCCESS":
                            _context.ChangeToDoneInvoice(refId);
                            _context.TaxGovermentInvoiceFinalInsertUpdate("", inquiryResultModels[0].Status, 1, "", refId);
                            result.Message = "صورتحساب فاقد خطا بود و با موفقیت در کارپوشه ثبت شد.";
                            break;
                        case "FAILED":
                            _context.ChangeToNotDoneInvoice(refId);
                            _context.TaxGovermentInvoiceFinalInsertUpdate("", inquiryResultModels[0].Status, 0, "", refId);
                            //_logger.LogError("TaxService GetInvoiceStatusByRefrenceAsync:" + inquiryResultModels[0].Data);
                            result.Message = "فاکتور به دلیل خطاهای:" + inquiryResultModels[0].Data + "ثبت نشده است";
                            break;
                        case "PENDING":
                        case "IN_PROGRESS":
                            result.Message = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
                            break;
                        case "NOT_FOUND":
                            _context.ChangeToNotDoneInvoice(refId);
                            _context.TaxGovermentInvoiceFinalInsertUpdate("", inquiryResultModels[0].Status, 0, "", refId);
                            result.Message = "فاکتور پیدا نشد";
                            break;
                        case "TIMEOUT":
                            result.Message = "پردازش صورتحساب بیش از اندازه طول کشیده و در کارپوشه ثبت نشد";
                            break;
                        default:
                            result.Message = "خطای پیش بینی نشده";
                            break;
                    }
                }
            }
            catch (TaxCollectData.Library.Exceptions.NotInitializedException ex)
            {
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogError($"{this.GetType().Name}::{System.Reflection.MethodBase.GetCurrentMethod().Name}"
            + ex.Message + " " + refId.ToString() + result.ToString());

                inquiryResultModels = new List<InquiryResultModel>();

                result.Message = ex.ToString();

                File.WriteAllText($"Logs\\GetInvoiceStatusByRefrenceAsync_{DateTime.Now.ToString()}.txt", ex.Message.ToString());


            }
            return result;
        }
        public async Task<List<InquiryResultModel>> GetInvoiceStatusByDateRange(string fromdate = "14020220", string todate = "14040310")
        {
            var fromDT = PersianDateTime.Parse(fromdate).ToDateTime();
            var todateDT = PersianDateTime.Parse(todate).ToDateTime();
            InquiryByTimeRangeDto timeRangeDto = new InquiryByTimeRangeDto(fromDT, todateDT);
            var inquiryResultModels = await TaxApiObject.InquiryByTimeAsync(timeRangeDto);
            var pars = inquiryResultModels[0].ParseInquiryResult();
            return inquiryResultModels ?? null;
        }
        public async Task GetNotFinishedLastRefrenceIdAsync()
        {
            var refId =await _context.GetNotFinishedLastRefrenceId();
            if (!string.IsNullOrEmpty(refId))
                await GetInvoiceStatusByRefrenceAsync(refId);
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
            string numbers = " ";
            ResultModel resultOfEachRow = new ResultModel();
            try
            {
                List<InvoiceDto> invoiceList = new List<InvoiceDto>();
                result.Message = "";
                foreach (Guid id in Ids)
                {
                    var ID = id.ToString();
                    try
                    {

                        {
                            resultOfEachRow = await SendInvoiceAsync(ID, false);
                            result.Message = result.Message + "-" + resultOfEachRow.Message;
                            if (resultOfEachRow.Success)
                            {
                                result.Count = result.Count + 1;

                            }
                            else
                            {

                                numbers = numbers + "-" + resultOfEachRow.Message;
                            }
                        }
                    }
                    catch
                    {


                    }

                }
                result.Message = numbers;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
                result.Data = ex;

            }
            return result;
        }

        public Task<ResultModel> GetInvoiceByID(string InvoiceID)
        {
            throw new NotImplementedException();
        }


    }
}