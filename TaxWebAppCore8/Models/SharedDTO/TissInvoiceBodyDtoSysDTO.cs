using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaxCollectData.Library.Dto;
using TaxServiceAndApp.Models.TaxDTO;
using TaxServiceAndApp.Models.TISSDTO;

namespace TaxServiceAndApp.Models.SharedDTO
{
    public class TissInvoiceBodyDtoSysDTO
    {
        public string id { get; set; }
        public bool retry { get; set; }
        public bool autofill { get; set; }
        public bool encrypted { get; set; }
        public TissInvoiceHeader header { get; set; }
        public List<DTO_UV_TissInfo> body { get; set; }
        public List<PaymentItemDto> payments { get; set; }
        public List<ExtensionItemDto> extension { get; set; }
        
    }
}