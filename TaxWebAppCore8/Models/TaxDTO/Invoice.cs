using System.Collections.Generic;
using TaxCollectData.Library.Dto;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TISSDTO;

namespace TaxServiceAndApp.Models.TaxDTO
{
    public class Invoice
    {
        public TaxInvoiceHeaderSysDTO Header { get; set; }
        public List<BodyItemDto> Rows { get; set; }
        public TissInvoiceHeader TissHeader { get; set; }
        public List<DTO_UV_TissInfo> TisRows { get; set; }
    }
}
public class CheackCurencyOfInvoice { }
