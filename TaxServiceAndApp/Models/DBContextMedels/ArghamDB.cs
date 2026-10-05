using System.Data.SqlClient;
using System.Data;

using Newtonsoft.Json;
using System.Threading.Tasks;
using TaxServiceAndApp.Models.TaxDTO;
using TaxServiceAndApp.BI;
using System.Collections.Generic;
using System;
using System.Linq;
using TaxServiceAndApp.Models.SharedDTO;
namespace TaxServiceAndApp.Models
{
    public class ArghamDB : IContext
    {
        private static ArghamDB _arghamDB = null;

        private readonly string _connectionString;
        private ArghamDB()
        {
            _connectionString = BMain.SrvInfo.DBConn;

        }
        public static ArghamDB getInstance()
        {
            if (_arghamDB == null)
                _arghamDB = new ArghamDB();
            return _arghamDB;
        }
        public void ExecCommand(string command)
        {

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
            }
            catch (Exception ex)
            {
            }
            finally
            {

            }
        }
        public string ExecReader(string command, string returnfield)
        {

            var res = "";

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
            }
            catch (Exception ex)
            {
            }
            finally
            {

            }
            return res;
        }
        public List<T> ExecCommandToObject<T>(string command) where T : new()
        {

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
            }

            finally
            {
            }
            // return the list
            return lst;

        }
        public TaxSetting GetAllSetting(string StoreId)
        {

            var setings = ExecCommandToObject<TaxSetting>(Consts.GetTaxSettingQuery);
            if (setings != null && !string.IsNullOrEmpty(StoreId) && int.Parse(StoreId) > 0 && setings.Count > 1)
            {

                return setings.FirstOrDefault(a => a.StoreId == int.Parse(StoreId));
            }
            else
            {

                var set = setings.FirstOrDefault();
                set.StoreId = 0;
                return set;

            }
        }
        public void SaveSetting(TaxSetting taxSetting)
        {

            if (!string.IsNullOrEmpty(taxSetting.ClientId))
                ExecCommandToObject<TaxSetting>(string.Format(Consts.UpdateTaxSetting_TaxId, taxSetting.ClientId));
            if (!string.IsNullOrEmpty(taxSetting.PrivateKeyPath))
                ExecCommandToObject<TaxSetting>(string.Format(Consts.UpdateTaxSetting_PrivateKeyPath, taxSetting.PrivateKeyPath));

        }
        public Invoice GetLastInvoiceForSend(short invoicetype, string invoiceId, bool isForTis)
        {

            try
            {
                if (isForTis)
                {
                    var res = ExecCommandToObject<TissInvoiceHeader>(string.Format(Consts.GetLastInvoiceHeaderQuery, invoicetype, invoiceId, 1)).FirstOrDefault();
                    if (res != null)
                    {
                        var isReturn = res.ins == 4 ? 1 : 0;

                        var rows = ExecCommandToObject<InvoiceBodyDto>(string.Format(Consts.GetLastInvoiceDetailQuery, res.InvoiceId, isReturn, 1));
                        return new Invoice()
                        {
                            TissHeader = res,
                            Rows = rows
                        };
                    }
                    return null;
                }
                else
                {
                    var res = ExecCommandToObject<TaxInvoiceHeader>(string.Format(Consts.GetLastInvoiceHeaderQuery, invoicetype, invoiceId, 0)).FirstOrDefault();
                    if (res != null)
                    {
                        var isReturn = res.Ins == 4 ? 1 : 0;

                        var rows = ExecCommandToObject<InvoiceBodyDto>(string.Format(Consts.GetLastInvoiceDetailQuery, res.InvoiceId, isReturn, 0));
                        return new Invoice()
                        {
                            Header = res,
                            Rows = rows
                        };
                    }
                    return null;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetLastInvoiceForSend error ");
                return null;
            }
        }
        public string GetNotFinishedLastRefrenceId()
        {
            var res = ExecReader(Consts.TaxGovermentLastRefrenceId, "LastRefrenceId");
            return res;
        }
        public void CreateSaveInvoiceResult(string invoiceId, string refId, string status, string uniqueId, string uId)
        {
            ExecCommand(string.Format(Consts.TaxGovermentSaveInvoiceInsertQuery, invoiceId, refId, status, uniqueId, uId));
        }
        public void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel)
        {
            ExecCommand(string.Format(Consts.TaxGovermentInvoiceFinalInsertUpdateQuery, invoiceId, lastStatus, "NULL", TaxId, lastRefId, (IsCancel == true ? 1 : 0)));
        }
        public void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, short Done, string TaxId, string lastRefId)
        {
            ExecCommand(string.Format(Consts.TaxGovermentInvoiceFinalInsertUpdateQuery, invoiceId, lastStatus, Done, TaxId, lastRefId, 0));
        }
        public void SaveInvoiceInquiryLog(string refId, string status, string uId, string data, string packetType)
        {
            ExecCommand(string.Format(Consts.TaxGovermentSaveInvoiceLogInsertQuery, refId, status, uId, data, packetType));
        }
        public void ChangeToDoneInvoice(string refId)
        {
            ExecCommand(string.Format(Consts.ChangeToDoneInvoiceQuery, refId));
        }
        public void ChangeToNotDoneInvoice(string refId)
        {
            ExecCommand(string.Format(Consts.ChangeToNotDoneInvoiceQuery, refId));
        }

        public async Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search)
        {
            List<GeneralInvoiceHeaderViewModel> list = new List<GeneralInvoiceHeaderViewModel>();
            try
            {

                var command = string.Format(Consts.TaxGetAllInvoiceFromArgham, 0, search.InvoiceNumberFrom, search.InvoiceNumberTo,
                     search.DateFrom, search.DateTo, search.Status, search.JustLast, search.CompanyID, search.StorID);
                list = ExecCommandToObject<GeneralInvoiceHeaderViewModel>(command);
            }
            catch (Exception ex)
            {


            }
            return list;
        }

        public async Task<GeneralInvoiceViewModel> GetInvoice(string InvoiceID)
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
            return Invoice;

        }

        public List<Invoice> GetGroupOfInvoiceForSend(short invoicetype, string invoiceId, bool isForTis)
        {
            throw new NotImplementedException();
        }
    }
}
