using System.Collections.Generic;
using System.Threading.Tasks;
using TaxCollectData.Library.Dto;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;

namespace TaxServiceAndApp.Models
{
    public interface IContext
    {
        void ExecCommand(string command);
        string ExecReader(string command, string returnfield);
        List<T> ExecCommandToObject<T>(string command) where T : new();

        //TaxSetting GetAllSetting(string StoreId);
        //void SaveSetting(TaxSetting taxSetting);
        Invoice GetLastInvoiceForSend(short invoicetype, string invoiceId, bool isForTis);
        void CreateSaveInvoiceResult(string invoiceId, string refId, string status, string uniqueId, string uId);
        void SaveInvoiceInquiryLog(string refId, string status, string uId, string data, string packetType);
        void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, short Done, string TaxId, string lastRefId);
        void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel);
        void ChangeToDoneInvoice(string refId);
        void ChangeToNotDoneInvoice(string refId);
       Task<string> GetNotFinishedLastRefrenceId();

        Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search);
        Task<GeneralInvoiceViewModel> GetInvoice(string InvoiceID);
        List<Invoice> GetGroupOfInvoiceForSend(short invoicetype, string invoiceId, bool isForTis);
    }
}
