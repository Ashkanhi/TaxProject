using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using TaxCollectData.Library.Dto;

namespace TaxServiceAndApp.Models.SharedDTO
{
    [DataContract]
    public class SendRequest
    {
        [DataMember] public string invoiceId { set; get; }
        [DataMember] public bool isReturn { set; get; }
        [DataMember] public List<Guid> idList { set; get; }
        public InvoiceDto invoice { set; get; }
    }
}