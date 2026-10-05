using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxServiceAndApp.Models.SharedDTO
{
    public class TaxInvoiceHeaderSysDTO
    {

        public Guid InvoiceId { get; set; }
        public string Taxid { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime SaveDate { get; set; }
        public DateTime TaxIdDate { get { return CreateDate; } }

        public long? Indatim { get; set; }

        public long? Indati2m { get; set; }

        public int? Inty { get; set; }

        public string Inno { get; set; }

        public string Irtaxid { get; set; }

        public int? Inp { get;  set; }

        public int? Ins { get; set; }

        public string Tins { get; set; }

        public int? Tob { get; set; }

        public string Bid { get; set; }

        public string Tinb { get; set; }

        public string Sbc { get; set; }

        public string Bpc { get; set; }

        public string Bbc { get; set; }

        public int? Ft { get; set; }

        public string Bpn { get; set; }

        public string Scln { get; set; }

        public string Scc { get; set; }

        public string Crn { get; set; }

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

        public string Cdcn { get; set; }

        public int? Cdcd { get; set; }

        public decimal? Tonw { get; set; }

        public decimal? Torv { get; set; }

        public decimal? Tocv { get; set; }


    }
    public class TissInvoiceHeaderSysDTO
    {
        public string InvoiceId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime SaveDate { get; set; }
        public DateTime TaxIdDate { get { return CreateDate; } }
        public int? sbc { set; get; }
        public string bid { set; get; }
        public string pbc { set; get; }
        public int? bbc { set; get; }
        public string bpn { set; get; }
        public int? ft { set; get; }
        public int? scln { set; get; }
        public string scc { set; get; }
        public string crn { set; get; }
        public int? cdcn { set; get; }
        public int? cdcd { set; get; }
        public int? billid { set; get; }
        public int tprdis { set; get; }
        public int tdis { set; get; }
        public int tadis { set; get; }
        public int? tvam { set; get; }
        public int? todam { set; get; }
        public int tbill { set; get; }
        public int? tonw { set; get; }
        public int? torv { set; get; }
        public int? tocv { set; get; }
        public int? setm { set; get; }
        public int? cap { set; get; }
        public int? insp { set; get; }
        public int? tvop { set; get; }
        public string taxid { set; get; }
        public string indatim { set; get; }
        public int? inty { set; get; } = 2;
        public long inno { set; get; }
        public string irtaxid { set; get; }
        public int? inp { set; get; } = 1;
        public int? ins { set; get; }
        public string tins { set; get; }
        public string tinb { set; get; }
        public int? tob { set; get; }
        public int? Tax17 { set; get; }
        public int? Indati2m { set; get; }
    }
}