namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaxGovermentSettingInfo")]
    public partial class TaxGovermentSettingInfo
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StoreId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string PrivateKey { get; set; }

        [StringLength(50)]
        public string ClientId { get; set; }

        [StringLength(500)]
        public string PrivateKeyPath { get; set; }

        public int? ResponseCheckerTimeInSec { get; set; }

        public string Url { get; set; }

        [StringLength(50)]
        public string Tins { get; set; }
    }
}
