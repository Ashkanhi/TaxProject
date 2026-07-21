using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaxCollectData.Library.Dto;
using TaxCollectData.Library.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using TaxServiceAndApp.Models.TISSDTO;
using TaxWebAppCore8.Models.DBContextMedels;
using TaxWebAppCore8.Models.SharedDTO;

namespace TaxServiceAndApp.Models
{
    public interface IContext
    {
        void ExecCommand(string command);
        string ExecReader(string command, string returnfield);
        List<T> ExecCommandToObject<T>(string command) where T : new();
        DTOTaxHistory GetTaxHistory(Guid invoiceID);
        //TaxSetting GetAllSetting(string StoreId);
        //void SaveSetting(TaxSetting taxSetting);
        Invoice GetLastInvoiceForSend(short invoicetype, DTOInvoiceLimitParameter invoice, int isForTis);
        void CreateSaveInvoiceResult(string invoiceId, string refId, string status, string uniqueId, string uId);
        void SaveInvoiceInquiryLog(string refId, string status, string uId, string data, string packetType);
        void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, short Done, string TaxId, string lastRefId);
        void TaxGovermentInvoiceFinalInsertUpdate(string invoiceId, string lastStatus, string TaxId, string lastRefId, bool IsCancel);
        Task LogErrorsAsync
            (NPSGeneralContext npsContext, List<errorDetail> errors);
  Task LogErrorsAsync
            (NPSGeneralContext npsContext, List<ErrorModel> errors);

        void ChangeToDoneInvoice(string refId);
        void ChangeToNotDoneInvoice(string refId);
        Task<string> GetNotFinishedLastRefrenceId();

        List<DTO_UV_TissInfoForShow> GetAllTissInfoItems(Guid invoiceID);
        List<DTO_UV_TissInfoForShow> GetAllTissInfoItemsFromDetailInfo(Guid invoiceID, int storeId, int workstaionID);
        List<DTOStore> GetStore();
        Task<List<GeneralInvoiceHeaderViewModel>> GetAllInvoice(SearchParameters search);
        Task<GeneralInvoiceViewModel> GetInvoice(string InvoiceID);
        List<Invoice> GetGroupOfInvoiceForSend(short invoicetype, string invoiceId, bool isForTis);
    }
}
