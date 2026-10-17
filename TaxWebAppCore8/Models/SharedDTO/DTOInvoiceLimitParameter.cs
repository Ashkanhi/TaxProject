using System.Runtime.Serialization;

namespace TaxWebAppCore8.Models.SharedDTO
{
    [DataContract]
    public class DTOInvoiceLimitParameter
    {

        [DataMember] public string InvoiceID { get; set; }

        [DataMember]
        public Guid InvoiceGuidID
        {
            get
            {
                if (!string.IsNullOrEmpty(InvoiceID))
                    return Guid.Parse(InvoiceID);
                else return default(Guid);
            }
        }
        // در ارسال امید مقدار ندارد؛ نباید به‌صورت فیلد اجباری اعتبارسنجی شود.
        public string? TaxGuid { get; set; }
        [DataMember]
        public int? StoreID { get; set; }
        [DataMember]
        public int? Status { get; set; }
        [DataMember]
        public int BookerWorkstationID { get; set; }


    }
}
