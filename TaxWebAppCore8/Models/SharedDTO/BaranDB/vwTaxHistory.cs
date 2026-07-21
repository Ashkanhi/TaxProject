namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("vwTaxHistory")]
    public partial class vwTaxHistory
    {
        public Guid InvoiceId { get; set; }

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
        public DateTime? BusinessDate { get; set; }

        public int? StoreID { get; set; }

        public int? fSendState { get; set; }

        public string fListOfErrorCode { get; set; }

        public string fListOfErrorCaption { get; set; }

        public string ReferenceNumber { get; set; }

        public DateTime? fLastSendDate { get; set; }

        public string fInvoiceID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long fID { get; set; }

        public string fTaxID { get; set; }

    }
}
