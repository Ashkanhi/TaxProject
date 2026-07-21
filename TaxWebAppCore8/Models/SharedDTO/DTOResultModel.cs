using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaxCollectData.Library.Dto;

namespace TaxServiceAndApp.Models.SharedDTO
{
    public class ResultModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public InvoiceDto TaxBreif { get; set; }
        public TissInvoiceBodyDtoSysDTO TissBreif { get; set; }
        public object Data { set; get; }
        public bool Success { get; set; }
        public string Status { set; get; }
        public int Count { get; set; }

    }
}