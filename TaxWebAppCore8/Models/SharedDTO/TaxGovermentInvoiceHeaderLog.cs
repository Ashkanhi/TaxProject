namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaxGovermentInvoiceHeaderLog")]
    public partial class TaxGovermentInvoiceHeaderLog
    {
        [Key]
        public Guid InvoiceId { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? SaveDate { get; set; }

        public int? Inty { get; set; }

        [StringLength(50)]
        public string Taxid { get; set; }

        [StringLength(50)]
        public string Inno { get; set; }

        public int? Inp { get; set; }

        public int? Ins { get; set; }

        [StringLength(50)]
        public string Tins { get; set; }

        public int? Tob { get; set; }

        [StringLength(50)]
        public string Bid { get; set; }

        [StringLength(50)]
        public string Tinb { get; set; }

        [StringLength(50)]
        public string Sbc { get; set; }

        [StringLength(50)]
        public string Bpc { get; set; }

        [StringLength(50)]
        public string Bbc { get; set; }

        public int? Ft { get; set; }

        [StringLength(50)]
        public string Scln { get; set; }

        [StringLength(50)]
        public string Scc { get; set; }

        [StringLength(50)]
        public string Crn { get; set; }

        [StringLength(50)]
        public string Billid { get; set; }

        public decimal? Tprdis { get; set; }

        public decimal? Tdis { get; set; }

        public decimal? Tadis { get; set; }

        public decimal? Tvam { get; set; }

        public decimal? Todam { get; set; }

        public decimal? Tbill { get; set; }

        public int? Setm { get; set; }

        public decimal? Cap { get; set; }

        public decimal? Insp { get; set; }

        public decimal? Tvop { get; set; }

        public decimal? Tax17 { get; set; }

        [StringLength(50)]
        public string Cdcn { get; set; }

        public int? Cdcd { get; set; }

        public decimal? Tonw { get; set; }

        public decimal? Torv { get; set; }

        public decimal? Tocv { get; set; }

        public DateTime? Date { get; set; }

        [StringLength(50)]
        public string irtaxid { get; set; }
    }
}
