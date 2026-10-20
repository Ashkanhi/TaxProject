using System.Data.SqlClient;
using System.Data;

using System.Collections.Generic;
using System.Reflection;
//using Org.BouncyCastle.Asn1.Ocsp;
using TaxServiceAndApp.BI;
using ClsTools;
using System;
using System.Threading.Tasks;
using TaxServiceAndApp.Models.TaxDTO;
using System.Linq;
using TaxServiceAndApp.Models.SharedDTO;
using TaxCollectData.Library.Dto;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System.Collections.Concurrent;
using TaxServiceAndApp.Models.TISSDTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Xml.Linq;
using TaxWebAppCore8.Models.DBContextMedels;
using System.Data.Entity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using NPSDTOModels.DBDTO.Request;
using System.Data.Entity.Migrations;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;
using TaxCollectData.Library.Models;
using TaxWebAppCore8.Models.SharedDTO;
using TaxWebAppCore8.Models.TaxDTO;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Newtonsoft.Json;
using Humanizer;

namespace TaxServiceAndApp.Models
{
    public class DBBridge : IContext
    {
        private static DBBridge _BridgeDB = null;
        private readonly string _connectionString;
        private readonly string _DBName;
        private DBBridge(string DBName)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogInfoMessage($" BaranDB :" +
                $"DBBridge() :DBName:: " + DBName
                    );
            _connectionString = BMain.SrvInfo.DBConn;
            _DBName = DBName;
        }
        public Invoice GetLastInvoiceForSend(short invoicetype,
            DTOInvoiceLimitParameter invoice, int isForTis)
        {
            try
            {

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.BalandingLog)
.LogInfoMessage($"::TaxService::" +
  $"GetLastInvoiceForSend::" + "::invoiceID::" + invoice.InvoiceID);

                if (isForTis == 0)
                {
                    ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.BalandingLog)
.LogInfoMessage($"::TaxService::" +
  $"GetLastInvoiceForSend:isForTis == 0:" + "::invoiceID::" + invoice.InvoiceID);
                    //var res = ExecCommandToObject<TissInvoiceHeader>
                    //    (string.Format(Consts.GetLastInvoiceHeaderQuery,
                    //    invoicetype, invoiceId, 1)).FirstOrDefault();
                    // if (res != null)
                    {
                        //var isReturn = res.ins == 4 ? 1 : 0;
                        var rows = ExecCommandToObject<DTO_UV_TissInfo>(string.Format
                            (Consts.Usp_TaxCartable_DetailInfo,
                            invoice.InvoiceID, invoice.StoreID, invoice.BookerWorkstationID));
                        var header = ObjectMapper.TargetObject<TissInvoiceHeader>
                            (rows.Where(a => a.tprdis != 0 && a.tprdis != null).FirstOrDefault());



                        return new Invoice()
                        {
                            TissHeader = header,
                            TisRows = rows
                        };
                    }
                    return null;
                }
                else //if (isForTis == 1)
                {
                    //var res = ExecCommandToObject<TaxInvoiceHeaderSysDTO>
                    //    (string.Format(Consts.GetLastInvoiceHeaderQuery,
                    //    invoicetype, invoice.InvoiceID, 0)).FirstOrDefault();
                    //if (res != null)
                    {
                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.BalandingLog)
.LogInfoMessage($"::TaxService::" +
  $"GetLastInvoiceForSend:isForTis == 2:" + "::invoiceID::" + invoice.InvoiceID);
                        //var rows = ExecCommandToObject<BodyItemDto>(string.Format(Consts.GetLastInvoiceDetailQuery, invoiceId, isReturn, 0));
                        var rows = ExecCommandToObject<BodyItemDtoExtend>(string.Format(Consts.Usp_TaxCartable_DetailInfo,
                            invoice.InvoiceID, invoice.StoreID, invoice.BookerWorkstationID),
                            // تبدیل جدید فقط برای امید فعال است؛ سایر سرویس‌ها همان مپ قبلی را دارند.
                            convertNumericStringsForOmid: string.Equals(
                                BMain.SrvInfo?.ServiceType?.Trim(), "Omid",
                                StringComparison.OrdinalIgnoreCase));


                        var res = ObjectMapper.TargetObject<TaxInvoiceHeaderSysDTO>
                            (rows.Where(a => a.prdis != 0 && a.prdis != null).FirstOrDefault());

                        ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.BalandingLog)
      .LogInfoMessage($"::TaxService::" +
          $"CreateUniqeTaxID::irTax::" + res.Irtaxid + "::IrTaxis::" + res.Irtaxid + "::invoiceID::" + invoice.InvoiceID);

                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = false,
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            };

                            var data = System.Text.Json.JsonSerializer.Serialize(rows, options);

                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJson).
                                      LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                  + "rows::InvoiceID:" + invoice.InvoiceID + "::\r\n" + data);

                        }
                        catch
                        {
                        }
                        var json = JsonConvert.SerializeObject(rows);
                        var items = JsonConvert.DeserializeObject<List<BodyItemDto>>(json);
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = false,
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            };

                            var data = System.Text.Json.JsonSerializer.Serialize(items, options);

                            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxJson).
                                      LogInfoMessage($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                                  + "items:::InvoiceID:" + invoice.InvoiceID + "::\r\n" + data);

                        }
                        catch
                        {
                        }
                        //var isReturn = res.Ins == 4 ? 1 : 0;
                        return new Invoice()
                        {
                            Header = res,
                            Rows = items// rows.Select(a => a as BodyItemDto).ToList()
                        };
                    }
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }
        }
        public List<DTO_UV_TissInfoForShow> GetAllTissInfoItemsFromDetailInfo(Guid invoiceID, int storeId, int workstaionID)
        {
            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                  .LogInfoMessage($" BaranDB :" +
                      $"GetAllTissInfoItemsFromDetailInfo() :invoiceID:: " + invoiceID
                          );
                DateTime now1 = DateTime.Now;
                var items = ExecCommandToObject<DTO_UV_TissInfoForShow>
                    (string.Format(Consts.Usp_TaxCartable_DetailInfo,
                            invoiceID, storeId, workstaionID));

                var timeSpan = (DateTime.Now - now1).TotalSeconds;
                return items;
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
               + "ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }
        }

        public static DBBridge getInstance(string dbName)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
         .LogInfoMessage($" BaranDB :" +
             $"getInstance() :DBName:: " + dbName
                 );

            if (_BridgeDB == null)
                _BridgeDB = new DBBridge(dbName);
            return _BridgeDB;
        }
        public void ExecCommand(string command)
        {

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
    .LogInfoMessage($" BaranDB :" +
        $"ExecCommand() :command:: " + command
            );
            if (string.IsNullOrEmpty(_connectionString))
                return;
            try
            {


                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand Sqlcmd = new SqlCommand(command, con);
                    Sqlcmd.CommandType = CommandType.Text;
                    Sqlcmd.ExecuteNonQuery();
                    Sqlcmd.Dispose();
                }
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($" BaranDB :" +
                $"ExecCommand_OUT : " + command
                    );

            }
            catch (Exception ex)
            {


                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                 + " ExecCommand ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
            }
            finally
            {

            }
        }
        public string ExecReader(string command, string returnfield)
        {
            var res = "";

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogInfoMessage($" BaranDB :" +
                $"ExecReader_IN : " + command + "    " + returnfield
                    );

            if (string.IsNullOrEmpty(_connectionString))
                return "";
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(command, con);
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            res = reader[returnfield].ToString();
                        }
                    }

                }
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($" BaranDB :" +
                $"ExecReader_OUT : " + _connectionString
        );
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " ExecCommand ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
            }
            finally
            {

            }
            return res;
        }
        public List<T> ExecCommandToObject<T>(string command) where T : new()
        {
            return ExecCommandToObject<T>(command, convertNumericStringsForOmid: false);
        }

        private List<T> ExecCommandToObject<T>(string command, bool convertNumericStringsForOmid) where T : new()
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
             .LogInfoMessage($" BaranDB : ExecCommandToObject : " + command);

            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(_connectionString))
                return new List<T>();

            List<T> lst = new List<T>();
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand Sqlcmd = new SqlCommand(command, con);
                    Sqlcmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(command, con))
                    {
                        da.Fill(dt);
                        da.SelectCommand.Dispose();
                    }
                }

                // --- راه حل اصلاح شده ---
                // پردازش به صورت خطی انجام می‌شود تا ترتیبی که از SQL می‌آید دقیقاً حفظ شود
                lst = dt.AsEnumerable().Select(r => r.CreateItemFromRow<T>(convertNumericStringsForOmid)).ToList();
                // ------------------------
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                    LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }

            return lst;
        }
        public List<T> ExecCommandToObjectLast<T>(string command) where T : new()
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
             .LogInfoMessage($" BaranDB :" +
             $"ExecCommandToObject : " + command
             );

            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(_connectionString))
                return new List<T>();


            List<T> lst = new List<T>();
            DataTable dt = new DataTable();
            try
            {

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand Sqlcmd = new SqlCommand(command, con);
                    Sqlcmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(command, con))
                    {
                        da.Fill(dt);
                        da.SelectCommand.Dispose();
                    }

                }
                
                var lst2 = new ConcurrentBag<T>();

                var tasks = dt.AsEnumerable().Select(r => Task.Run(() =>
                {

                    var item = r.CreateItemFromRow<T>();
                    lst2.Add(item); 
                })).ToList();

                Task.WhenAll(tasks).Wait();

                lst = lst2.ToList();
            }
            catch (Exception ex)
            {

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                    LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }

            finally
            {
            }
            return lst;

        }

        public void ChangeToDoneInvoice(string refId)
        {
            ExecCommand(string.Format(Consts.ChangeToDoneInvoiceQuery, refId));

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogInfoMessage($" BaranDB :" +
                $"ChangeToDoneInvoice : " + (string.Format(Consts.ChangeToDoneInvoiceQuery, refId))
                        );

        }
        public void ChangeToNotDoneInvoice(string refId)
        {
            ExecCommand(string.Format(Consts.ChangeToNotDoneInvoiceQuery, refId));

            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
        .LogInfoMessage($" BaranDB :" +
            $"ChangeToDoneInvoice : " + (string.Format(Consts.ChangeToNotDoneInvoiceQuery, refId))
            );


        }


        public void CreateSaveInvoiceResult(string invoiceId, string refId, string status, string uniqueId, string uId)
        {
            try
            {
                ExecCommand(string.Format(Consts.TaxGovermentSaveInvoiceInsertQuery, invoiceId, refId, status, uniqueId, uId));

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($" BaranDB :" +
                    $"CreateSaveInvoiceResult : " + (string.Format(Consts.TaxGovermentSaveInvoiceInsertQuery, invoiceId, refId, status, uniqueId, uId))
                    );

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " CreateSaveInvoiceResult : " + ex.Message + "::" + ex?.InnerException?.Message, ex);

            }
        }

        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search)
        {
            List<GeneralInvoiceHeaderViewModel> list = new List<GeneralInvoiceHeaderViewModel>();
            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogInfoMessage($"::search::" +
$"search:model::search.StorID::" + search.StorID + "::search.DateFrom::" + search.DateFrom);

                using (NPSGeneralContext nPSGeneral = new NPSGeneralContext())
                {

                    var vwTaxGoverment = nPSGeneral.vw_TaxGovermentInvoiceHeaderForShow.Where(a => a.fID > 0);
                    if (search.DateFrom != null && search.DateTo != null)
                    {
                        var dateTo = search.DateTo.Value.AddHours(23).AddMinutes(59).AddSeconds(59)
    .AddMilliseconds(500);
                        vwTaxGoverment = vwTaxGoverment.Where(a => a.InvoiceDate >= search.DateFrom
                        && a.InvoiceDate <= dateTo);
                    }

                    if (search.Status != null && search.Status != -1)
                        vwTaxGoverment = vwTaxGoverment.Where(a => a.fSendState == search.Status || (search.Status == 1 && a.fSendState == null));


                    if (search.InvoiceID != null && search.InvoiceID != default(Guid))
                        vwTaxGoverment = vwTaxGoverment.Where(a => a.InvoiceId == search.InvoiceID);
                    if (search.Status == 3 && search.ErrorTypeID != null && search.ErrorTypeID != -1)
                    {
                        var errortext = await nPSGeneral.tTisTaxErrorTypes.
                            FirstOrDefaultAsync(a => a.fID == search.ErrorTypeID);
                        vwTaxGoverment = vwTaxGoverment.Where(a => a.fListOfErrorCaption.Contains
                        (errortext.fCaption));
                    }
                    if (search.StorIDs != null && search.StorIDs.Any(id => id != -1))
                    {
                        vwTaxGoverment = vwTaxGoverment
                            .Where(a => a.StoreID.HasValue && search.StorIDs.Contains(a.StoreID.Value));
                    }
                    else if (search.StorID != null && search.StorID != -1)
                    {
                        vwTaxGoverment = vwTaxGoverment.Where(a => a.StoreID == search.StorID);
                    }

                    var tlist = await vwTaxGoverment.ToListAsync();
                    foreach (var item in tlist)
                    {
                        list.Add(ObjectMapper.TargetObject<GeneralInvoiceHeaderViewModel>(item));
                    }

                }

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                   LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
               + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
            }

            return list.OrderBy(a => a.fSendState).ThenBy(a => a.InvoiceNumber).ToList();
        }



        public async Task LogErrorsAsync(NPSGeneralContext npsContext, List<errorDetail> errors)
        {
            foreach (var error in errors)
            {
                try
                {
                    var existingError = await npsContext.tTisTaxErrorTypes
                        .FirstOrDefaultAsync
                        (e => e.fType == 1 && e.fCaption.ToLower().Trim() == error.message.ToLower().
                        Trim());

                    if (existingError == null)
                    {
                        var newError = new tTisTaxErrorType
                        {
                            fCaption = error.message,
                            fDescription = error.message,
                            fCode = error.code ?? "-",
                            fType = 1,
                            fCreatedDateTime = DateTime.Now
                        };
                        npsContext.tTisTaxErrorTypes.AddOrUpdate(e => e.fID, newError);
                        await npsContext.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error logging error code {error.code}: {ex.Message}");
                }
            }
        }
        public async Task LogErrorsAsync(NPSGeneralContext npsContext, List<ErrorModel> errors)
        {
            foreach (var error in errors)
            {
                try
                {
                    var existingError = await npsContext.tTisTaxErrorTypes
                        .FirstOrDefaultAsync
                        (e => e.fType == 1 && e.fCode.ToLower().Trim() == error.Code.ToLower().
                        Trim());

                    if (existingError == null)
                    {
                        var newError = new tTisTaxErrorType
                        {
                            fCaption = error.Message,
                            fDescription = error.Message,
                            fCode = error.Code,
                            fType = 1,
                            fCreatedDateTime = DateTime.Now
                        };
                        npsContext.tTisTaxErrorTypes.AddOrUpdate(e => e.fID, newError);
                        await npsContext.SaveChangesAsync();
                    }

                }
                catch (Exception ex)
                {
                    // Log the exception for debugging
                    Console.WriteLine($"Error logging error code {error.Code}: {ex.Message}");
                }
            }
        }

        public List<DTO_UV_TissInfoForShow> GetAllTissInfoItems(Guid invoiceID)
        {
            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
  .LogInfoMessage($" BaranDB :" +
      $"GetAllTissInfoItems() :invoiceID:: " + invoiceID
          );
                var items = ExecCommandToObject<DTO_UV_TissInfoForShow>(string.Format(Consts.GetTissInfoViewQuery, invoiceID));
                return items;

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }
        }
        public DTOTaxHistory GetTaxHistory(Guid invoiceID)
        {
            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
  .LogInfoMessage($" BaranDB :" +
      $"GetTaxHistory() :invoiceID:: " + invoiceID
          );
                //var items = ExecCommandToObject<DTOTaxHistory>(string.Format(Consts.GetTaxFromTaxHistoryQuery, invoiceID));
                //if (items != null && items.Any() && items.FirstOrDefault() != null)
                //    return items.FirstOrDefault();

                using (NPSGeneralContext nPSGeneral = new NPSGeneralContext())
                {
                    var vwTaxGoverment = nPSGeneral.vwTaxHistories.FirstOrDefault(a => a.InvoiceId == invoiceID);
                    var dtotax = ObjectMapper.TargetObject<DTOTaxHistory>(vwTaxGoverment);
                    return dtotax;

                }
                return null;

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                 LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
             + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }
        }
        
        
        
        public List<DTOStore> GetStore()
        {
            try
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogInfoMessage($" BaranDB :" +
    $"GetStore() "
        );

                DateTime now1 = DateTime.Now;
                var items = ExecCommandToObject<DTOStore>(string.Format(Consts.GetStoreQuery));
                var timeSpan = (DateTime.Now - now1).TotalSeconds;
                return items.Where(a => a.StatusID == 243).OrderByDescending(a => a.Name).ToList();

            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
               LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
           + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }
        }

        public Task<GeneralInvoiceViewModel> GetInvoice(string InvoiceID)
        {
            ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
.LogInfoMessage($" BaranDB :" +
    $"GetInvoice() InvoiceID::" + InvoiceID
        );
            GeneralInvoiceViewModel Invoice = new GeneralInvoiceViewModel();
            try
            {
                var headerCommend = string.Format(Consts.TaxGetAllInvoiceFromBaran, InvoiceID);
                var header = ExecCommandToObject<GeneralInvoiceHeaderViewModel>(headerCommend);
                var DetailCommend = string.Format(Consts.TaxGetInvoiceDetailForShowBaran, InvoiceID);
                var details = ExecCommandToObject<GeneralInvoiceDetailViewModel>(DetailCommend);
                Invoice.InvoiceID = InvoiceID;
                Invoice.Header = header.FirstOrDefault();
                Invoice.Details = new List<GeneralInvoiceDetailViewModel>();
                Invoice.Details = details;

            }
            catch (Exception ex)
            {

                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
               LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
           + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
            }
            return Task.FromResult(Invoice);
        }

     

        public List<Invoice> GetGroupOfInvoiceForSend(short invoicetype, string invoiceId, bool isForTis)
        {
            List<Invoice> list = new List<Invoice>();
            try
            {
                if (isForTis)
                {
                    List<TissInvoiceHeader> res = ExecCommandToObject<TissInvoiceHeader>(string.Format(Consts.GetInvoiceGroupHeaderForSend, invoicetype, invoiceId, 1));
                    if (res != null)
                    {

                        foreach (var item in res)
                        {
                            try
                            {
                                var isReturn = item.ins == 4 ? 1 : 0;

                                var rows = ExecCommandToObject<DTO_UV_TissInfo>(string.Format(Consts.GetLastInvoiceDetailQuery, item.InvoiceId, isReturn, 1));

                                Invoice inv = new Invoice();
                                inv.TissHeader = new TissInvoiceHeader();
                                inv.TissHeader = item;
                                inv.TisRows = new List<DTO_UV_TissInfo>();
                                inv.TisRows = rows;
                                list.Add(inv);
                            }
                            catch
                            {

                            }
                        }

                    }
                }
                else
                {
                    var res = ExecCommandToObject<TaxInvoiceHeaderSysDTO>(string.Format(Consts.GetInvoiceGroupHeaderForSend, invoicetype, invoiceId, 0));
                    if (res != null)
                    {

                        foreach (var item in res)
                        {
                            var isReturn = item.Ins == 4 ? 1 : 0;
                            var rows = ExecCommandToObject<BodyItemDto>(string.Format(Consts.GetLastInvoiceDetailQuery, item.InvoiceId, isReturn, 0));

                            Invoice inv = new Invoice();
                            inv.Header = new TaxInvoiceHeaderSysDTO();
                            inv.Header = item;
                            inv.Rows = rows;
                            // inv.Rows = rows;
                            list.Add(inv);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
               LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
           + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                // _logger.LogError(ex, "GetLastInvoiceForSend error ");

            }
            return list;
        }

        public async Task<string> GetNotFinishedLastRefrenceId()
        {
            var res = ExecReader(Consts.TaxGovermentLastRefrenceId, "LastRefrenceId");
            return res;
        }



        public void SaveInvoiceInquiryLog(string refId, string status, string uId, string data, string packetType)
        {
            ExecCommand(string.Format(Consts.TaxGovermentSaveInvoiceLogInsertQuery, refId, status, uId, data, packetType));
        }

        //public void SaveSetting(TaxSetting taxSetting)
        //{
        //    if (!string.IsNullOrEmpty(taxSetting.ClientId))
        //        ExecCommandToObject<TaxSetting>(string.Format(Consts.UpdateTaxSetting_TaxId, taxSetting.ClientId));
        //    if (!string.IsNullOrEmpty(taxSetting.PrivateKeyPath))
        //        ExecCommandToObject<TaxSetting>(string.Format(Consts.UpdateTaxSetting_PrivateKeyPath, taxSetting.PrivateKeyPath));
        //}

        public void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel)
        {
            ExecCommand(string.Format(Consts.TaxGovermentInvoiceFinalInsertUpdateQuery, invoiceId, lastStatus, "NULL", TaxId, lastRefId, (IsCancel == true ? 1 : 0)));
        }
        public void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, short Done, string TaxId, string lastRefId)
        {
            ExecCommand(string.Format(Consts.TaxGovermentInvoiceFinalInsertUpdateQuery, invoiceId, lastStatus, Done, TaxId, lastRefId, 0));
        }
    }
    public class ErrorModel
    {
        public string Code { get; }

        public string Message { get; }

        public ErrorModel(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
