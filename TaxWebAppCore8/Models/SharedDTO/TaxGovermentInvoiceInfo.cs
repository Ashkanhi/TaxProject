namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaxGovermentInvoiceInfo")]
    public partial class TaxGovermentInvoiceInfo
    {
        public long TaxGovermentInvoiceInfoId { get; set; }

        public Guid InvoiceId { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(50)]
        public string InvoiceUniqueId { get; set; }

        [StringLength(100)]
        public string UId { get; set; }

        public DateTime? SendDate { get; set; }

        public bool? IsReturn { get; set; }

        public bool? Done { get; set; }
    }
}
