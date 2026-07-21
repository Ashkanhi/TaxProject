namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaxGovermentInvoiceFinal")]
    public partial class TaxGovermentInvoiceFinal
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceId { get; set; }

        [StringLength(50)]
        public string LastStatus { get; set; }

        public bool? Done { get; set; }

        [StringLength(50)]
        public string TaxId { get; set; }

        [StringLength(50)]
        public string LastRefrenceId { get; set; }

        public DateTime? LastResponseCheckTime { get; set; }
    }
}
