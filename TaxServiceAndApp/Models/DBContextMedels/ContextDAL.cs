using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TaxCollectData.Library.Dto;
using TaxServiceAndApp.Models.DBContextMedels.Interface;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;

namespace TaxServiceAndApp.Models
{
    public class ContextDAL : IContext
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContextDAL> _logger;
        private IContext _dataBase;
        private readonly string _connectionString;
        private string cStoreId = "0";


        private string MakeConnectionString()
        {
            var datasource = _configuration["ConnectionStrings:DataBaseServer"] ?? "";
            var dbname = _configuration["ConnectionStrings:DbName"] ?? "";
            var userid = _configuration["ConnectionStrings:UserId"] ?? "";
            var userpass = _configuration["ConnectionStrings:UserPass"] ?? "";
            cStoreId = _configuration["StoreId"] ?? "0";

            if (string.IsNullOrEmpty(datasource) || string.IsNullOrEmpty(dbname) || string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(userpass))
                return "";
            string con = string.Format("Server={0};Database={1};user id ={2};password={3}; TrustServerCertificate=True;", datasource, dbname, userid, userpass);
            AppSettingClass.DBConn = con;
            return con;
        }
        public ContextDAL(IConfiguration configuration, ILogger<ContextDAL> logger)
        {
            this._configuration = configuration;
            this._connectionString = MakeConnectionString();
            _logger = logger;
            //Strategy Design patern
            //چون در این پروژه نوع دیتابیس در تنظیمات هستش پس استراتژی رو اینجوری پیاده کرده ایم
            var databaseType = _configuration["DataBaseType"] ?? "Argham";
            switch (databaseType.ToLower())
            {
                case "baran":
                    _dataBase = BaranDB.getInstance();
                    break;
                case "argham":
                    _dataBase = ArghamDB.getInstance();

                    break;
                default:
                    _dataBase = ArghamDB.getInstance();
                    break;
            }
            IService service;
            var serviceType = _configuration["ServiceType"] ?? "Tax";
            switch (serviceType)
            {
                case "Tax":
                    service = TaxService.getInstance(this, taxLoger);
                    break;
                case "Tis":
                    Logger<TissServise> tisLoger = new Logger<TissServise>(new LoggerFactory());
                    service = TissServise.getInstance(this, tisLoger);
                    break;
                default:
                    Logger<TaxService> defLoger = new Logger<TaxService>(new LoggerFactory());
                    service = TaxService.getInstance(this, defLoger);
                    break;
            }
            AppSettingClass.ServiceName = service;
        }

        #region Logs
        public void LogConnectionSuccess() => _logger.LogInformation("Connection Opened");
        private void LogSqlCommand(string command) => _logger.LogInformation("Sql Command\r\n" + command);
        private void GetTaxSettingLog() => _logger.LogInformation("Get TaxSetting From DB");
        private void SetTaxSettingLog() => _logger.LogInformation("Set TaxSetting In DB");
        private void SaveInvoiceForSendLog() => _logger.LogInformation("Save LastInvoiceForSend In DB");
        private void SaveInvoiceFinalStatusLog() => _logger.LogInformation("Save Final Status In DB");
        private void ChangeToDoneLog(string refId) => _logger.LogInformation("Change To Done " + refId);
        private void SaveInvoiceInquiryLog() => _logger.LogInformation("Save Invoice Inquiry In DB");
        #endregion
        #region DB Methods
        public void ExecCommand(string command)
        {
            LogSqlCommand(command);
            _dataBase.ExecCommand(command);
        }
        public string ExecReader(string command, string returnfield)
        {
            LogSqlCommand(command);
            return _dataBase.ExecReader(command, returnfield);

        }
        public List<T> ExecCommandToObject<T>(string command) where T : new()
        {

            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(_connectionString))
                return new List<T>();
            LogSqlCommand(command);
            return _dataBase.ExecCommandToObject<T>(command);


        }
        #endregion
        #region Methods
        //public TaxSetting GetAllSetting(string StoreId)
        //{
        //    return _dataBase.GetAllSetting(cStoreId);
        //}
        //public void SaveSetting(TaxSetting taxSetting)
        //{
        //    _dataBase.SaveSetting(taxSetting);

        //}
        public InvoiceDto GetLastInvoiceForSend(short invoicetype, string invoiceId, bool isForTis)
        {
            return _dataBase.GetLastInvoiceForSend(invoicetype, invoiceId, isForTis);

        }
        public string GetNotFinishedLastRefrenceId()
        {
            return _dataBase.GetNotFinishedLastRefrenceId();
        }
        public void CreateSaveInvoiceResult(string invoiceId, string refId, string status, string uniqueId, string uId)
        {
            _dataBase.CreateSaveInvoiceResult(invoiceId, refId, status, uniqueId, uId);
        }
        public void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel)
        {
            _dataBase.TaxGovermentInvoiceFinalInsertUpdate(invoiceId, lastStatus, TaxId, lastRefId, IsCancel);
        }
        public void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, short Done, string TaxId, string lastRefId)
        {
            _dataBase.TaxGovermentInvoiceFinalInsertUpdate(invoiceId, lastStatus, Done, TaxId, lastRefId);
        }
        public void SaveInvoiceInquiryLog(string refId, string status, string uId, string data, string packetType)
        {
            _dataBase.SaveInvoiceInquiryLog(refId, status, uId, data, packetType);
        }
        public void ChangeToDoneInvoice(string refId)
        {
            _dataBase.ChangeToDoneInvoice(refId);
        }
        public void ChangeToNotDoneInvoice(string refId)
        {
            _dataBase.ChangeToNotDoneInvoice(refId);
        }

        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search)
        {
            var result = await _dataBase.GetAllInvoice(search);
            return result;
        }

        public async Task<GeneralInvoiceViewModel> GetInvoice(string InvoiceID)
        {
            return await _dataBase.GetInvoice(InvoiceID);

        }
        public List<Invoice> GetGroupOfInvoiceForSend(short invoicetype, string invoiceId, bool isForTis)
        {
            return _dataBase.GetGroupOfInvoiceForSend(invoicetype, invoiceId, isForTis);
        }
        #endregion
    }
}