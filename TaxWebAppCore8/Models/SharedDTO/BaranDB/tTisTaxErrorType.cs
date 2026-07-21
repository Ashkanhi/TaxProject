namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tTisTaxErrorType")]
    public partial class tTisTaxErrorType
    {
        [Key]
        public long fID { get; set; }

        public short fType { get; set; }

        [Required]
        public string fCaption { get; set; }

        [Required]
        public string fDescription { get; set; }

        public DateTime fCreatedDateTime { get; set; }

       
        public string fCode { get; set; }
    }
}
