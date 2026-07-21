using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaxCollectData.Library.Dto;

namespace TaxServiceAndApp.Models.TaxDTO
{
    public class TissInvoiceHeader
    {
        public string InvoiceId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
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
        public decimal tprdis { set; get; }
        public decimal tdis { set; get; }
        public decimal tadis { set; get; }
        public int? tvam { set; get; }
        public int? todam { set; get; }
        public decimal tbill { set; get; }
        public int? tonw { set; get; }
        public int? torv { set; get; }
        public int? tocv { set; get; }
        public int? setm { set; get; }
        public decimal? cap { set; get; }
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