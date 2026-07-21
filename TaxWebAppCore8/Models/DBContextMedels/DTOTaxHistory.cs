using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TaxWebAppCore8.Models.DBContextMedels
{
    public class DTOTaxHistory
    {
        public int SendStat { get; set; }
        public string InvoiceId { get; set; }
        public string StoreName { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumberSt { get; set; }
        public double? TotalPrice { get; set; }
        public double? TotalDiscount { get; set; }
        public double? TotalTax { get; set; }
        public double? NetPrice { get; set; }
        public DateTime? BusinessDate { get; set; }
        public string ReferenceNumber { get; set; }
        public int? StoreID { get; set; }



        public int? fSendState { get; set; }

        public string fListOfErrorCode { get; set; }

        public string fListOfErrorCaption { get; set; }


        public DateTime? fLastSendDate { get; set; }

        public string fInvoiceID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long fID { get; set; }
    }
}
