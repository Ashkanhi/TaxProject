using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaxCollectData.Library.Algorithms;
using TaxCollectData.Library.Dto;
using TaxCollectData.Library.Providers;

namespace TaxServiceAndApp.BI
{
    public static class BTaxMain
    {
        public static string CreateUniqeTaxID(long InvoiceNumber, DateTime? dateTime = null)
        {
            TaxIdProvider taxIdProvider = new TaxIdProvider(new VerhoeffAlgorithm());

            string memoryId = BMain.SrvInfo.MEMORY_ID;
            DateTimeOffset now = new DateTimeOffset((dateTime == null ? DateTime.Now : dateTime.Value));
            string taxId = taxIdProvider.GenerateTaxId(
                                         memoryId,
                                         InvoiceNumber,
                                         now.DateTime);
            return taxId;
        }
    }
}