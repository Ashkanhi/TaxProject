namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("TaxHistory")]
    public partial class TaxHistory
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

        [Key]
        public long fID { get; set; }
    }
}
