using Microsoft.Extensions.Logging;
using NPSTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using TaxCollectData.Library.Abstraction.Clients;
using TaxCollectData.Library.Abstraction.Cryptography;
using TaxCollectData.Library.Clients;
using TaxCollectData.Library.Factories;
using TaxCollectData.Library.Models;
using TaxCollectData.Library.Properties;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;

namespace TaxServiceAndApp.BI
{
    public class BMain
    {
        [DataMember] public static TissServise _tissServise;//{ get; set; }
        [DataMember] public static TaxService _taxService;//{ get; set; }

        public static DTOServerInfo SrvInfo;

        public static string ServerResourcesPath;
        public static IContext _Context;
        public BMain()
        {
            SrvInfo = ClsTools.DeSerializeObject<DTOServerInfo>(BMain.ServerResourcesPath + "/CustomerID/CustomerInformation.xml");
            var databaseType = SrvInfo.DataBaseType ?? "baran";
            switch (databaseType.ToLower())
            {
                case "baran":
                    _Context = BaranDB.getInstance();
                    break;

                case "argham":
                    _Context = ArghamDB.getInstance();
                    break;

                default:
                    _Context = ArghamDB.getInstance();
                    break;
            }

            //var serviceType = SrvInfo.ServiceType ?? "Tax";
            //switch (serviceType)
            //{
            //    case "Tax":
            //        Logger<TaxService> taxLoger = new Logger<TaxService>(new LoggerFactory());
            //        ServiceName = TaxService.getInstance(_Context);
            //        break;
            //    case "Tis":
            //        Logger<TissServise> tisLoger = new Logger<TissServise>(new LoggerFactory());
            //        ServiceName = TissServise.getInstance(_Context);
            //        break;
            //    default:
            //        Logger<TaxService> defLoger = new Logger<TaxService>(new LoggerFactory());
            //        ServiceName = TaxService.getInstance(_Context);
            //        break;
            //}
        }

        public static async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoiceAsync(SearchParameters search)
        {
            await Init();
            if (SrvInfo.ServiceType == "Tax")
                return (await _taxService.GetAllInvoice(search));
            else return (await _tissServise.GetAllInvoice(search));
        }

        public static async Task<GeneralInvoiceViewModel> GetInvoiceAsync(string InvoiceID)
        {

            return await BMain._Context.GetInvoice(InvoiceID);
            //if (SrvInfo.ServiceType == "Tax")
            //    return await _taxService.GetInvoice(InvoiceID);
            //else (await _tissServise.GetInvoice(InvoiceID));
        }

        public static async Task<List<InquiryResultModel>> GetInvoiceStatusByDateRangeAsync(string fromdate, string todate)
        {
            await Init();
            if (SrvInfo.ServiceType == "Tax")
                return (await _taxService.GetInvoiceStatusByDateRange(fromdate, todate));
            else return (await _tissServise.GetInvoiceStatusByDateRange(fromdate, todate));
        }

        public static async Task<ResultModel> GetInvoiceStatusByRefrenceAsync(string refId)
        {
            await Init();
            if (SrvInfo.ServiceType == "Tax")
                return (await _taxService.GetInvoiceStatusByRefrenceAsync(refId));
            else return (await _tissServise.GetInvoiceStatusByRefrenceAsync(refId));
        }

        public static async Task GetNotFinishedLastRefrenceIdAsync()
        {
            await Init();
            if (SrvInfo.ServiceType == "Tax")
                await _taxService.GetNotFinishedLastRefrenceIdAsync();
            else await _tissServise.GetNotFinishedLastRefrenceIdAsync();
        }

        public static async Task Init()
        {

            if (SrvInfo.ServiceType == "Tax")
                await _taxService.Init();
            else await _tissServise.Init();
        }

        public static async Task<ResultModel> SendGroupInvoicesAsync(List<Guid> Ids)
        {
            await Init();
            if (SrvInfo.ServiceType == "Tax")
                return await _taxService.SendGroupInvoices(Ids);
            else return await _tissServise.SendGroupInvoices(Ids);
        }

        public static async Task<ResultModel> SendInvoiceAsync(string invoiceId, bool isCancel = false)
        {
            await Init();
            if (SrvInfo.ServiceType == "Tax")
                return await _taxService.SendInvoiceAsync(invoiceId, isCancel);
            else return await _tissServise.SendInvoiceAsync(invoiceId, isCancel);
        }

        public static async Task<ResultModel> SendInvoiceAsync(string invoiceId, object invoice, bool isCancel = false)
        {
           await Init();
            return await ServiceName.SendInvoiceAsync(invoiceId, invoice, isCancel);
        }
        public static async Task<ResultModel> GetInvoiceByID(string invoiceId)
        {
            Init();
            if (SrvInfo.ServiceType != "Tax")
                return await ServiceName.GetInvoiceByID(invoiceId);
            return null;
        }
    }
}