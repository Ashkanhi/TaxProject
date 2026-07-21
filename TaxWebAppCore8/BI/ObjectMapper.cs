using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TaxServiceAndApp.BI
{
    public static class ObjectMapper
    {
        /// <summary>
        /// برای تبدیل اشیا که ویزگی مشترک اما حروف بزرگ و کوچک دارند.
        /// برای مثال 
        /// public class BodyItemDto
        /// public string sstid { get; set; } این مورد در دیتابیس با حروف بزرگ شروع شده است یعنی 
        /// Sstid
        /// لذا برای امکان کست مکانیزه این متد را نوشتیم.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T TargetObject<T>(object source)
        {
            try
            {

                var target = Activator.CreateInstance<T>();
                var sourceType = source.GetType();
                var targetType = typeof(T);

                foreach (PropertyInfo sourceProp in sourceType.GetProperties())
                {


                    try
                    {
                        
                        var targetProp = targetType.GetProperty(sourceProp.Name, BindingFlags.Public | BindingFlags.Instance)
                                     ?? targetType.GetProperty(sourceProp.Name.ToLower(), BindingFlags.Public | BindingFlags.Instance)
                                     ?? targetType.GetProperty(sourceProp.Name.ToUpper(), BindingFlags.Public | BindingFlags.Instance)
                                     ?? targetType.GetProperty(sourceProp.Name.Replace("_", "").ToLower(), BindingFlags.Public | BindingFlags.Instance)
                                     ?? targetType.GetProperty(sourceProp.Name.Replace("_", "").ToUpper(), BindingFlags.Public | BindingFlags.Instance);
                        if (targetProp == null)
                            targetProp = targetType.GetProperty(sourceProp.Name, BindingFlags.Public | BindingFlags.Instance)
                       ?? targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .FirstOrDefault(p => p.Name.ToLower().Equals(sourceProp.Name.ToLower(), StringComparison.OrdinalIgnoreCase));

                        if (targetProp == null)
                        {
                            foreach (var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                            {
                                if (prop.Name.ToLower().Equals(sourceProp.Name.ToLower(), StringComparison.OrdinalIgnoreCase))
                                {
                                    targetProp = prop;
                                    break;
                                }
                            }
                        }

                        if (targetProp != null)
                        {
                            object value = sourceProp.GetValue(source);

                            // Handle type conversion
                            if (value != null)
                            {
                                try
                                {

                                    if ((
                                        targetProp.PropertyType == typeof(long?)
                                        ||
                                        targetProp.PropertyType == typeof(long)) && (sourceProp.PropertyType == typeof(decimal?) ||
                                      sourceProp.PropertyType == typeof(decimal)))
                                    {
                                        value = Convert.ToDouble((decimal)value);
                                    }

                                }
                                catch
                                {
                                }
                                try
                                {

                                    if ((
                                        targetProp.PropertyType == typeof(long?)
                                        ||
                                        targetProp.PropertyType == typeof(long)) && (sourceProp.PropertyType == typeof(decimal?) ||
                                      sourceProp.PropertyType == typeof(decimal)))
                                    {
                                        value = Convert.ToDouble((decimal?)value);
                                    }

                                }
                                catch
                                {


                                }

                                if (targetProp.PropertyType == typeof(int) && sourceProp.PropertyType == typeof(decimal))
                                {
                                    value = Convert.ToInt32((decimal)value);
                                }
                                if (targetProp.PropertyType == typeof(decimal) && sourceProp.PropertyType == typeof(long))
                                {
                                    value = Convert.ToDecimal(value);
                                }
                                if (targetProp.PropertyType == typeof(decimal?) && sourceProp.PropertyType == typeof(long?))
                                {
                                    value = Convert.ToDecimal(value);
                                }
                                if (targetProp.PropertyType == typeof(int?) && sourceProp.PropertyType == typeof(int?))
                                {
                                    value = Convert.ToInt32((int?)value);
                                }
                                if (targetProp.PropertyType == typeof(decimal) && sourceProp.PropertyType == typeof(int))
                                {
                                    value = Convert.ToDecimal((int)value);
                                }
                                if (targetProp.PropertyType == typeof(decimal?) && sourceProp.PropertyType == typeof(int?))
                                {
                                    value = Convert.ToDecimal(value);
                                }
                                if (targetProp.PropertyType == typeof(int?) && sourceProp.PropertyType == typeof(decimal?))
                                {
                                    value = Convert.ToInt32((decimal?)value);
                                }
                                if (targetProp.PropertyType == typeof(int) && sourceProp.PropertyType == typeof(decimal?))
                                {
                                    value = Convert.ToInt32(value);
                                }
                                if (targetProp.PropertyType == typeof(int) && sourceProp.PropertyType == typeof(decimal))
                                {
                                    value = Convert.ToInt32(value);
                                }
                                if (targetProp.PropertyType == typeof(decimal) && (sourceProp.PropertyType == typeof(int?) ||
                                    sourceProp.PropertyType == typeof(int)))
                                {
                                    value = Convert.ToDecimal(value);
                                }
                                if (targetProp.PropertyType == typeof(decimal?) && (sourceProp.PropertyType == typeof(int?) ||
                                    sourceProp.PropertyType == typeof(int)))
                                {
                                    value = Convert.ToDecimal(value);
                                }
                                else if (targetProp.PropertyType == typeof(decimal) && sourceProp.PropertyType == typeof(decimal?))
                                {
                                    value = ((decimal?)value).GetValueOrDefault();
                                }
                                else if (targetProp.PropertyType == typeof(string) && sourceProp.PropertyType == typeof(DateTime))
                                {
                                    value = ((DateTime)value).ToString("yyyy-MM-dd");
                                }
                                else if (targetProp.PropertyType == typeof(string) && sourceProp.PropertyType == typeof(Guid))
                                {
                                    value = value.ToString();
                                }
                                else if (targetProp.PropertyType == typeof(string) && sourceProp.PropertyType == typeof(string))
                                {
                                    value = value.ToString();
                                }
                                else if (targetProp.PropertyType == typeof(Guid) && sourceProp.PropertyType == typeof(string))
                                {
                                    value = Guid.Parse(value.ToString());
                                }
                                else if (targetProp.PropertyType.IsGenericType && targetProp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    // Handle nullable types
                                    var underlyingType = Nullable.GetUnderlyingType(targetProp.PropertyType);
                                    if (underlyingType != null && underlyingType.IsEnum && value is int intValue)
                                    {
                                        value = Enum.ToObject(underlyingType, intValue);
                                    }
                                }
                            }

                            // Set the converted value
                            targetProp.SetValue(target, value);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        // Optionally log or handle the inner exception
                    }
                }

                return target;
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

    }
}