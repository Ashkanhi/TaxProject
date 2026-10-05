using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaxCollectData.Library.Dto;

namespace TaxServiceAndApp.Models.TaxDTO
{
    public class TissInvoiceHeader: HeaderDto
    {
        public string InvoiceId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime SaveDate { get; set; }
        public DateTime TaxIdDate { get { return CreateDate; } }
      
    }
}