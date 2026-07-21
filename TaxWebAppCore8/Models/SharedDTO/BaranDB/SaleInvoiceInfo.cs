namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SaleInvoiceInfo")]
    public partial class SaleInvoiceInfo
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

        [StringLength(20)]
        public string DailyInvoiceNumber { get; set; }

        public Guid? TableID { get; set; }

        public int? ReasonID { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(20)]
        public string OrderCode { get; set; }

        public short? OrderState { get; set; }

        [StringLength(20)]
        public string DoctorCode { get; set; }

        [StringLength(100)]
        public string DoctorName { get; set; }

        [StringLength(100)]
        public string DoctorExpertise { get; set; }

        public bool? IsDoctorExpertise { get; set; }

        [Column(TypeName = "date")]
        public DateTime? VisitDate { get; set; }
    }
}
