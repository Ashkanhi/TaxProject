//using MD.PersianDateTime;
//using Newtonsoft.Json;
//using ClsTools;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using TaxCollectData.Library.Abstraction.Clients;
//using TaxCollectData.Library.Abstraction.Cryptography;
//using TaxCollectData.Library.Dto;
//using TaxCollectData.Library.Factories;
//using TaxCollectData.Library.Models;
//using TaxCollectData.Library.Properties;
//using TaxServiceAndApp.Models;
//using TaxServiceAndApp.Models.SharedDTO;
//using TaxServiceAndApp.Models.TaxDTO;
////using Org.BouncyCastle.Crypto.Parameters;
////using Org.BouncyCastle.Security;
////using Org.BouncyCastle.OpenSsl;
////using Org.BouncyCastle.Asn1.X509;
//using System.Security.Cryptography.X509Certificates;
//using static NPSDTOModels.EnumsAndConsts.ClsEnums;
//using Microsoft.AspNetCore.SignalR;
//using System.Data.Entity.Migrations;
//using System.Reflection;
//using System.Text.Json;
//using JsonSerializer = System.Text.Json.JsonSerializer;
//using TaxWebAppCore8.Models.SharedDTO.BaranDB;
//using System.Data.Entity;
//using Microsoft.Extensions.Options;
//using static ClsTools.NPSEnum;
//using TaxWebAppCore8.Models.SharedDTO;
//using TaxCollectData.Library.Algorithms;
//using TaxCollectData.Library.Providers;
//using Microsoft.CodeAnalysis;

//namespace TaxServiceAndApp.BI
//{
//    public class TaxService
//    {
//        private static TaxService _taxService = null;
//        private Pkcs8SignatoryFactory pkcs8SignatoryFactory;
//        private TaxProperties properties;
//        private EncryptorFactory encryptorFactory;
//        private static TaxApiFactory taxApiFactory;
//        private ISignatory signatory;
//        private static ITaxPublicApi publicApi;
//        private IEncryptor encryptor;

//        private static bool NeedInit = true;
//        private readonly IContext _context;

//        private static ServerInformationModel ServerInformationModel;
//        private static TokenModel TokenModel;
//        #region Singleton


//        public static ITaxApi TaxApiObject;
//        public static ILowLevelTaxApi LowLevelApi;
//        public static TaxService getInstance(IContext context)
//        {

//            if (_taxService == null)
//            {
//                _taxService = new TaxService(context);

//            }

//            return _taxService;

//        }
//        private TaxService(IContext context)
//        {
//            _context = context;

//        }
//        #endregion
//        #region Logs
//        private void StartInitLog() => ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage($"Start Init Log Tax Service");
//        private void EndInitLog() => ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage($"End Init Tax Service");

//        private void TaxSettingCountLog(int count) => ClsTools.Logger.NPSLogger.
//            GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage(($"Get TaxSetting Finished,Count {0}", count).ToString());

//        private void GetServerInformationLog() => ClsTools.Logger.NPSLogger.
//            GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage("Get Tax Server Information");

//        private void MakeInvoiceDtoLog()
//            => ClsTools.Logger.NPSLogger.
//            GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage("Make Invoice Dto");

//        private void GenerateTaxIdLog() => ClsTools.Logger.NPSLogger.
//            GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage("Generate Tax Id Log");

//        private void GetInvoiceStatusByRefrenceLog() => ClsTools.Logger.NPSLogger.
//            GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage("Get Invoice Status By Refrence Log");


//        private void SaveInvoiceJsonToLog(object obj)
//        {
//            try
//            {
//                var json = JsonConvert.SerializeObject(obj);
//                // string
//                // Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dto.txt");

//                PersianCalendar pc = new PersianCalendar();

//                int year = pc.GetYear(DateTime.Now);
//                int month = pc.GetMonth(DateTime.Now);
//                int day = pc.GetDayOfMonth(DateTime.Now);
//                int hour = pc.GetHour(DateTime.Now);
//                int minute = pc.GetMinute(DateTime.Now);

//                StringBuilder sb = new StringBuilder();

//                sb.Append(year.ToString());
//                sb.Append("_");
//                sb.Append(month.ToString());
//                sb.Append("_");
//                sb.Append(day.ToString());
//                sb.Append("_T_");
//                sb.Append(hour.ToString());
//                sb.Append("_");
//                sb.Append(minute.ToString());


//                try
//                {

//                    /*
//                    // Create a subdirectory named "jasonlogs" within the base directory
//                    string subFolderPath = Path.Combine(basePath, "jasonlogs");
//                    // Ensure the subdirectory exists
//                    Directory.CreateDirectory(subFolderPath);

//                    // Now, combine the subfolder path with your file name
//                    string filePath = Path.Combine(subFolderPath, $"JasonLog_{sb.ToString()}.txt");

//                    // Write the JSON content to the file
//                    File.WriteAllText(filePath, json.ToString());
//                    */


//                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
//                    string jasonlogsPath = Path.Combine(basePath, "jasonlogs");

//                    // Ensure the subdirectory exists
//                    Directory.CreateDirectory(jasonlogsPath);

//                    File.WriteAllText($"{jasonlogsPath}\\JasonLog_{sb.ToString()}.txt", json.ToString());

//                    //File.WriteAllText($"Logs\\JasonLog_{sb.ToString()}.txt", (string?)JsonConvert.DeserializeObject(json));
//                    //File.WriteAllText($"{basePath}\\JasonLog_{sb.ToString()}.txt", (string?)JsonConvert.DeserializeObject(json));


//                    //File.WriteAllText($"Logs\\DTOS.txt", DateTime.Now.ToString() + "AAA" + "_" + json);
//                    // File.WriteAllText($"Logs\\DTOS.txt", DateTime.Now.ToString() + "AAA" + "_" + json);
//                }
//                catch (Exception ex)
//                {
//                    string basePath = AppDomain.CurrentDomain.BaseDirectory;
//                    string jasonlogsPath = Path.Combine(basePath, "jasonlogs");
//                    // Ensure the subdirectory exists
//                    Directory.CreateDirectory(jasonlogsPath);

//                    string filePath = Path.Combine(basePath, "GetError.txt");

//                    File.WriteAllLines(filePath, new string[] { ex.Message });
//                }



//                ClsTools.Logger.NPSLogger.
//           GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                .LogInfoMessage("DTOJson" + "::" + json);

//                // _logger.LogDebug("DTOJson", json);
//            }
//            catch (Exception er)
//            {
//                ClsTools.Logger.NPSLogger.
//          GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//               .LogError("DTOJson" + "::" + er.Message, er);
//                // _logger.LogError("DTOJson");

//            }

//        }
//        #endregion

//        public async Task Init()
//        {
//            try
//            {

//                if (BMain.SrvInfo.TokenType == "CertificateFile")
//                    TaxInitWithFileCertificate();
//                else
//                    TaxInitWithTokenCertificate();

//                NeedInit = false;
//            }
//            catch (Exception rr)
//            {
//                throw rr;
//            }
//            //}
//        }
//        private void TaxInitWithFileCertificate()
//        {
//            try
//            {

//                StartInitLog();

//                pkcs8SignatoryFactory = new Pkcs8SignatoryFactory();
//                properties = new TaxProperties(BMain.SrvInfo.MEMORY_ID);
//                encryptorFactory = new EncryptorFactory();
//                taxApiFactory = new TaxApiFactory(BMain.SrvInfo.API_URL, properties);

//                //var privateKeyPemReader = new PemReader(File.OpenText(BMain.SrvInfo.FILE_KEY_PRIVATE));

//                //var toRSA = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)privateKeyPemReader.ReadObject());

//                //string certPath = @"C:\key\arghamnegarandisheh [Stamp].crt";
//                //var certBytes = File.ReadAllBytes(certPath);
//                //var certificate = new X509Certificate2(certBytes);


//                //var file = File.ReadAllBytes(@"F:\4Ashkan\10100341929_private_key.txt");
//                // var ce = new X509Certificate(File.ReadAllBytes(@"F:\4Ashkan\10100341929_private_key.txt"));


//                signatory = pkcs8SignatoryFactory.Create(
//             BMain.SrvInfo.FILE_KEY_PRIVATE,
//             BMain.SrvInfo.FILE_CERTIFICATE);

//                publicApi = taxApiFactory.CreatePublicApi(signatory);
//                encryptor = encryptorFactory.Create(publicApi);
//                TaxApiObject = taxApiFactory.CreateApi(signatory, encryptor);
//                LowLevelApi = taxApiFactory.CreateLowLevelApi(signatory);

//                var invoices = new List<Invoice>();


//                //var SendInvoicesResult =// await TaxApiService.Instance.TaxApis.SendInvoicesAsync(new List<InvoiceDto>() { nInvoice }, null);
//                //TaxApiObject.SendInvoices(new List<InvoiceDto>() {});
//                EndInitLog();

//            }
//            catch (Exception sdsd)
//            {

//                throw sdsd;
//            }
//        }
//        private void TaxInitWithTokenCertificate()
//        {
//            StartInitLog();
//            Pkcs11SignatoryFactory pkcs11SignatoryFactory = new Pkcs11SignatoryFactory();
//            EncryptorFactory encryptorFactory = new EncryptorFactory();
//            properties = new TaxProperties(BMain.SrvInfo.MEMORY_ID);
//            taxApiFactory = new TaxApiFactory(BMain.SrvInfo.API_URL, properties);
//            ISignatory signatory = pkcs11SignatoryFactory
//             .Create(BMain.SrvInfo.CertificateName, BMain.SrvInfo.LIBRARY_PATH, BMain.SrvInfo.PASSWORD);

//            ITaxPublicApi publicApi = taxApiFactory.CreatePublicApi(signatory);
//            IEncryptor encryptor = encryptorFactory.Create(publicApi);
//            TaxApiObject = taxApiFactory.CreateApi(signatory, encryptor);
//            LowLevelApi = taxApiFactory.CreateLowLevelApi(signatory);

//            EndInitLog();
//        }
//        private string GenerateTaxId(long InvoiceNumber, DateTime createDate, bool IsCancel = false)
//        {

//            var id = BTaxMain.CreateUniqeTaxID(InvoiceNumber, createDate);

//            return id;
//        }
//        public async Task<InvoiceDto> GetInvoice(DTOInvoiceLimitParameter invoice, bool isCancel)
//        {

//            Invoice inv = _context.GetLastInvoiceForSend(
//                (short)(isCancel == true ? 3 : 1), invoice, 1);
//            if (inv == null)
//                return null;

//            //invoiceId = inv.Header.InvoiceId.ToString();
//            //var date = inv.Header.BusinessDate;
//            //var taxId = GenerateTaxId(long.Parse(inv.Header.Inno.Replace("-", "")), date, isCancel); //(isCancel == true ? inv.Header.Irtaxid : GenerateTaxId(long.Parse(inv.Header.Inno.Replace("-", "")), date, isCancel));
//            DateTime calcTissDocumentDate = inv.Header.InvoiceDate ?? DateTime.Now.AddHours(-3);// ?? DateTime.Now; // Replace with your actual date
//            DateTime tissDocumentDate = calcTissDocumentDate.AddHours(-3);

//            // Convert to Unix timestamp in milliseconds

//            // Convert to string
//            //Indatim = 1711196764000
//            //1711150200000
//            var date = inv.Header.InvoiceDate ?? DateTime.Now.AddHours(-3);
//            var taxId = BTaxMain.CreateUniqeTaxID(long.Parse(inv.Header.Inno.Replace("-", "")), date);

//            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//               .LogInfoMessage($"::TaxService::" +
//                   $"CreateUniqeTaxID::taxId::" + taxId + "invoiceID::" +
//                   invoice.InvoiceID + "inv.Header.InvoiceDate" + inv.Header.InvoiceDate ?? "NOW");

//            TaxIdProvider taxIdProvider = new TaxIdProvider(new VerhoeffAlgorithm());
//            string memoryId = BMain.SrvInfo.MEMORY_ID;

//            DateTimeOffset now = new DateTimeOffset(inv.Header.InvoiceDate ?? DateTime.Now);

//            taxId = taxIdProvider.GenerateTaxId(
//            memoryId,
//            long.Parse(inv.Header.Inno.Replace("-", "")),
//            now.DateTime);

//            //string inno = long.Parse(inv.Header.Inno.Replace("-", ""))
//            //.ToString("X")
//            //.PadLeft(10, '0');

//            long indatim = now.ToUnixTimeMilliseconds();
//            invoice.TaxGuid = taxId;

//            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//       .LogInfoMessage($"::TaxService::" +
//           $"CreateUniqeTaxID::NewtaxId::" + taxId + "::indatim" + indatim + "::invoiceID::" + invoice.InvoiceID);

//            var header = new HeaderDto()
//            {
//                taxid = taxId,
//                indatim = indatim,
//                indati2m = indatim,
//                inty = inv.Header.Inty,
//                inno = string.Format("{0:X}", inv.Header.Inno.Replace("-", "")).PadLeft(10, '0'),
//                irtaxid = inv.Header.Irtaxid,
//                inp = inv.Header.Inp ?? 0,
//                ins = inv.Header.Ins ?? 0,
//                tins = BMain.SrvInfo.TINS,
//                tob = inv.Header.Tob,
//                bid = inv.Header.Bid,
//                tinb = inv.Header.Tinb,
//                sbc = inv.Header.Sbc,
//                bpc = inv.Header.Bbc,
//                bbc = inv.Header.Bbc,
//                ft = null, // inv.Header.Ft ?? 0,
//                bpn = inv.Header.Bpn,
//                scln = inv.Header.Scln,
//                scc = inv.Header.Scc,
//                crn = inv.Header.Crn,
//                billid = inv.Header.Billid,
//                tprdis = (long?)inv.Header.Tprdis ?? 0,
//                tdis = (long?)inv.Header.Tdis ?? 0,
//                tadis = (long?)inv.Header.Tadis ?? 0,
//                tvam = (long?)inv.Header.Tvam ?? 0,
//                todam = inv.Header.Todam == null ? 0 : (long?)inv.Header.Todam,
//                tbill = (long?)inv.Header.Tbill ?? 0,
//                setm = inv.Header.Setm ?? 0,
//                cap = (long?)inv.Header.Cap ?? 0,
//                insp = (long?)inv.Header.Insp ?? 0,
//                tvop = (long?)inv.Header.Tvop ?? 0,
//                tax17 = (long?)inv.Header.Tax17 ?? 0,
//                cdcn = inv.Header.Cdcn,
//                cdcd = null,// inv.Header.Cdcd??0,
//                tonw = null,// inv.Header.Tonw??0,
//                torv = null,//  (long?)inv.Header.Torv,
//                tocv = null,//  inv.Header.Tocv,
//            };

//            PaymentItemDto paymentDto = new PaymentItemDto()
//            {
//                pmt = 3,
//            };
//            var invoiceDto = new InvoiceDto()
//            {
//                Header = header,
//                Body = inv.Rows,
//                Payments = new List<PaymentItemDto>() { paymentDto }
//            };

//            return invoiceDto;

//        }

//        public async Task<GeneralInvoiceViewModel> GetInvoice(DTOInvoiceLimitParameter Invoice)
//        {
//            return await _context.GetInvoice(Invoice.InvoiceID);
//        }
//        public async Task<ResultModel> SendInvoiceAsync(DTOInvoiceLimitParameter Invoice, bool isCancel = false)
//        {
//            ResultModel result = new ResultModel();
//            try
//            {
//                var CurrentInvoice = await GetInvoice(Invoice, isCancel);

//                if (CurrentInvoice == null || CurrentInvoice.Header == null)
//                {
//                    result.Message = "No Invoice Found";
//                }
//                else
//                {
//                    var allTax = CurrentInvoice.Body.Sum(a => a.vam);
//                    if (allTax != CurrentInvoice.Header.tvam)
//                    {
//                        result.Success = false;
//                        result.Message = "مبلغ مالیات کل با جمع مبالغ مالیات نمی خواند";
//                    }
//                }
//                result.TaxBreif = CurrentInvoice;

//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                 .LogInfoMessage($"::TaxService::" +
//                     $"SendInvoice:IN:request.invoiceId::" + CurrentInvoice?.ToString());

//                SaveInvoiceJsonToLog(CurrentInvoice);

//                var SendInvoicesResult =// await TaxApiService.Instance.TaxApis.SendInvoicesAsync(new List<InvoiceDto>() { nInvoice }, null);
//               await TaxApiObject.SendInvoicesAsync(new List<InvoiceDto>() { CurrentInvoice });
//                if (SendInvoicesResult != null && SendInvoicesResult.Any())
//                {
//                    if (SendInvoicesResult.Any())
//                    {
//                        var packetres = SendInvoicesResult.FirstOrDefault();
//                        _context.CreateSaveInvoiceResult(Invoice.InvoiceID, packetres.ReferenceNumber, "Sent",
//                            CurrentInvoice.Header.taxid, packetres.Uid);
//                        _context.TaxGovermentInvoiceFinalInsertUpdate(Invoice.InvoiceID, "Sent", CurrentInvoice.Header.taxid,
//                            packetres.ReferenceNumber, isCancel);
//                        _ = BMain.ScheduleOperationAsync(Invoice.InvoiceID);

//                        // in this point ScheduleOperationAsync must run

//                        result.Success = true;
//                        result.Data = packetres;
//                        result.Message = "ثبت با موفقیت انجام شد";
//                    }
//                    else
//                    {
//                        result.Success = false;
//                        result.Data = null;
//                        result.Message = "ثبت انجام نشد";
//                    }
//                }
//                else
//                {
//                    result.Success = false;
//                    result.Message = "ارتباط با سامانه مودیان برقرار نشد";
//                    result.Data = SendInvoicesResult;

//                }

//            }
//            catch (Exception ex)
//            {
//                result.Success = false;
//                result.Data = ex;
//                result.Message = "exception errore:" + ex.Message;
//                ClsTools.Logger.NPSLogger.
//GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//.LogError("TaxService SendInvoiceAsync:" + "::" + ex.ToString(), ex);
//            }
//            return result;
//        }

//        public async Task<ResultModel> GetInvoiceStatusByRefrenceAsync(string invoicestr = "")
//        {
//            List<InquiryResultModel> inquiryResultModels = new List<InquiryResultModel>();
//            ResultModel result = new ResultModel();
//            try
//            {
//                var CurrentInvoice = _context.GetTaxHistory(Guid.Parse(invoicestr));

//                List<string> ids = new List<string>();
//                ids.Add(CurrentInvoice.ReferenceNumber);
//                InquiryByReferenceNumberDto byReferenceNumberDto = new InquiryByReferenceNumberDto(ids);
//                DateTime date1 = DateTime.Now;

//                inquiryResultModels = await TaxApiObject.InquiryByReferenceIdAsync(byReferenceNumberDto);
//                var InquiryByReferenceIdAsynctimProcess = (DateTime.Now - date1).TotalMilliseconds;
//                result.Data = inquiryResultModels;

//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//            .LogInfoMessage($"::TaxService::" +
//            $"InquiryByReferenceIdAsync:IN:refId::" + CurrentInvoice.ReferenceNumber.ToString() +
//            "-TimeProccess Of InquiryByReferenceIdAsync::" + InquiryByReferenceIdAsynctimProcess);

//                if (inquiryResultModels == null || inquiryResultModels.Count == 0)
//                    return new ResultModel() { Message = "ارتباط برقرار نشد", Status = "no thing" };

//                if (inquiryResultModels[0].ReferenceNumber != null)
//                {
//                    result.Status = inquiryResultModels[0].Status;
//                    if (result.Status == "IN_PROGRESS")
//                        _ = BMain.ScheduleOperationAsync(invoicestr, 1);
//                    _context.SaveInvoiceInquiryLog(CurrentInvoice.ReferenceNumber,
//                        inquiryResultModels[0].Status,
//                        inquiryResultModels[0].Uid,
//                        inquiryResultModels[0]?.Data == null ? "" :
//                        inquiryResultModels[0]?.Data.ToString() ?? "",
//                        inquiryResultModels[0].PacketType);

//                    switch (result.Status.ToUpper())
//                    {
//                        case "SUCCESS":
//                            _context.ChangeToDoneInvoice(CurrentInvoice.ReferenceNumber);
//                            _context.TaxGovermentInvoiceFinalInsertUpdate(invoicestr, inquiryResultModels[0].Status, 1, "", CurrentInvoice.ReferenceNumber);
//                            result.Message = "صورتحساب فاقد خطا بود و با موفقیت در کارپوشه ثبت شد.";
//                            {
//                                var message = @"صورتحساب فاقد خطا بود و با موفقیت در کارپوشه ثبت شد.";
//                                await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);
//                            }
//                            break;
//                        case "FAILED":
//                            var errorList = String.Join("::", inquiryResultModels[0].Data.Error.Select(a => a.Message).ToList());
//                            var warningList = String.Join("::", inquiryResultModels[0].Data.Warning.Select(a => a.Message).ToList());

//                            _context.ChangeToNotDoneInvoice(CurrentInvoice.ReferenceNumber);
//                            _context.TaxGovermentInvoiceFinalInsertUpdate(invoicestr, inquiryResultModels[0].Status, 0, "", CurrentInvoice.ReferenceNumber);
//                            result.Message = "فاکتور به دلیل خطاهای:" + errorList + "ثبت نشده است";
//                            {
//                                var message = @"فاکتور به دلیل خطاهای:" + errorList + "ثبت نشده است";
//                                await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
//                            }
//                            break;
//                        case "PENDING":
//                        case "IN_PROGRESS":
//                            result.Message = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
//                            {
//                                var message = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
//                                await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
//                            }
//                            break;
//                        case "NOT_FOUND":
//                            _context.ChangeToNotDoneInvoice(CurrentInvoice.ReferenceNumber);
//                            _context.TaxGovermentInvoiceFinalInsertUpdate(invoicestr, inquiryResultModels[0].Status, 0, "", CurrentInvoice.ReferenceNumber);
//                            result.Message = "فاکتور پیدا نشد";
//                            {
//                                var message = "فاکتور پیدا نشد";
//                                await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);
//                            }
//                            break;
//                        case "TIMEOUT":
//                            result.Message = "پردازش صورتحساب بیش از اندازه طول کشیده و در کارپوشه ثبت نشد";
//                            break;
//                        default:
//                            result.Message = "خطای پیش بینی نشده";
//                            break;
//                    }
//                }
//            }
//            catch (TaxCollectData.Library.Exceptions.NotInitializedException ex)
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//            .LogError($"{this.GetType().Name}::{System.Reflection.MethodBase.GetCurrentMethod().Name}"
//            + ex.Message + " " + invoicestr);

//                inquiryResultModels = new List<InquiryResultModel>();

//                result.Message = ex.ToString();
//            }
//            return result;
//        }


//        public async Task<List<GeneralInvoiceHeaderViewModel>> GetInvoiceStatusByReferenceListEnhanced(List<GeneralInvoiceHeaderViewModel> models)
//        {
//            List<InquiryResultModel> inquiryResultModels = new List<InquiryResultModel>();
//            try
//            {
//                // Filter references with fSendState == 2
//                var references = models
//                    .Where(a => a.fSendState == 2)
//                    .Select(a => a.fTaxHistoryReferenceNumber)
//                    .ToList();

//                ClsTools.Logger.NPSLogger
//                    .GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//                    .LogInfoMessage($"::TaxService::references count::{references.Count}::models count::{models.Count}");

//                DateTime startTime = DateTime.Now;

//                // Break references into chunks of 50
//                var referenceBatches = references
//                    .Select((value, index) => new { value, index })
//                    .GroupBy(x => x.index / 20)
//                    .Select(g => g.Select(x => x.value).ToList())
//                    .ToList();

//                foreach (var batch in referenceBatches)
//                {
//                    ClsTools.Logger.NPSLogger
//                    .GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//                    .LogInfoMessage($"::TaxService::referenceBatches batch count::{batch.Count}");

//                    var byReferenceNumberDto = new InquiryByReferenceNumberDto(batch);
//                    var batchResult = await TaxApiObject.InquiryByReferenceIdAsync(byReferenceNumberDto);
//                    if (batchResult != null)
//                    {
//                        inquiryResultModels.AddRange(batchResult);
//                    }
//                }

//                ClsTools.Logger.NPSLogger
//                    .GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//                    .LogInfoMessage($"::TaxService::inquiryResultModels count::{inquiryResultModels.Count}");

//                var processTime = (DateTime.Now - startTime).TotalMilliseconds;

//                if (inquiryResultModels.Count == 0)
//                {
//                    models.ForEach(a => a.InqMessage = "!");
//                    return models;
//                }
//                int countofAll = inquiryResultModels.Count;
//                int countofSend = 0;
//                // Process the results
//                foreach (var model in models)
//                {
//                    ClsTools.Logger.NPSLogger
//                    .GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//                    .LogInfoMessage($"::TaxService::foreach (var model in models) count::{model.ReferenceNumber}");

//                    var item = inquiryResultModels.FirstOrDefault(a =>
//                        a.ReferenceNumber == model.fTaxHistoryReferenceNumber ||
//                        a.ReferenceNumber == model.ReferenceNumber);

//                    model.ResultModel = item;
//                    string status = "";
//                    if (item == null)
//                    {
//                        ClsTools.Logger.NPSLogger
//                    .GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//                    .LogInfoMessage($"::TaxService::if (item == null)::{model.ReferenceNumber}");

//                        model.InqMessage = model.fListOfErrorCaption;
//                    }
//                    else
//                    {
//                        status = await ProcessResultAsync(model, item); // Handle based on the result status
//                    }
//                    var message = "فاکتور شماره " + model.InvoiceNumberSt + " با موفقیت احراز شد. \r\n" +
//                           "وضعیت ثبت: " + status + "\r\n " +
//                               "\r\n تعداد : " + countofSend +
//                              " از " + countofAll;

//                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxInq", message);
//                    countofSend++;
//                }
//            }
//            catch (Exception ex)
//            {
//                ClsTools.Logger.NPSLogger
//                    .GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//                    .LogError($"Error in {nameof(GetInvoiceStatusByReferenceListEnhanced)}: {ex.Message}", ex);

//                inquiryResultModels = new List<InquiryResultModel>();
//            }

//            return models;
//        }

//        // Separate method to handle results for clarity
//        private async Task<string> ProcessResultAsync(GeneralInvoiceHeaderViewModel model, InquiryResultModel item)
//        {
//            ClsTools.Logger.NPSLogger
//                    .GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//                    .LogInfoMessage($"::TaxService::item.Status.ToUpper() count::{item.Status.ToUpper()}");
//            string status = "";
//            _context.SaveInvoiceInquiryLog
//                            (
//                            model.fTaxHistoryReferenceNumber,
//                            item.Status,
//                            item.Uid,
//                            item.Data == null ? "" :
//                            item.Data.ToString() ?? "",
//                            item.PacketType
//                            );

//            switch (item.Status.ToUpper())
//            {
//                case "SUCCESS":
//                    await HandleSuccessAsync(model, item);
//                    //model.InqMessage = "ثبت موفق";
//                    status = "ثبت موفق";
//                    break;
//                case "FAILED":
//                    await HandleFailedAsync(model, item);
//                    //model.InqMessage = "خطا";
//                    status = "خطا";
//                    break;
//                case "PENDING":
//                case "IN_PROGRESS":
//                    await HandlePendingAsync(model, item);
//                    // model.InqMessage = "در حال بررسی";
//                    status = "در حال بررسی";
//                    break;
//                case "TIMEOUT":
//                    model.InqMessage = "پردازش صورتحساب بیش از اندازه طول کشیده و در کارپوشه ثبت نشد";
//                    status = "پردازش صورتحساب بیش از اندازه طول کشیده و در کارپوشه ثبت نشد";
//                    break;

//                case "NOT_FOUND":
//                default:
//                    status = "فاکتور پیدا نشد";
//                    model.InqMessage = "فاکتور پیدا نشد";
//                    using (var npsContext = new NPSGeneralContext())
//                    {
//                        var errror = new errorDetail()
//                        {
//                            code = "0000",
//                            details = "فاکتور پیدا نشد!",
//                            message = "فاکتور پیدا نشد!"
//                        };
//                        await _context.LogErrorsAsync(npsContext, new List<errorDetail> { errror });
//                        var taxhistory = await
//npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == model.InvoiceId);
//                        if (taxhistory == null)
//                        {
//                            taxhistory = new tTaxHistoryStateParallel();
//                            taxhistory.ReferenceNumber = model.fTaxHistoryReferenceNumber;
//                            taxhistory.fInvoiceID = model.InvoiceId;
//                        }
//                        {
//                            taxhistory.fListOfErrorCaption =
//                                string.Join('-', "فاکتور پیدا نشد!");
//                            //taxhistory.fSendState = 1;

//                            taxhistory.fListOfErrorCode =
//                                string.Join('-', "فاکتور پیدا نشد!");
//                            npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

//                        }
//                        await npsContext.SaveChangesAsync();
//                    }
//                    model.InqMessage = "فاکتور پیدا نشد!";

//                    _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "خطای ناشناس - فاکتور پیدا نشد", 0, "", model.fTaxHistoryReferenceNumber);
//                    break;
//            }

//            return status;
//        }
















//        public async Task<List<GeneralInvoiceHeaderViewModel>> GetInvoiceStatusByRefrenceList(List<GeneralInvoiceHeaderViewModel> models)
//        {
//            List<InquiryResultModel> inquiryResultModels = new List<InquiryResultModel>();
//            try
//            {
//                List<string> ids = new List<string>();
//                var refrences = models.Where(a => a.fSendState == 2).Select(a => a.fTaxHistoryReferenceNumber).ToList();
//                ClsTools.Logger.NPSLogger.
//                    GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//      .LogInfoMessage($"::TaxService::" +
//          $"refrences::" + refrences.Count() + "::models::" + models.Count());

//                DateTime date1 = DateTime.Now;

//                InquiryByReferenceNumberDto byReferenceNumberDto = new
//                    InquiryByReferenceNumberDto(refrences);
//                inquiryResultModels = await TaxApiObject.InquiryByReferenceIdAsync
//                    (byReferenceNumberDto);
//                ClsTools.Logger.NPSLogger.
//                  GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//    .LogInfoMessage($"::TaxService::" +
//        $"inquiryResultModels::::" + inquiryResultModels.Count());
//                var InquiryByReferenceIdAsynctimProcess = (DateTime.Now - date1).TotalMilliseconds;

//                if (inquiryResultModels == null || inquiryResultModels.Count == 0)
//                    return null;

//                if (inquiryResultModels[0].ReferenceNumber != null)
//                {
//                    ClsTools.Logger.NPSLogger.
//                   GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//     .LogInfoMessage($"::TaxService::" +
//         $"if (inquiryResultModels[0].ReferenceNumber != null)");
//                    int countofAll = inquiryResultModels.Count;
//                    int countofSend = 0;
//                    foreach (var model in models)
//                    {
//                        string status = "";

//                        var item = inquiryResultModels.FirstOrDefault(a => a.ReferenceNumber == model.fTaxHistoryReferenceNumber
//                        || a.ReferenceNumber == model.ReferenceNumber);
//                        model.ResultModel = item;
//                        try
//                        {
//                            ClsTools.Logger.NPSLogger.
//                     GetLogger(ClsTools.Logger.ModuleName.ArghamTaxService)
//       .LogInfoMessage($"::TaxService::" +
//           $"item::" + item?.ReferenceNumber + "::" + item?.Status);
//                        }
//                        catch { }
//                        if (item == null)
//                        {
//                            model.InqMessage = model.fListOfErrorCaption;
//                            status = model.SendStatStr;
//                        }
//                        else
//                        {
//                            _context.SaveInvoiceInquiryLog
//                                (
//                                model.fTaxHistoryReferenceNumber,
//                                item.Status,
//                                item.Uid,
//                                item.Data == null ? "" :
//                                item.Data.ToString() ?? "",
//                                item.PacketType
//                                );

//                            switch (item.Status.ToUpper())
//                            {

//                                case "SUCCESS":

//                                    await HandleSuccessAsync(model, item);
//                                    status = "ثبت موفق";
//                                    break;

//                                case "FAILED":
//                                    await HandleFailedAsync(model, item);
//                                    status = "خطا";
//                                    break;
//                                //var errorList = String.Join("::", item.Data.Error.Select(a => a.Message).ToList());
//                                //var warningList = String.Join("::", item.Data.Warning.Select(a => a.Message).ToList());

//                                //_context.ChangeToNotDoneInvoice(model.fTaxHistoryReferenceNumber);
//                                //_context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, item.Status, 0, "",
//                                //    model.fTaxHistoryReferenceNumber);
//                                //model.InqMessage = "فاکتور به دلیل خطاهای:" + errorList + "ثبت نشده است" + "\r\n " + warningList;
//                                //break;
//                                case "PENDING":
//                                case "IN_PROGRESS":


//                                    await HandlePendingAsync(model, item);
//                                    status = "در حال بررسی";
//                                    break;


//                                //model.InqMessage = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
//                                //break;


//                                case "TIMEOUT":
//                                    model.InqMessage = "پردازش صورتحساب بیش از اندازه طول کشیده و در کارپوشه ثبت نشد";
//                                    break;

//                                case "NOT_FOUND":
//                                default:
//                                    status = "فاکتور پیدا نشد";
//                                    using (var npsContext = new NPSGeneralContext())
//                                    {
//                                        var errror = new errorDetail()
//                                        {
//                                            code = "0000",
//                                            details = "فاکتور پیدا نشد!",
//                                            message = "فاکتور پیدا نشد!"
//                                        };
//                                        await _context.LogErrorsAsync(npsContext, new List<errorDetail> { errror });
//                                        var taxhistory = await
//    npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == model.InvoiceId);
//                                        if (taxhistory == null)
//                                        {
//                                            taxhistory = new tTaxHistoryStateParallel();
//                                            taxhistory.ReferenceNumber = model.fTaxHistoryReferenceNumber;
//                                            taxhistory.fInvoiceID = model.InvoiceId;
//                                        }
//                                        {
//                                            taxhistory.fListOfErrorCaption =
//                                                string.Join('-', "فاکتور پیدا نشد!");
//                                            //taxhistory.fSendState = 1;

//                                            taxhistory.fListOfErrorCode =
//                                                string.Join('-', "فاکتور پیدا نشد!");
//                                            npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

//                                        }
//                                        await npsContext.SaveChangesAsync();
//                                    }
//                                    model.InqMessage = "فاکتور پیدا نشد!";

//                                    _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "خطای ناشناس - فاکتور پیدا نشد", 0, "", model.fTaxHistoryReferenceNumber);
//                                    break;


//                            }
//                        }

//                        var message = "فاکتور شماره " + model.InvoiceNumberSt + " با موفقیت احراز شد. \r\n" +
//                                "وضعیت ثبت: " + status + "\r\n " +
//                                    "\r\n تعداد : " + countofSend +
//                                   " از " + countofAll;

//                        await BMain._hubContext.Clients.All.SendAsync("SendToTaxInq", message);
//                        countofSend++;
//                    }
//                }
//                else
//                {
//                    models.ForEach(a => a.InqMessage = "ارتباط با سرور مالیات برقرار نگردید!");

//                    {
//                        foreach (var item in models)
//                        {
//                            if (item.fSendState != 2)
//                            {
//                                item.InqMessage = item.fListOfErrorCaption;
//                            }
//                        }

//                    }
//                }
//            }
//            catch (TaxCollectData.Library.Exceptions.NotInitializedException ex)
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//            .LogError($"{this.GetType().Name}::{System.Reflection.MethodBase.GetCurrentMethod().Name}"
//            + ex.Message, ex);

//                inquiryResultModels = new List<InquiryResultModel>();

//            }
//            return models;
//        }
























//        private async Task HandleSuccessAsync(GeneralInvoiceHeaderViewModel model, InquiryResultModel inquiry)
//        {
//            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//            .LogInfoMessage($"::TissService::" +
//                       $"HandleSuccessAsync:Out: " + model.InvoiceId.ToString() + "::Success");

//            _context.ChangeToDoneInvoice(model.fTaxHistoryReferenceNumber);
//            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, inquiry.Status,
//                1, "", model.fTaxHistoryReferenceNumber);

//            model.InqMessage = "صورتحساب فاقد خطا بود و با موفقیت در کارپوشه ثبت شد.";

//            using (var npsContext = new NPSGeneralContext())
//            {
//                var maxLastOrder = await npsContext.
//                    tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower()).
//                    OrderByDescending(a => a.fID).FirstOrDefaultAsync();

//                if (maxLastOrder != null)
//                {
//                    maxLastOrder.fStatus = 4;
//                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(maxLastOrder);
//                    await npsContext.SaveChangesAsync();

//                }
//                else
//                {
//                    maxLastOrder = new tSendInvoiceTaxTissLog()
//                    {
//                        fInvoiceID = model.InvoiceId,
//                        fLastSendDate = DateTime.Now,
//                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
//                        fReferencedID = model.fTaxHistoryReferenceNumber,
//                        fStatus = 4,
//                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
//                    };
//                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, maxLastOrder);
//                    await npsContext.SaveChangesAsync();
//                }



//                var taxhistory = await
//                  npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower());

//                if (taxhistory == null)
//                {
//                    taxhistory = new tTaxHistoryStateParallel();
//                    taxhistory.ReferenceNumber = maxLastOrder.fReferencedID;
//                    taxhistory.fInvoiceID = model.InvoiceId;
//                }

//                if (taxhistory != null)
//                {
//                    taxhistory.fListOfErrorCaption = "ثبت موفق";
//                    taxhistory.fListOfErrorCode = "ثبت موفق";
//                    taxhistory.fSendState = 4;
//                    npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);
//                }
//                await npsContext.SaveChangesAsync();
//            }

//            model.InqMessage = "فاکتور با موفقیت ثبت شده است.";
//        }

//        private async Task HandleFailedAsync(GeneralInvoiceHeaderViewModel model, InquiryResultModel inquiry)
//        {
//            var errorMessages = string.Join("::", inquiry.Data.Error.Select(e => e.Message));
//            var warningMessages = string.Join("::", inquiry.Data.Warning.Select(w => w.Message));

//            using (var npsContext = new NPSGeneralContext())
//            {
//                try
//                {
//                    var errordetails = new List<errorDetail>();
//                    foreach (var item in inquiry.Data.Error)
//                    {
//                        var errordetail = new errorDetail() {
//                        code=item.Code,
//                        message=item.Message
//                        };
//                        errordetails.Add(errordetail);
//                    }
//                    await _context.LogErrorsAsync(npsContext, errordetails);

//                    var maxLastOrder = await npsContext.
//                                     tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower()).
//                                     OrderByDescending(a => a.fID).FirstOrDefaultAsync();

//                    if (maxLastOrder != null)
//                    {
//                        maxLastOrder.fStatus = 3;
//                        npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(maxLastOrder);
//                        await npsContext.SaveChangesAsync();

//                    }
//                    else
//                    {
//                        maxLastOrder = new tSendInvoiceTaxTissLog()
//                        {
//                            fInvoiceID = model.InvoiceId,
//                            fLastSendDate = DateTime.Now,
//                            fLogUID = ClsTools.ClsTools.CreateUniqueID(),
//                            fReferencedID = model.fTaxHistoryReferenceNumber,
//                            fStatus = 3,
//                            fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
//                        };
//                        npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, maxLastOrder);
//                        await npsContext.SaveChangesAsync();
//                    }

//                    foreach (var error in inquiry.Data.Error)
//                    {
//                        try
//                        {
//                            var existingError = await npsContext.tTisTaxErrorTypes
//                                .FirstOrDefaultAsync(e => e.fType == 1 && e.fCode.ToLower().Trim() == error.Code.ToLower().Trim());

//                            if (existingError == null)
//                            {
//                                var newError = new tTisTaxErrorType
//                                {
//                                    fCaption = error.Message,
//                                    fDescription = error.Message,
//                                    fCode = error.Code,
//                                    fType = 1,
//                                };
//                                npsContext.tTisTaxErrorTypes.AddOrUpdate(e => e.fID, newError);
//                                await npsContext.SaveChangesAsync();
//                                existingError = await npsContext.tTisTaxErrorTypes
//                                .FirstOrDefaultAsync(e => e.fType == 1 && e.fCode.ToLower().Trim() == error.Code.ToLower().Trim());
//                            }

//                            var tErrorslog = new tErrorsOfLog()
//                            {
//                                fCaption = error.Message,
//                                fErrorCode = error.Code,
//                                fErrorType = existingError.fID,
//                                fLogUID = ClsTools.ClsTools.CreateUniqueID(),
//                                fSendLogUID = maxLastOrder.fLogUID,
//                                fInvoiceId = model.InvoiceId,

//                            };
//                            npsContext.tErrorsOfLogs.AddOrUpdate(e => e.fLogUID, tErrorslog);
//                            await npsContext.SaveChangesAsync();
//                        }
//                        catch (Exception ex)
//                        {
//                            Console.WriteLine($"Error logging error code {error.Code}: {ex.Message}");
//                        }
//                    }
//                    var taxhistory = await
//                        npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower());
//                    if (taxhistory == null)
//                    {
//                        taxhistory = new tTaxHistoryStateParallel();
//                        taxhistory.ReferenceNumber = maxLastOrder.fReferencedID;
//                        taxhistory.fInvoiceID = model.InvoiceId;
//                    }
//                    {
//                        taxhistory.fListOfErrorCaption =
//                            string.Join('-', inquiry.Data.Error.Select(a => a.Message).ToArray());
//                        taxhistory.fSendState = 3;

//                        taxhistory.fListOfErrorCode =
//                            string.Join('-', inquiry.Data.Error.Select(a => a.Code).ToArray());
//                        npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);

//                    }
//                    await npsContext.SaveChangesAsync();
//                }
//                catch (Exception er)
//                {

//                    Console.WriteLine(er.Message);
//                }
//            }

//            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "FAILED", 0, inquiry.Uid.ToString(), model.fTaxHistoryReferenceNumber);

//            model.InqMessage = $"فاکتور به دلیل این خطاها ثبت نگردید : {errorMessages}";
//        }


//        private async Task HandlePendingAsync(GeneralInvoiceHeaderViewModel model, InquiryResultModel inquiry)
//        {
//            _context.TaxGovermentInvoiceFinalInsertUpdate(model.InvoiceId, "PENDING", 2, inquiry.Uid.ToString(), model.fTaxHistoryReferenceNumber);
//            using (var npsContext = new NPSGeneralContext())
//            {
//                var maxLastOrder = await npsContext.tSendInvoiceTaxTissLogs.
//                   Where(a => a.fInvoiceID.ToLower() == model.InvoiceId.ToLower()).OrderByDescending(a => a.fID)
//                   .FirstOrDefaultAsync();

//                if (maxLastOrder != null)
//                {
//                    maxLastOrder.fStatus = 2;
//                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(maxLastOrder);
//                    await npsContext.SaveChangesAsync();

//                }
//                else
//                {
//                    maxLastOrder = new tSendInvoiceTaxTissLog()
//                    {
//                        fInvoiceID = model.InvoiceId,
//                        fLastSendDate = DateTime.Now,
//                        fLogUID = ClsTools.ClsTools.CreateUniqueID(),
//                        fReferencedID = model.fTaxHistoryReferenceNumber,
//                        fStatus = 2,
//                        fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1))
//                    };
//                    npsContext.tSendInvoiceTaxTissLogs.AddOrUpdate(e => e.fID, maxLastOrder);
//                    await npsContext.SaveChangesAsync();
//                }
//                var taxhistory = await
//                   npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == model.InvoiceId);
//                if (taxhistory != null)
//                {
//                    taxhistory.fListOfErrorCaption = "درحال بررسی";
//                    taxhistory.fListOfErrorCode = "درحال بررسی";
//                    taxhistory.fSendState = 2;

//                    if (taxhistory.fLastSendDate != null && (DateTime.Now - taxhistory.fLastSendDate.Value).TotalDays > 4)
//                    {
//                        taxhistory.fListOfErrorCaption = "خطای نامشخص(پردازش بیش از 72 ساعت مودیان)";
//                        taxhistory.fListOfErrorCode = "خطای نامشخص(پردازش بیش از 72 ساعت مودیان)";
//                        taxhistory.fSendState = 3;
//                    }
//                }
//                else if (taxhistory == null)
//                {
//                    taxhistory = new tTaxHistoryStateParallel();
//                    taxhistory.ReferenceNumber = maxLastOrder.fReferencedID;
//                    taxhistory.fInvoiceID = model.InvoiceId;
//                    taxhistory.fSendState = 2;
//                }
//                {
//                    taxhistory.fListOfErrorCaption = "درحال بررسی";
//                    taxhistory.fListOfErrorCode = "درحال بررسی";
//                    npsContext.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, taxhistory);
//                }
//                await npsContext.SaveChangesAsync();
//            }
//            model.InqMessage = "صورتحساب هنوز اعتبارسنجی نشده و در صف بررسی میباشد";
//        }



//        public async Task<List<InquiryResultModel>> GetInvoiceStatusByDateRange(string fromdate = "14020220", string todate = "14040310")
//        {
//            var fromDT = PersianDateTime.Parse(fromdate).ToDateTime();
//            var todateDT = PersianDateTime.Parse(todate).ToDateTime();
//            InquiryByTimeRangeDto timeRangeDto = new InquiryByTimeRangeDto(fromDT, todateDT);
//            var inquiryResultModels = await TaxApiObject.InquiryByTimeAsync(timeRangeDto);
//            var pars = inquiryResultModels[0].ParseInquiryResult();
//            return inquiryResultModels ?? null;
//        }
//        public async Task GetNotFinishedLastRefrenceIdAsync()
//        {
//            var refId = await _context.GetNotFinishedLastRefrenceId();
//            if (!string.IsNullOrEmpty(refId))
//                await GetInvoiceStatusByRefrenceAsync(refId);
//        }
//        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search)
//        {
//            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//.LogInfoMessage($"::search::" +
//$"GetAllInvoice::" + search.StorID + "::search.DateFrom::" + search.DateFrom);

//            List<GeneralInvoiceHeaderViewModel> list = new List<GeneralInvoiceHeaderViewModel>();
//            try
//            {

//                //.StorID = BMain.SrvInfo.StoreId;
//                list = (await _context.GetAllInvoice(search));
//            }
//            catch (Exception ex)
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

//            }
//            return list;
//        }


//        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoiceByInqFromServer(SearchParameters search)
//        {
//            List<GeneralInvoiceHeaderViewModel> list = new List<GeneralInvoiceHeaderViewModel>();
//            try
//            {
//                list = (await _context.GetAllInvoice(search));
//                list = list.Where(a => !string.IsNullOrEmpty(a.ReferenceNumber) ||
//                !string.IsNullOrEmpty(a.fTaxHistoryReferenceNumber) ||
//                !string.IsNullOrEmpty(a.taxReferenceNumber)).ToList();
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//.LogInfoMessage($"::TissService::" +
//$"PostAsync:model::" + list.Count.ToString());
//                if (list != null && list.Any() && list.Count > 0)
//                    //list = await GetInvoiceStatusByRefrenceList(list);
//                    list = await GetInvoiceStatusByReferenceListEnhanced(list);
//            }
//            catch (Exception ex)
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
//.LogError($"::TissService::" +
//$"PostAsync:model " + ex.Message + ex.InnerException?.Message, ex);

//            }
//            return list;

//        }


//        public static SemaphoreSlim _SemaphoreSlimForLimitSend = new SemaphoreSlim(10);
//        public static int countofSend;
//        public static int countofAll;

//        public async Task<ResultModel> SendGroupInvoices(List<DTOInvoiceLimitParameter> invoices)
//        {
//            ResultModel result = new ResultModel();
//            result.Count = 0;
//            string numbers = " ";
//            ResultModel resultOfEachRow = new ResultModel();
//            try
//            {
//                List<InvoiceDto> invoiceList = new List<InvoiceDto>();
//                result.Message = "";
//                int countofSend = 0;
//                int countofAll = invoices.Count;
//                foreach (var item in invoices)
//                {
//                    try
//                    {
//                        await _SemaphoreSlimForLimitSend.WaitAsync(); // Await the semaphore

//                        await Task.Run(async () =>
//                        {
//                            try
//                            {
//                                await DoSendPertoPerAsync(item);
//                            }
//                            finally
//                            {
//                                _SemaphoreSlimForLimitSend.Release();
//                            }
//                        });
//                    }
//                    finally
//                    {
//                        _SemaphoreSlimForLimitSend.Release();
//                    }
//                }
//                result.Success = true;
//                result.Message = numbers;
//            }
//            catch (Exception ex)
//            {
//                result.Message = ex.Message;
//                result.Success = false;
//                result.Data = ex;

//            }
//            return result;
//        }
//        public async Task DoSendPertoPerAsync(DTOInvoiceLimitParameter item)
//        {
//            try
//            {
//                Interlocked.Increment(ref countofSend); // Safely increment count

//                var res = await SendInvoiceAsync(item, false);
//                if (res.Success)
//                {
//                    var message = @"ارسال فاکتور به شماره: " + res.TaxBreif.Header.inno +
//                                   " و مبلغ :" + res.TaxBreif.Header.tprdis +
//                                   " با موفقیت ارسال گردید. "
//                                   + "\r\n تعداد ارسالی: " + countofSend +
//                                   " از " + countofAll;
//                    await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);
//                }
//            }
//            catch (Exception ex)
//            {
//                // Handle or log the exception
//                // _logger.LogError("Error in DoSendPertoPer: " + ex.ToString());
//            }
//        }

//        public Task<ResultModel> GetInvoiceByID(string InvoiceID)
//        {
//            throw new NotImplementedException();
//        }


//        public async Task<bool> CheckConnectionAsync()
//        {
//            try
//            {
//                var SendInvoicesResult =
//              await TaxApiObject.GetFiscalInformationAsync(BMain.SrvInfo.MEMORY_ID);
//                if (SendInvoicesResult != null)
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }

//                return false;

//            }
//            catch (Exception er)
//            {
//                var message = er.Message;
//                await BMain._hubContext.Clients.All.SendAsync("SendToTaxEventsError", message);

//                return false;
//            }
//        }

//        public static SemaphoreSlim _SemaphoreSlimForLimitSendForGrouping
//            = new SemaphoreSlim(10);
//        public static int countofSendForGrouping;
//        public static int countofAllForGrouping;
//        public const int MaxBatchSizeInBytes = 2 * 1024 * 1024;
//        public const int MaxCurrentCount = 99;


//        public async Task<ResultModel> SendGroupInvoicesForGroupingAsync(List<DTOInvoiceLimitParameter> invoices)
//        {
//            ResultModel result = new ResultModel();
//            result.Count = 0;

//            try
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//                   LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//               + " SendGroupInvoicesForGroupingAsync ::");
//                countofSendForGrouping = 0;
//                countofAllForGrouping = invoices.Count;
//                var tasks = new List<Task>();

//                // Step 1: Pre-fetch and serialize all invoices once
//                Dictionary<Guid, InvoiceDto> invoiceDataMap = new Dictionary<Guid, InvoiceDto>();
//                using (var npsContext = new NPSGeneralContext())
//                {
//                    foreach (var item in invoices)
//                    {
//                        var resultodLastSent = await npsContext.TaxGovermentInvoiceInfoes.CountAsync(a => a.InvoiceId == item.InvoiceGuidID && a.Status == "Sent");
//                        var resultodfinalLastSent = await npsContext.TaxGovermentInvoiceFinals.CountAsync
//                            (a => a.InvoiceId == item.InvoiceID);
//                        var resultofSendTTLog = await npsContext.tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID == item.InvoiceID).ToListAsync();
//                        if (
//                            !resultofSendTTLog.Any(a =>/* a.fStatus == 2 ||*/ a.fStatus == 4) &&
//                            //!(resultodLastSent > 0/* && resultodLastSent > resultodfinalLastSent*/) &&
//                            (item.Status == 1 || item.Status == 3 || item.Status == 0)
//                            )
//                        {
//                            var serializedInvoice = await GetSerializedInvoiceAsync(item, false);
//                            if (serializedInvoice != null)
//                            {
//                                invoiceDataMap[item.InvoiceGuidID] = serializedInvoice;
//                            }
//                        }
//                        else
//                        {
//                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//  LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ " ExecCommandToObject ::item.Status == 1 || item.Status == 3 || item.Status == 0");
//                            var parallel = await npsContext.tTaxHistoryStateParallels.FirstOrDefaultAsync(a => a.fInvoiceID == item.InvoiceID);
//                            parallel.fSendState = 2;
//                            await npsContext.SaveChangesAsync();
//                            countofAllForGrouping = countofAllForGrouping - 1;
//                        }

//                    }
//                }
//                if (invoiceDataMap.Count == 0)
//                {
//                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//                   LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//               + " ExecCommandToObject ::آیتم های بارگذاری نمی شود");
//                    result.Data = null;
//                    result.Message = "آیتم ها بارگذاری نمی شود";
//                    result.Success = false;
//                    return result;
//                }
//                // Step 2: Create batches based on the size limit (2MB)
//                List<Guid> currentBatch = new List<Guid>();
//                int currentBatchSize = 0;
//                int currentSendCount = 0;
//                var options = new JsonSerializerOptions
//                {
//                    WriteIndented = false,
//                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
//                };
//                if (invoiceDataMap.Count == 0)
//                {
//                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//                   LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//               + " ExecCommandToObject ::آیتم های بارگذاری نمی شود");
//                    result.Data = null;
//                    result.Message = "آیتم ها بارگذاری نمی شود";
//                    result.Success = false;
//                    return result;
//                }
//                foreach (var id in invoiceDataMap.Keys)
//                {
//                    var data = JsonSerializer.Serialize(invoiceDataMap[id], options);
//                    data = data.Replace("\r\n", "").Replace("\n", "").Replace("\\", "").Replace("  ", "").Replace("   ", "");

//                    var invoiceSize = Encoding.UTF8.GetByteCount(data);

//                    // Check if adding this invoice exceeds the batch size limit
//                    if ((invoiceSize + currentBatchSize > MaxBatchSizeInBytes) ||
//                       currentSendCount > MaxCurrentCount)
//                    {
//                        // Send the current batch when size exceeds the limit
//                        tasks.Add(SendBatchAsync(currentBatch, invoiceDataMap));

//                        // Reset the batch
//                        currentBatch = new List<Guid>();
//                        currentBatchSize = 0;
//                        currentSendCount = 0;
//                    }

//                    // Add the current invoice to the batch
//                    currentBatch.Add(id);
//                    currentBatchSize += invoiceSize;
//                    currentSendCount += 1;
//                }

//                // Send any remaining batch
//                if (currentBatch.Any())
//                {
//                    tasks.Add(SendBatchAsync(currentBatch, invoiceDataMap));
//                }

//                // Await all tasks to complete
//                await Task.WhenAll(tasks);
//                result.Success = true;
//            }
//            catch (Exception ex)
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//                   LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//               + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
//                result.Data = ex;
//                result.Message = ex.Message;
//                result.Success = false;
//            }

//            return result;
//        }
//        // Helper method to serialize the invoice and return the data as a string
//        public async Task<InvoiceDto> GetSerializedInvoiceAsync(DTOInvoiceLimitParameter invoice, bool isCancel)
//        {

//            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ " ExecCommandToObject ::GetSerializedInvoiceAsync");
//            var Date1 = DateTime.Now;
//            var CurrentInvoice = await GetInvoice(invoice, isCancel);
//            var options = new JsonSerializerOptions
//            {
//                WriteIndented = false,
//                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
//            };
//            var data = JsonSerializer.Serialize(CurrentInvoice, options);
//            data = data.Replace("\r\n", "").Replace("\n", "").Replace("\\", "").Replace("  ", "").Replace("   ", "");
//            if (data.StartsWith("[") && data.EndsWith("]"))
//            {
//                data = data.Substring(1, data.Length - 2);


//            }

//            try
//            {

//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJson).
//                          LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//                      + "TaxJson:InvoiceID:" + invoice.InvoiceID + ":inno:" + CurrentInvoice.Header.inno + "::\r\n" + data);

//            }
//            catch
//            {
//            }

//            var timespan = (DateTime.Now - Date1).TotalSeconds;

//            if (CurrentInvoice == null) return null;
//            try
//            {

//                Interlocked.Increment(ref countofSendForGrouping); // Safely increment count
//                var message = @"فاکتور به شماره: " + CurrentInvoice.Header.inno +
//                                    " و مبلغ :" + CurrentInvoice.Header.tprdis +
//                                    " با موفقیت بارگذاری گردید. "
//                                    + "\r\n تعداد : " + countofSendForGrouping +
//                                    " از " + countofAllForGrouping +
//                                    " :: زمان صرف شده: " + timespan.ToString() + " ثانیه";

//                await BMain._hubContext.Clients.All.SendAsync("SendToTaxEvents", message);

//            }
//            catch (Exception ex)
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//             LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//         + "  ::" + ex.Message + "::" + ex.InnerException?.Message, ex);

//            }

//            return CurrentInvoice;
//        }

//        public async Task SendBatchAsync(List<Guid> batch, Dictionary<Guid, InvoiceDto> invoiceDataMap)
//        {
//            await _SemaphoreSlimForLimitSendForGrouping.WaitAsync();
//            try
//            {
//                var invoiceBatchData = new Dictionary<Guid, InvoiceDto>();
//                foreach (var id in batch)
//                {
//                    if (invoiceDataMap.TryGetValue(id, out var serializedInvoice))
//                    {
//                        invoiceBatchData[id] = serializedInvoice;
//                    }
//                }

//                if (invoiceBatchData.Any())
//                {
//                    await SendInvoiceBatchAsync(invoiceBatchData);
//                }
//            }
//            finally
//            {
//                _SemaphoreSlimForLimitSendForGrouping.Release();
//            }
//        }

//        public async Task<ResultModel> SendInvoiceBatchAsync(Dictionary<Guid, InvoiceDto> invoiceBatchData)
//        {
//            ResultModel result = new ResultModel();
//            try
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//    LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ " SendInvoiceBatchAsync ::LastLevelOfSend");
//                // Combine the serialized invoice data into a JSON array string
//                //var combinedData = "[" + string.Join(",", invoiceBatchData.Values) + "]";
//                var ids = invoiceBatchData.Keys.ToList();

//                List<InvoiceDto> currentSend = invoiceBatchData.Values.ToList();
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ " SendInvoiceBatchAsync ::" + currentSend.Count().ToString());
//                await Init();
//                var SendInvoicesResult =// await TaxApiService.Instance.TaxApis.SendInvoicesAsync(new List<InvoiceDto>() { nInvoice }, null);
//               await TaxApiObject.SendInvoicesAsync(currentSend);
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ " SendInvoiceBatchAsync ::TaxApiObject.SendInvoicesAsync");
//                if (SendInvoicesResult != null && SendInvoicesResult.Any())
//                {
//                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ "SendInvoicesResult != null && SendInvoicesResult.Any():::SendInvoicesResult.Count::" + SendInvoicesResult.Count());
//                    foreach (var item in SendInvoicesResult)
//                    {
//                        if (item != null)
//                        {
//                            var packetres = item;
//                            var objectofCurrent = invoiceBatchData.FirstOrDefault(a => a.Value.Header.taxid.ToLower() == item.TaxId.ToLower());
//                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ " packetres.ReferenceNumber::::::" + packetres.ReferenceNumber +
//"::packetres.TaxId::" + packetres.TaxId + "::packetres.Uid::" + packetres.Uid);


//                            _context.CreateSaveInvoiceResult(objectofCurrent.Key.ToString(), packetres.ReferenceNumber, "Sent",
//                                objectofCurrent.Value.Header.taxid, packetres.Uid);
//                            _context.TaxGovermentInvoiceFinalInsertUpdate(objectofCurrent.Key.ToString(),
//                                "Sent", objectofCurrent.Value.Header.taxid,
//                                packetres.ReferenceNumber, false);



//                            using (var context = new NPSGeneralContext())
//                            {
//                                var updatetax = await context.tTaxHistoryStateParallels.FirstOrDefaultAsync
//                                    (a => a.fInvoiceID.ToLower() == objectofCurrent.Key.ToString().ToLower());
//                                if (updatetax == null)
//                                {
//                                    updatetax = new tTaxHistoryStateParallel()
//                                    {
//                                        fInvoiceID = objectofCurrent.Key.ToString(),
//                                        fLastSendDate = DateTime.Now,
//                                        fListOfErrorCaption = "در حال بررسی",
//                                        fListOfErrorCode = "در حال بررسی",
//                                        fSendState = 2,
//                                        ReferenceNumber = packetres.ReferenceNumber,
//                                        fTaxID = objectofCurrent.Value.Header.taxid
//                                    };
//                                }


//                                updatetax.ReferenceNumber = packetres.ReferenceNumber;
//                                updatetax.fLastSendDate = DateTime.Now;
//                                updatetax.fSendState = 2;
//                                updatetax.fListOfErrorCaption = "در حال بررسی";
//                                updatetax.fListOfErrorCode = "در حال بررسی";
//                                updatetax.fTaxID = objectofCurrent.Value.Header.taxid;
//                                context.tTaxHistoryStateParallels.AddOrUpdate(a => a.fID, updatetax);
//                                await context.SaveChangesAsync();
//                                var maxLastOrder = await context.
//                  tSendInvoiceTaxTissLogs.Where(a => a.fInvoiceID.ToLower() == objectofCurrent.Key.ToString().ToLower()).
//                  OrderByDescending(a => a.fID).FirstOrDefaultAsync();

//                                tSendInvoiceTaxTissLog tSendInvoice = new tSendInvoiceTaxTissLog()
//                                {
//                                    fInvoiceID = objectofCurrent.Key.ToString(),
//                                    fLastSendDate = DateTime.Now,
//                                    fLogUID = ClsTools.ClsTools.CreateUniqueID(),
//                                    fReferencedID = packetres.ReferenceNumber,
//                                    fStatus = 2,
//                                    fOrder = ((short)((maxLastOrder == null ? 0 : maxLastOrder.fOrder) + 1)),
//                                    fTaxID = objectofCurrent.Value.Header.taxid
//                                };
//                                context.tSendInvoiceTaxTissLogs.AddOrUpdate(tSendInvoice);

//                                await context.SaveChangesAsync();
//                                _ = BMain.ScheduleOperationAsync(objectofCurrent.Key.ToString());

//                            }

//                            // in this point ScheduleOperationAsync must run

//                            result.Success = true;
//                            result.Data = packetres;
//                            result.Message = "ثبت با موفقیت انجام شد";
//                        }
//                        else
//                        {
//                            result.Success = false;
//                            result.Data = null;
//                            result.Message = "ثبت انجام نشد";
//                        }
//                    }

//                }
//                else
//                {
//                    result.Success = false;
//                    result.Message = "ارتباط با سامانه مودیان برقرار نشد";
//                    result.Data = SendInvoicesResult;

//                }
//            }
//            catch (Exception ex)
//            {
//                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
//LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
//+ " SendInvoiceBatchAsync ::" + ex.Message, ex);
//                result.Success = false;
//                result.Data = ex;
//                result.Message = ex.Message;
//            }

//            return result;
//        }


//    }
//}