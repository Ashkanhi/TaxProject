using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxServiceAndApp.Models.TaxDTO
{
    public class GeneralInvoiceDetailViewModel
    {
        public string InvoiceID { get; set; }
        public string ProductName { get; set; }
        public double Count { set; get; } = 0;
        public decimal UnitPrice { set; get; } = 0;
        public decimal TotalPrice { get { return (decimal)Count * UnitPrice; } }
        public string RowID { get; set; }
        public string UnitName { set; get; }
        public decimal DiscountPercent { set; get; } = 0;
        public decimal DiscountPrice { set; get; }
        public decimal NetPrice { set; get; }
    }
}