namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UV_TissInfo
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

        public int? BaranTypeId { get; set; }

        public Guid? ItemID { get; set; }

        public int? bid { get; set; }

        public int? bpc { get; set; }

        public int? bbc { get; set; }

        public int? bpn { get; set; }

        public int? ft { get; set; }

        public int? scln { get; set; }

        public int? scc { get; set; }

        public int? crn { get; set; }

        public int? cdcn { get; set; }

        public int? cdcd { get; set; }

        public int? billid { get; set; }

        public int? todam { get; set; }

        public int? tonw { get; set; }

        public int? torv { get; set; }

        public int? tocv { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int setm { get; set; }

        public int? insp { get; set; }

        public int? tvop { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(1)]
        public string taxid { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(4000)]
        public string indatim { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int inty { get; set; }

        public long? inno { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(1)]
        public string irtaxid { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int inp { get; set; }

        public int? ins { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(11)]
        public string tins { get; set; }

        public int? tinb { get; set; }

        public int? tob { get; set; }

        public int? Tax17 { get; set; }

        public int? Indati2m { get; set; }

        [StringLength(100)]
        public string sstid { get; set; }

        [StringLength(255)]
        public string sstt { get; set; }

        public int? mu { get; set; }

        public decimal? am { get; set; }

        public int? nw { get; set; }

        public decimal? fee { get; set; }

        public int? cfee { get; set; }

        public int? cut { get; set; }

        public int? exr { get; set; }

        public int? ssrv { get; set; }

        public int? sscv { get; set; }

        public decimal? prdis { get; set; }

        public decimal? dis { get; set; }

        public decimal? adis { get; set; }

        public decimal? vra { get; set; }

        public decimal? vam { get; set; }

        public int? odt { get; set; }

        public int? odr { get; set; }

        public int? odam { get; set; }

        [StringLength(1)]
        public string olt { get; set; }

        public int? olr { get; set; }

        public int? sbc { get; set; }

        public int? olam { get; set; }

        public int? consfee { get; set; }

        public int? spro { get; set; }

        public int? bros { get; set; }

        public int? tcpbs { get; set; }

        public int? cop { get; set; }

        public int? vop { get; set; }

        public int? bsrn { get; set; }

        public decimal? tsstam { get; set; }

        public int? iinn { get; set; }

        public int? acn { get; set; }

        public int? trmn { get; set; }

        public int? pmt { get; set; }

        public int? trn { get; set; }

        public int? pcn { get; set; }

        public int? pid { get; set; }

        public int? pdt { get; set; }

        public int? pv { get; set; }

        public int? InvoiceTypeID { get; set; }

        public DateTime? InvoiceDate { get; set; }

        [StringLength(152)]
        public string CustomerName { get; set; }

        [Column(TypeName = "date")]
        public DateTime? BusinessDate { get; set; }

        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        public decimal? tprdis { get; set; }

        public decimal? tdis { get; set; }

        public decimal? tadis { get; set; }

        public decimal? tvam { get; set; }

        public decimal? tbill { get; set; }

        public decimal? cap { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(7)]
        public string Ver { get; set; }
    }
}
