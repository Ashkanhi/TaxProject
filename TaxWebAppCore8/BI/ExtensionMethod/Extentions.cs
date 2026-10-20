using Microsoft.Extensions.Logging;
using System.Data;
using System.Reflection;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

//using Org.BouncyCastle.Ocsp;
using System.Text.Encodings.Web;
using System.Text.Json;

using TaxCollectData.Library.Models;
using System.Linq;

namespace TaxServiceAndApp.BI
{
    public static class Extentions
    {
        public static T CreateItemFromRow<T>(this DataRow row, bool convertNumericStringsForOmid = false) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            row.SetItemFromRow(item, convertNumericStringsForOmid);

            // return 
            return item;
        }
        //public static void SetItemFromRow<T>(this DataRow row,T item) where T : new()
        //{
        //    // go through each column
        //    foreach (DataColumn c in row.Table.Columns)
        //    {
        //        // find the property for the column
        //        PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

        //        // if exists, set the value
        //        if (p != null && row[c] != DBNull.Value)
        //        {
        //            p.SetValue(item, row[c], null);
        //        }
        //    }
        //}
        public static void SetItemFromRow<T>(this DataRow row, T item, bool convertNumericStringsForOmid = false) where T : new()
        {
            // Go through each column in the DataRow
            foreach (DataColumn column in row.Table.Columns)
            {
                PropertyInfo property = item.GetType().GetProperties()
               .FirstOrDefault(p => string.Equals(
                   p.Name.ToLower(), column.ColumnName.ToLower(), StringComparison.OrdinalIgnoreCase)
               || p.Name.ToLower().Replace("_", "") == column.ColumnName.ToLower().Replace("_", "")
               );
                if (column.ColumnName.ToLower() == "inno")
                    Console.WriteLine("");
                // If the property exists and the value is not DBNull
                if (property != null && row[column] != DBNull.Value)
                {
                    try
                    {
                        // Handle conversion for Nullable<long> (or int?) from Decimal
                        if (property.PropertyType == typeof(long?) && row[column] is decimal decimalValue)
                        {
                            property.SetValue(item, (long?)Convert.ToInt64(decimalValue));
                        }
                        else if (property.PropertyType == typeof(int?) && row[column] is decimal decimalValueForInt)
                        {
                            property.SetValue(item, (int?)Convert.ToInt32(decimalValueForInt));
                        }
                        else if (property.PropertyType == typeof(int) && row[column] is decimal decimalValueForIntu)
                        {
                            property.SetValue(item, (int)Convert.ToInt32(decimalValueForIntu));
                        }
                        else if (property.PropertyType == typeof(decimal?) && row[column] is int intValueForDecimal)
                        {
                            property.SetValue(item, (decimal?)Convert.ToInt32(intValueForDecimal));
                        }
                        else if (property.PropertyType == typeof(int?) && row[column] is string stringValue)
                        {
                            property.SetValue(item, (int?)Convert.ToInt32(stringValue));
                        }
                        else if (property.PropertyType == typeof(long?) && row[column] is string stringValue2)
                        {
                            property.SetValue(item, (long?)Convert.ToInt64(stringValue2));
                        }
                        else if (property.PropertyType == typeof(decimal?) && row[column] is string stringDecimal)
                        {
                            property.SetValue(item, (decimal?)Convert.ToDecimal(stringDecimal));
                        }
                        else if (property.PropertyType == typeof(int?) && row[column] is int intnullint)
                        {
                            property.SetValue(item, (int?)Convert.ToInt32(intnullint));
                        }
                        else if (property.PropertyType == typeof(int?) && row[column] is long longnullint)
                        {
                            property.SetValue(item, (int?)Convert.ToInt32(longnullint));
                        }  
                        else if (property.PropertyType == typeof(int) && row[column] is long long2nullint)
                        {
                            property.SetValue(item, (int)Convert.ToInt32(long2nullint));
                        }
                        else if (property.PropertyType == typeof(long) && row[column] is int intgnullint)
                        {
                            property.SetValue(item, (long)Convert.ToInt32(intgnullint));
                        }
                        else if (property.PropertyType == typeof(int) && row[column] is int intint)
                        {
                            property.SetValue(item, (int)Convert.ToInt32(intint));
                        }
                        else if (property.PropertyType == typeof(string) && row[column] is string stringstring)
                        {
                            property.SetValue(item, row[column].ToString());

                        }
                        else if (property.PropertyType == typeof(string) && row[column] is Int64 strintoint64)
                        {
                            property.SetValue(item, row[column].ToString());
                        }
                        else if (convertNumericStringsForOmid && property.PropertyType == typeof(string) &&
                                 (row[column] is int || row[column] is short ||
                                  row[column] is long || row[column] is decimal))
                        {
                            // فقط مسیر امید این تبدیل را فعال می‌کند؛ ریما رفتار قبلی را نگه می‌دارد.
                            // بعضی ستون‌ها مثل MU عددی‌اند، اما DTO آن‌ها را رشته نگه می‌دارد.
                            property.SetValue(
                                item,
                                Convert.ToString(row[column], CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            // Set the value directly for other types
                            if (row[column].GetType() == typeof(string))
                                property.SetValue(item, row[column].ToString());
                            else property.SetValue(item, row[column]);

                        }
                    }
                    catch (Exception er)
                    {

                    }
                }
                // Find the property for the column, using case-insensitive matching
                //PropertyInfo property = item.GetType().GetProperties()
                //    .FirstOrDefault(p => string.Equals(p.Name, column.ColumnName, StringComparison.OrdinalIgnoreCase));

                //// If the property exists and the value is not DBNull
                //if (property != null && row[column] != DBNull.Value)
                //{

                //    // Check if the property type is int and the value is a string
                //    if (property.PropertyType == typeof(Int64) && row[column] is string stringValue)
                //    {
                //        // Try to parse the string to an int
                //        if (Int64.TryParse(stringValue, out Int64 intValue))
                //        {
                //            property.SetValue(item, intValue);
                //        }
                //        else
                //        {
                //            // Handle the case where parsing fails (optional)
                //            Console.WriteLine($"Failed to parse '{stringValue}' to int for property '{property.Name}'.");
                //        }
                //    }
                //    if (property.PropertyType == typeof(Int64) && row[column] is decimal decimalValue)
                //    {

                //        property.SetValue(item, (Int64)decimalValue);

                //    }
                //    else
                //    {
                //        try
                //        {

                //            // Set the value directly for other types
                //            property.SetValue(item, row[column]);
                //        }
                //        catch (Exception ee)
                //        {

                //            //throw ee;
                //        }
                //    }
                //}
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
