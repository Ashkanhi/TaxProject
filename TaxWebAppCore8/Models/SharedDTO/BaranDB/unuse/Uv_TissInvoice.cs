namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Uv_TissInvoice
    {
        public decimal? ExpectedAmount { get; set; }

        [Key]
        [Column(Order = 0)]
        public Guid InvoiceID { get; set; }

        public int? TypeID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LineItemID { get; set; }

        public Guid? ItemID { get; set; }

        public decimal? vra { get; set; }

        public decimal? ManualDiscount { get; set; }

        public decimal? PriceAmount { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? TotalDiscount { get; set; }

        public decimal? TotalDiscountPerLine { get; set; }

        public decimal? TotalDiscountPercent { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int vop { get; set; }

        public decimal? fee { get; set; }

        public decimal? dis { get; set; }

        public decimal? adis { get; set; }

        public decimal? vam { get; set; }

        public decimal? NewPayAmount { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(7)]
        public string Ver { get; set; }
    }
}
