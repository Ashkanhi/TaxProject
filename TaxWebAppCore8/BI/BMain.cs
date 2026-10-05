using Microsoft.Extensions.Logging;
using ClsTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
//using Org.BouncyCastle.Crypto;
//using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using TaxCollectData.Library.Models;
using TaxCollectData.Library.Properties;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using TaxServiceAndApp.Models.TISSDTO;
using Microsoft.AspNetCore.SignalR;
using TaxWebAppCore8.BI.MessagingService;
using Microsoft.EntityFrameworkCore;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;
using TaxWebAppCore8.Models.SharedDTO;
using Org.BouncyCastle.Crypto;
using System.Reflection;

namespace TaxServiceAndApp.BI
{
    public class BMain
    {
        [DataMember] public static TissServise _tissServise;//{ get; set; }
        [DataMember] public static TaxService _taxService;//{ get; set; }
        [DataMember] public static RimaService _rimaService;//{ get; set; }
        [DataMember] public static RimaServiceCalculateMethod _rimaCalcService;//{ get; set; }
        public static IHubContext<RequestsHub> _hubContext;
        public static DTOServerInfo SrvInfo;
        public static string ServerResourcesPath;
        public static IContext _Context;
        public BMain()
        {
            SrvInfo = ClsTools.ClsTools.DeSerializeObject<DTOServerInfo>
                (BMain.ServerResourcesPath + "/CustomerID/CustomerInformation.xml");
            var databaseType = SrvInfo.DataBaseType ?? "baran";
            _Context = DBBridge.getInstance(databaseType.ToLower());
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                _taxService = TaxService.getInstance(_Context);
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                _rimaService = RimaService.getInstance(_Context);
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                _rimaCalcService = RimaServiceCalculateMethod.getInstance(_Context);
            else _tissServise = TissServise.getInstance(_Context);
        }

        public static async Task<List<GeneralInvoiceHeaderViewModel>>
            GetAllInvoiceAsync(SearchParameters search)
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return (await _taxService.GetAllInvoice(search));
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return (await _rimaService.GetAllInvoice(search));
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return (await _rimaCalcService.GetAllInvoice(search));
            else return (await _tissServise.GetAllInvoice(search));
        }
        public static async Task<List<GeneralInvoiceHeaderViewModel>> 
            GetAllInvoiceWithInqFromServerAsync(SearchParameters search, string? operationId = null)
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return (await _taxService.GetAllInvoiceByInqFromServer(search));
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return (await _rimaService.GetAllInvoiceByInqFromServer(search));
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return (await _rimaCalcService.GetAllInvoiceByInqFromServer(search, operationId));
            else return (await _tissServise.GetAllInvoiceByInqFromServer(search));
        }

        public static async Task<GeneralInvoiceViewModel> GetInvoiceAsync(string InvoiceID)
        {
            return await BMain._Context.GetInvoice(InvoiceID);
        }
        public static List<DTO_UV_TissInfoForShow> GetAllTissInfoItems
            (Guid invoiceID, int storeId, int workstaionID)
        {
            return BMain._Context.GetAllTissInfoItemsFromDetailInfo
                (invoiceID, storeId, workstaionID);
        }

        public static async Task<List<InquiryResultModel>> 
            GetInvoiceStatusByDateRangeAsync(string fromdate, string todate)
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return (await _taxService.GetInvoiceStatusByDateRange(fromdate, todate));
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return (await _rimaService.GetInvoiceStatusByDateRange(fromdate, todate));
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return (await _rimaCalcService.GetInvoiceStatusByDateRange(fromdate, todate));

            else return (await _tissServise.GetInvoiceStatusByDateRange(fromdate, todate));
        }

        public static async Task<ResultModel> GetInvoiceStatusByRefrenceAsync(string invoiceID)
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return (await _taxService.GetInvoiceStatusByRefrenceAsync(invoiceID));
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return (await _rimaService.GetInvoiceStatusByRefrenceAsync(invoiceID));
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return (await _rimaCalcService.GetInvoiceStatusByRefrenceAsync(invoiceID));
            else return (await _tissServise.GetInvoiceStatusByRefrenceAsync(invoiceID));
        }

        public static async Task GetNotFinishedLastRefrenceIdAsync()
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                await _taxService.GetNotFinishedLastRefrenceIdAsync();
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                await _rimaService.GetNotFinishedLastRefrenceIdAsync();
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                await _rimaCalcService.GetNotFinishedLastRefrenceIdAsync();

            else await _tissServise.GetNotFinishedLastRefrenceIdAsync();
        }

        public static async Task InitAsync()
        {
            try
            {
                if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                    await _taxService.Init();
                else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                    await _rimaService.Init();
                else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                    await _rimaCalcService.Init();
                else
                {
                    await _tissServise.Init();
                }
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger
                    (ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                  LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
              + " InitAsync ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
            }
        }
        public static async Task<ResultModel> SendGroupInvoicesAsync(List<DTOInvoiceLimitParameter> Ids, string? operationId = null)
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return await _taxService.SendGroupInvoicesForGroupingAsync(Ids);
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return await _rimaService.SendGroupInvoicesForGroupingAsync(Ids);
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return await _rimaCalcService.SendGroupInvoicesForGroupingAsync(Ids, operationId);
            else return await _tissServise.SendGroupInvoicesForGroupingAsync(Ids);
        }

        public static async Task<ResultModel> SendInvoiceAsync(DTOInvoiceLimitParameter invoice, bool isCancel = false)
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return await _taxService.SendGroupInvoicesForGroupingAsync
                    (new List<DTOInvoiceLimitParameter> { invoice });
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return await _rimaService.SendGroupInvoicesForGroupingAsync
                    (new List<DTOInvoiceLimitParameter> { invoice });
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return await _rimaCalcService.SendGroupInvoicesForGroupingAsync
                    (new List<DTOInvoiceLimitParameter> { invoice });

            else return await _tissServise.SendGroupInvoicesForGroupingAsync
                    (new List<DTOInvoiceLimitParameter> { invoice });
        }

        public static async Task ScheduleOperationAsync(string operationKey, int milisecond = -1)
        {
            // Wait for one hour (3600000 milliseconds)
            if (milisecond == -1)
                await Task.Delay(TimeSpan.FromMinutes(100));
            else await Task.Delay(1800000);
            // Perform the operation on the list
            PerformOperation(operationKey);
        }

        static async void PerformOperation(string operations)
        {
            await GetInvoiceStatusByRefrenceAsync(operations);
        }
        public static async Task<bool> CheckConnectionAsync()
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return await _taxService.CheckConnectionAsync();
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return await _rimaService.CheckConnectionAsync();
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return await _rimaCalcService.CheckConnectionAsync();
            else return await _tissServise.CheckConnectionAsync();
        }
        public static List<DTOStore> GetStore()
        {
            return _Context.GetStore();
        }
        public static List<tTisTaxErrorType> GetErrorType()
        {
            using (var NPScontext = new NPSGeneralContext())
            {
                NPScontext.Database.Initialize(force: true);
                return NPScontext.tTisTaxErrorTypes.ToList();
            }
        }

        public static async Task<ResultModel> GetInvoiceByIDAsync(string invoiceId)
        {
            await InitAsync();
            if (SrvInfo.ServiceType.ToLower() == "Tax".ToLower())
                return await _taxService.GetInvoiceByID(invoiceId);
            else if (SrvInfo.ServiceType.ToLower() == "Rima".ToLower())
                return await _rimaService.GetInvoiceByID(invoiceId);
            else if (SrvInfo.ServiceType.ToLower() == "RimaCalculate".ToLower())
                return await _rimaCalcService.GetInvoiceByID(invoiceId);
            else return await _tissServise.GetInvoiceByID(invoiceId);
        }
    }
}
