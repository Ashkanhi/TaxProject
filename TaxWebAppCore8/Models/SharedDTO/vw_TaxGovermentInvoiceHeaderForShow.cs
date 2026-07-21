using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaxCollectData.Library.Models;
using TaxServiceAndApp.Models;

namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{


    public partial class vw_TaxGovermentInvoiceHeaderForShow
    {

        [StringLength(50)]
        public string StoreName { get; set; }

        [StringLength(152)]
        public string CustomerName { get; set; }

        [StringLength(50)]
        public string InvoiceNumberSt { get; set; }

        public decimal? TotalPrice { get; set; }

        public decimal? TotalDiscount { get; set; }

        public decimal? TotalTax { get; set; }

        public decimal? NetPrice { get; set; }

        [Column(TypeName = "date")]
        public DateTime? CreateDate { get; set; }

        [StringLength(100)]
        public string InvoiceDescription { get; set; }

        public int? StoreID { get; set; }

        public int? fSendState { get; set; }

        public string fListOfErrorCode { get; set; }

        public string fListOfErrorCaption { get; set; }

        public string fTaxHistoryReferenceNumber { get; set; }

        public DateTime? fLastSendDate { get; set; }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long fID { get; set; }

        public int? BookerWorkstationID { get; set; }

        public string fTaxID { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid InvoiceId { get; set; }
    }
}
