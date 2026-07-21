namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tTaxHistoryStateParallel")]
    public partial class tTaxHistoryStateParallel
    {
        [Key]
        public long fID { get; set; }

        [Required]
        public Guid fInvoiceID { get; set; }

        public int fSendState { get; set; }

        [Required]
        public string fListOfErrorCode { get; set; }

        [Required]
        public string fListOfErrorCaption { get; set; }

        public string ReferenceNumber { get; set; }

        public DateTime? fLastSendDate { get; set; }


        public string fTaxID { get; set; }
    }
}
