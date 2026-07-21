namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaxGovermentSendLog")]
    public partial class TaxGovermentSendLog
    {
        [Key]
        public long LogId { get; set; }

        public DateTime? Date { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(100)]
        public string RefrenceId { get; set; }

        [StringLength(100)]
        public string UId { get; set; }

        public string Json { get; set; }

        [StringLength(10)]
        public string TaxGovermentInvoiceInfoId { get; set; }

        [StringLength(50)]
        public string PacketType { get; set; }
    }
}
