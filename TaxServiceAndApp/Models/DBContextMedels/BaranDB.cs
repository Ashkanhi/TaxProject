using System.Data.SqlClient;
using System.Data;

using System.Collections.Generic;
using System.Reflection;
using Org.BouncyCastle.Asn1.Ocsp;
using TaxServiceAndApp.BI;
using NPSTools;
using System;
using System.Threading.Tasks;
using TaxServiceAndApp.Models.TaxDTO;
using System.Linq;
using TaxServiceAndApp.Models.SharedDTO;
using TaxCollectData.Library.Dto;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

namespace TaxServiceAndApp.Models
{
    public class BaranDB : IContext
    {
        private static BaranDB _baranDB = null;
        private readonly string _connectionString;
        private BaranDB()
        {
            _connectionString = BMain.SrvInfo.DBConn;
        }
        public static BaranDB getInstance()
        {
            if (_baranDB == null)
                _baranDB = new BaranDB();
            return _baranDB;
        }
        public void ExecCommand(string command)
        {


            if (string.IsNullOrEmpty(_connectionString))
                return;
            try
            {
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($" BaranDB :" +
                    $"ExecCommand_IN : " + command
                        );

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();

                    SqlCommand Sqlcmd = new SqlCommand(command, con);
                    Sqlcmd.CommandType = CommandType.Text;
                    Sqlcmd.ExecuteNonQuery();
                    Sqlcmd.Dispose();
                }
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($" BaranDB :" +
                $"ExecCommand_OUT : " + command
                    );

            }
            catch (Exception ex)
            {


                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
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

            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
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
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($" BaranDB :" +
                $"ExecReader_OUT : " + _connectionString
        );
            }
            catch (Exception ex)
            {
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
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
            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
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
                foreach (DataRow r in dt.Rows)
                {
                    // add to the list

                    lst.Add(ObjectMapper.TargetObject<T>(r.CreateItemFromRow<T>()));

                }
            }
            catch (Exception ex)
            {

                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                    LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " ExecCommandToObject ::" + ex.Message + "::" + ex?.InnerException?.Message, ex);
                return null;
            }

            finally
            {
            }
            // return the list
            return lst;

        }

        public void ChangeToDoneInvoice(string refId)
        {
            ExecCommand(string.Format(Consts.ChangeToDoneInvoiceQuery, refId));

            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
            .LogInfoMessage($" BaranDB :" +
                $"ChangeToDoneInvoice : " + (string.Format(Consts.ChangeToDoneInvoiceQuery, refId))
                        );

        }
        public void ChangeToNotDoneInvoice(string refId)
        {
            ExecCommand(string.Format(Consts.ChangeToNotDoneInvoiceQuery, refId));

            NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
        .LogInfoMessage($" BaranDB :" +
            $"ChangeToDoneInvoice : " + (string.Format(Consts.ChangeToNotDoneInvoiceQuery, refId))
            );


        }


        public void CreateSaveInvoiceResult(string invoiceId, string refId, string status, string uniqueId, string uId)
        {
            try
            {
                ExecCommand(string.Format(Consts.TaxGovermentSaveInvoiceInsertQuery, invoiceId, refId, status, uniqueId, uId));

                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                .LogInfoMessage($" BaranDB :" +
                    $"CreateSaveInvoiceResult : " + (string.Format(Consts.TaxGovermentSaveInvoiceInsertQuery, invoiceId, refId, status, uniqueId, uId))
                    );

            }
            catch (Exception ex)
            {
                NPSTools.Logger.NPSLogger.GetLogger(NPSTools.Logger.ModuleName.ArghamCompany_TaxGoverment).
                LogError($"BaranDB:{MethodBase.GetCurrentMethod().DeclaringType.Name}::"
                + " CreateSaveInvoiceResult : " + ex.Message + "::" + ex?.InnerException?.Message, ex);

            }
        }

        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search)
        {
            List<GeneralInvoiceHeaderViewModel> list = new List<GeneralInvoiceHeaderViewModel>();
            try
            {
                var command = string.Format(Consts.TaxGetAllInvoiceFromBaran, 0, search.InvoiceNumberFrom, search.InvoiceNumberTo,
                     search.DateFromStr, search.DateToStr, search.Status, search.JustLast, search.CompanyID, search.StorID);
                list = ExecCommandToObject<GeneralInvoiceHeaderViewModel>(command);
            }
            catch (Exception ex)
            {
            }
            return list.OrderBy(a => a.SendStat).ThenBy(a => a.InvoiceNumber).ToList();
        }

        //public TaxSetting GetAllSetting(string StoreId)
        //{

        //    var setings = ExecCommandToObject<TaxSetting>(Consts.GetTaxSettingQuery);
        //    if (setings != null && !string.IsNullOrEmpty(StoreId) && int.Parse(StoreId) > 0 && setings.Count > 1)
        //    {

        //        return setings.FirstOrDefault(a => a.StoreId == int.Parse(StoreId));
        //    }
        //    else
        //    {

        //        var set = setings.FirstOrDefault();
        //        set.StoreId = 0;
        //        return set;

        //    }
        //}

        public Task<GeneralInvoiceViewModel> GetInvoice(string InvoiceID)
        {
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


            }
            return Task.FromResult(Invoice);
        }

        public Invoice GetLastInvoiceForSend(short invoicetype,
            string invoiceId, bool isForTis)
        {
            try
            {
                if (isForTis)
                {
                    var res = ExecCommandToObject<TissInvoiceHeader>(string.Format(Consts.GetLastInvoiceHeaderQuery, invoicetype, invoiceId, 1)).FirstOrDefault();
                    if (res != null)
                    {
                        var isReturn = res.ins == 4 ? 1 : 0;
                        var rows = ExecCommandToObject<object>(string.Format(Consts.GetLastInvoiceDetailQuery, invoiceId, isReturn, 1));
                        List<BodyItemDto> Rows = new List<BodyItemDto>();
                        foreach (var irow in rows)
                        {
                            Rows.Add(ObjectMapper.TargetObject<BodyItemDto>(irow));
                        }
                        return new Invoice()
                        {
                            TissHeader = res,
                            TisRows = Rows
                        };
                    }
                    return null;
                }
                else
                {
                    var res = ExecCommandToObject<TaxInvoiceHeaderSysDTO>(string.Format(Consts.GetLastInvoiceHeaderQuery,
                        invoicetype, invoiceId, 0)).FirstOrDefault();
                    if (res != null)
                    {
                        var isReturn = res.Ins == 4 ? 1 : 0;
                        var rows = ExecCommandToObject<object>(string.Format(Consts.GetLastInvoiceDetailQuery,
                            invoiceId, isReturn, 0));
                        var rowsDto = new List<BodyItemDto>();

                        foreach (var item in rows)
                        {
                            rowsDto.Add(ObjectMapper.TargetObject<BodyItemDto>
                                (item));
                        }

                        return new Invoice()
                        {
                            Header = res,
                            Rows = rowsDto
                        };
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
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

                                var rows = ExecCommandToObject<object>(string.Format(Consts.GetLastInvoiceDetailQuery, item.InvoiceId, isReturn, 1));
                                List<BodyItemDto> Rows = new List<BodyItemDto>();
                                foreach (var irow in rows)
                                {
                                    Rows.Add(ObjectMapper.TargetObject<BodyItemDto>(irow));
                                }
                                Invoice inv = new Invoice();
                                inv.TissHeader = new TissInvoiceHeader();
                                inv.TissHeader = item;
                                inv.TisRows = new List<BodyItemDto>();
                                inv.TisRows = Rows;
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
                            var rows = ExecCommandToObject<object>(string.Format(Consts.GetLastInvoiceDetailQuery, item.InvoiceId, isReturn, 0));
                            List<BodyItemDto> Rows = new List<BodyItemDto>();
                            foreach (var irow in rows)
                            {
                                Rows.Add(ObjectMapper.TargetObject<BodyItemDto>(irow));
                            }
                            Invoice inv = new Invoice();
                            inv.Header = new TaxInvoiceHeaderSysDTO();
                            inv.Header = item;
                            inv.Rows = Rows;
                            // inv.Rows = rows;
                            list.Add(inv);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "GetLastInvoiceForSend error ");

            }
            return list;
        }

        public string GetNotFinishedLastRefrenceId()
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
}
