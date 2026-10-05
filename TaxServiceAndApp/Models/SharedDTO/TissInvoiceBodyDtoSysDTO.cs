using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaxCollectData.Library.Dto;

namespace TaxServiceAndApp.Models.SharedDTO
{
    public class TissInvoiceBodyDtoSysDTO
    {
        public string id { get; set; }
        public bool retry { get; set; }
        public bool autofill { get; set; }
        public bool encrypted { get; set; }
        public HeaderDto header { get; set; }
        public List<BodyItemDto> body { get; set; }
        public List<PaymentItemDto> payments { get; set; }
        public List<ExtensionItemDto> Extension { get; set; }
        
    }
}