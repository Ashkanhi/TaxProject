namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SaleInvoiceView")]
    public partial class SaleInvoiceView
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BookerStoreID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BookerWorkstationID { get; set; }

        [Key]
        [Column(Order = 2)]
        public Guid InvoiceID { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? BusinessDate { get; set; }

        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        public int? InvoiceTypeID { get; set; }

        public Guid? CustomerID { get; set; }

        public decimal? ExpectedAmount { get; set; }

        public Guid? CreationUser { get; set; }

        public DateTime? CreationDate { get; set; }

        public Guid? ModifyUser { get; set; }

        public DateTime? ModifyDate { get; set; }

        public Guid? AuthorizeUser { get; set; }

        public bool? IsOnlineSale { get; set; }

        public int? ScanCount { get; set; }

        public TimeSpan? InvoiceTime { get; set; }

        public int? SaleChannelID { get; set; }

        public Guid? StockID { get; set; }

        public byte? SitNumber { get; set; }

        [StringLength(20)]
        public string DailyInvoiceNumber { get; set; }

        [StringLength(152)]
        public string CustomerName { get; set; }

        public int? LanguageID { get; set; }

        [StringLength(50)]
        public string BookerStoreName { get; set; }

        [StringLength(50)]
        public string InvoiceType { get; set; }
    }
}
