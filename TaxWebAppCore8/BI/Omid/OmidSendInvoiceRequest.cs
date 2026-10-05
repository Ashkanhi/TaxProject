using System.Collections.Generic;
using Newtonsoft.Json;

namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidSendInvoiceRequest
    {
        [JsonProperty("fiscalId")]
        public string FiscalId { get; set; } = string.Empty;

        [JsonProperty("invoices")]
        public List<OmidInvoice> Invoices { get; set; } = new();
    }

    public sealed class OmidInvoice
    {
        [JsonProperty("header")]
        public OmidInvoiceHeader Header { get; set; } = new();

        [JsonProperty("body")]
        public List<OmidInvoiceBodyItem> Body { get; set; } = new();

        [JsonProperty("payments")]
        public List<OmidPayment> Payments { get; set; } = new();
    }

    public sealed class OmidInvoiceHeader
    {
        [JsonProperty("taxId")] public string TaxId { get; set; }
        [JsonProperty("indatim")] public string Indatim { get; set; }
        [JsonProperty("inty")] public int? Inty { get; set; }
        [JsonProperty("inno")] public long? Inno { get; set; }
        [JsonProperty("inp")] public int? Inp { get; set; }
        [JsonProperty("crn")] public string Crn { get; set; }
        [JsonProperty("tonw")] public decimal? Tonw { get; set; }
        [JsonProperty("setm")] public int? Setm { get; set; }
        [JsonProperty("tax17")] public decimal? Tax17 { get; set; }
        [JsonProperty("ins")] public int? Ins { get; set; }
        [JsonProperty("sg")] public List<object> Sg { get; set; } = new();
        [JsonProperty("tins")] public string Tins { get; set; }
        [JsonProperty("tob")] public int? Tob { get; set; }
        [JsonProperty("bid")] public string Bid { get; set; }
        [JsonProperty("tinb")] public string Tinb { get; set; }
        [JsonProperty("sbc")] public string Sbc { get; set; }
        [JsonProperty("bpc")] public string Bpc { get; set; }
        [JsonProperty("bbc")] public string Bbc { get; set; }
        [JsonProperty("tprdis")] public decimal? Tprdis { get; set; }
        [JsonProperty("tdis")] public decimal? Tdis { get; set; }
        [JsonProperty("tadis")] public decimal? Tadis { get; set; }
        [JsonProperty("tvam")] public decimal? Tvam { get; set; }
        [JsonProperty("todam")] public decimal? Todam { get; set; }
        [JsonProperty("tbill")] public decimal? Tbill { get; set; }
        [JsonProperty("tvop")] public decimal? Tvop { get; set; }
        [JsonProperty("cap")] public decimal? Cap { get; set; }
        [JsonProperty("insp")] public string Insp { get; set; }
    }

    public sealed class OmidInvoiceBodyItem
    {
        [JsonProperty("sstt")] public string Sstt { get; set; }
        [JsonProperty("sstid")] public string Sstid { get; set; }
        [JsonProperty("prdis")] public decimal? Prdis { get; set; }
        [JsonProperty("dis")] public decimal? Dis { get; set; }
        [JsonProperty("adis")] public decimal? Adis { get; set; }
        [JsonProperty("vra")] public decimal? Vra { get; set; }
        [JsonProperty("vam")] public decimal? Vam { get; set; }
        [JsonProperty("tsstam")] public decimal? Tsstam { get; set; }
        [JsonProperty("cfee")] public string Cfee { get; set; }
        [JsonProperty("cut")] public string Cut { get; set; }
        [JsonProperty("exr")] public string Exr { get; set; }
        [JsonProperty("odr")] public decimal? Odr { get; set; }
        [JsonProperty("odam")] public decimal? Odam { get; set; }
        [JsonProperty("olr")] public decimal? Olr { get; set; }
        [JsonProperty("olam")] public decimal? Olam { get; set; }
        [JsonProperty("olt")] public string Olt { get; set; }
        [JsonProperty("odt")] public string Odt { get; set; }
        [JsonProperty("cop")] public decimal? Cop { get; set; }
        [JsonProperty("vop")] public decimal? Vop { get; set; }
        [JsonProperty("am")] public decimal? Am { get; set; }
        [JsonProperty("mu")] public string Mu { get; set; }
        [JsonProperty("fee")] public decimal? Fee { get; set; }
        [JsonProperty("consfee")] public decimal? Consfee { get; set; }
        [JsonProperty("spro")] public decimal? Spro { get; set; }
        [JsonProperty("bros")] public decimal? Bros { get; set; }
        [JsonProperty("tcpbs")] public decimal? Tcpbs { get; set; }
        [JsonProperty("cui")] public string Cui { get; set; }
        [JsonProperty("bsrn")] public string Bsrn { get; set; }
    }

    public sealed class OmidPayment
    {
        [JsonProperty("iinn")] public string Iinn { get; set; }
        [JsonProperty("acn")] public string Acn { get; set; }
        [JsonProperty("trmn")] public string Trmn { get; set; }
        [JsonProperty("pmt")] public int? Pmt { get; set; }
        [JsonProperty("trn")] public string Trn { get; set; }
        [JsonProperty("pcn")] public string Pcn { get; set; }
        [JsonProperty("pid")] public string Pid { get; set; }
        [JsonProperty("pdt")] public string Pdt { get; set; }
        [JsonProperty("pv")] public decimal? Pv { get; set; }
    }
}