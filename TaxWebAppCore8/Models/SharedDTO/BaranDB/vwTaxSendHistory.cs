namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("vwTaxSendHistory")]
    public partial class vwTaxSendHistory
    {
       
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long fID { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid fLogUID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long fErrorType { get; set; }

        public string fCaption { get; set; }

        public string fErrorCode { get; set; }

        public string fReferencedID { get; set; }

        public string fInvoiceID { get; set; }

        public DateTime? fLastSendDate { get; set; }

        public bool? fDeleted { get; set; }

        public short? fOrder { get; set; }

        public short? fStatus { get; set; }

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

        [Key]
        [Column(Order = 0)]
        public Guid InvoiceId { get; set; }


        public Guid? fSendLogUID { get; set; }

        public string fTaxID { get; set; }

    }
}
