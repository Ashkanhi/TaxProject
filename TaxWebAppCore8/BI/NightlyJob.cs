using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Quartz;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.TaxDTO;

namespace TaxWebAppCore8.BI
{
    public class NightlyJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.JobSchedule)
.LogInfoMessage($"::Execute::");




                var searchFilter = new TaxServiceAndApp.Models.SharedDTO.SearchParameters();
                searchFilter.Status = 2;



                var list = await BMain.GetAllInvoiceWithInqFromServerAsync(searchFilter);
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.JobSchedule)
.LogInfoMessage($"::Execute::list.Count::" + list.Count());
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogError($"NightlyJob::" + ex.Message + ex.InnerException?.Message, ex);

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.BalandingLog)
.LogError($"NightlyJob::" + ex.Message + ex.InnerException?.Message, ex);

            }
        }
    }
}
