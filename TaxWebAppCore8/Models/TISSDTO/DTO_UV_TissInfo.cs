using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace TaxServiceAndApp.Models.TISSDTO
{
    public class DTO_UV_TissInfo
    {
  

     
        public System.Guid InvoiceID { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }

        public Nullable<decimal> cap { get; set; }
        public Nullable<int> bid { get; set; }
        public Nullable<int> bpc { get; set; }
        public Nullable<int> bbc { get; set; }
        public Nullable<int> bpn { get; set; }
        public Nullable<int> ft { get; set; }
        public Nullable<int> scln { get; set; }
        public Nullable<int> scc { get; set; }
        public Nullable<int> crn { get; set; }
        public Nullable<int> cdcn { get; set; }
        public Nullable<int> cdcd { get; set; }
        public Nullable<int> billid { get; set; }
        public Nullable<decimal> tprdis { get; set; }
        public Nullable<decimal> tdis { get; set; }
        public Nullable<decimal> tadis { get; set; }
        public Nullable<decimal> tvam { get; set; }
        public Nullable<int> todam { get; set; }
        public Nullable<decimal> tbill { get; set; }
        public Nullable<int> tonw { get; set; }
        public Nullable<int> torv { get; set; }
        public Nullable<int> tocv { get; set; }
        public int setm { get; set; }
        public Nullable<int> insp { get; set; }
        public Nullable<int> tvop { get; set; }
        public string taxid { get; set; }
        public string indatim { get; set; }
        public int inty { get; set; }
        public Nullable<long> inno { get; set; }
        public string irtaxid { get; set; }
        public int inp { get; set; }
        public Nullable<int> ins { get; set; }
        public string tins { get; set; }
        public Nullable<int> tinb { get; set; }
        public Nullable<int> tob { get; set; }
        public Nullable<int> Tax17 { get; set; }
        public Nullable<int> Indati2m { get; set; }
        public string sstid { get; set; }
        public string sstt { get; set; }
        public Nullable<int> mu { get; set; }
        public Nullable<decimal> am { get; set; }
      
        public Nullable<int> nw { get; set; }
        public decimal? fee { get; set; }
        
        public Nullable<int> cfee { get; set; }
        public Nullable<int> cut { get; set; }
        public Nullable<int> exr { get; set; }
        public Nullable<int> ssrv { get; set; }
        public Nullable<int> sscv { get; set; }
        public Nullable<decimal> prdis { get; set; }
        
        public Nullable<decimal> dis { get; set; }
        public Nullable<decimal> adis { get; set; }
     
        public Nullable<decimal> vra { get; set; }
        public Nullable<decimal> vam { get; set; }
        public Nullable<int> odt { get; set; }
        public Nullable<int> odr { get; set; }
        public Nullable<int> odam { get; set; }
        public string olt { get; set; }
        public Nullable<int> olr { get; set; }
        public Nullable<int> sbc { get; set; }
        public Nullable<int> olam { get; set; }
        public Nullable<int> consfee { get; set; }
        public Nullable<int> spro { get; set; }
        public Nullable<int> bros { get; set; }
        public Nullable<int> tcpbs { get; set; }
        public Nullable<int> cop { get; set; }
        public Nullable<int> vop { get; set; }
        public Nullable<int> bsrn { get; set; }
        public Nullable<decimal> tsstam { get; set; }
       
        public Nullable<int> iinn { get; set; }
        public Nullable<int> acn { get; set; }
        public Nullable<int> trmn { get; set; }
        public Nullable<int> pmt { get; set; }
        public Nullable<int> trn { get; set; }
        public Nullable<int> pcn { get; set; }
        public Nullable<int> pid { get; set; }
        public Nullable<int> pdt { get; set; }
        public Nullable<int> pv { get; set; }
        public Nullable<System.Guid> ItemID { get; set; }

    }



    public class DTO_UV_TissInfoForShow: DTO_UV_TissInfo
    {
        public Nullable<int> TypeID { get; set; }
        public Nullable<int> InvoiceTypeID { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public Nullable<System.DateTime> BusinessDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string ItemName { get; set; }
        public string BarCode { get; set; }
        public int BookerStoreID { get; set; }
        public int BookerWorkstationID { get; set; }
        public string amS
        {
            get
            {
                return am?.ToString("#,0");
            }
        }
    
        public string feeS
        {
            get
            {
                return fee?.ToString("#,0");
            }
        }
    
        public string prdisS
        {
            get
            {
                return prdis?.ToString("#,0");
            }
        }
   
        public string adisS
        {
            get
            {
                return adis?.ToString("#,0");
            }
        }
     
        public string tsstamS
        {
            get
            {
                return tsstam?.ToString("#,0");
            }
        }
      
    }
}