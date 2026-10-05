using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxServiceAndApp.Models.TaxDTO
{
    public class GeneralInvoiceViewModel
    {
        public string InvoiceID { get; set; }
        public GeneralInvoiceHeaderViewModel Header { get; set; }
        public List<GeneralInvoiceDetailViewModel> Details { get; set; }

    }
}