using Microsoft.Extensions.Logging;
using System.Data;
using System.Reflection;
using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

using Org.BouncyCastle.Ocsp;
using System.Text.Encodings.Web;
using System.Text.Json;

using TaxCollectData.Library.Models;

namespace TaxServiceAndApp.BI
{
    public static class Extentions
    {
        public static T CreateItemFromRow<T>(this DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            row.SetItemFromRow(item);

            // return 
            return item;
        }
        public static void SetItemFromRow<T>(this DataRow row,T item) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }
        public static long ToTimestamp(this DateTime date)
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        }

        public static InquiryResultModel ParseInquiryResult(this InquiryResultModel inquiryResultModel)
        {
            InquiryResultModel resultData = new InquiryResultModel();
            if (inquiryResultModel != null)
            {
                if (inquiryResultModel.Data != null)
                    resultData = JsonConvert.DeserializeObject<InquiryResultModel>(inquiryResultModel.Data.ToString());

            }
            return resultData;
        }
        public static string ToJson(this object obj)
        {
            
            var options3 = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            var jsonString = System.Text.Json.JsonSerializer.Serialize(obj, options3);
            return jsonString;

        }
    }
}
