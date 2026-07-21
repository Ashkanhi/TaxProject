namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tSendInvoiceTaxTissLog")]
    public partial class tSendInvoiceTaxTissLog
    {
        [Key]
        public long fID { get; set; }

        [Required]
        public string fReferencedID { get; set; }

        [Required]
        public Guid fInvoiceID { get; set; }

        public DateTime fLastSendDate { get; set; }

        public bool fDeleted { get; set; }

        public short fOrder { get; set; }

        public short fStatus { get; set; }

        public Guid fLogUID { get; set; }

        public string fTaxID { get; set; }
    }
}
