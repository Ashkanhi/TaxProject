using System.Collections.Generic;
using TaxCollectData.Library.Dto;
using TaxServiceAndApp.Models.SharedDTO;

namespace TaxServiceAndApp.Models.TaxDTO
{
    public class Invoice
    {
        public TaxInvoiceHeaderSysDTO Header { get; set; }
        public List<BodyItemDto> Rows { get; set; }
        public TissInvoiceHeader TissHeader { get; set; }
        public List<BodyItemDto> TisRows { get; set; }

        }
}
public class CheackCurencyOfInvoice { }
