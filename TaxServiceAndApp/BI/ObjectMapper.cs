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
            var target = Activator.CreateInstance<T>();
            var sourceType = source.GetType();
            var targetType = typeof(T);

            foreach (PropertyInfo sourceProp in sourceType.GetProperties())
            {
                var targetProp = targetType.GetProperty(sourceProp.Name, BindingFlags.Public | BindingFlags.Instance);

                if (targetProp == null)
                    targetProp = targetType.GetProperty(sourceProp.Name.ToLower(), BindingFlags.Public | BindingFlags.Instance);

                if (targetProp == null)
                    targetProp = targetType.GetProperty(sourceProp.Name.ToUpper(), BindingFlags.Public | BindingFlags.Instance);
                if (targetProp == null)
                    targetProp = targetType.GetProperty(sourceProp.Name.Replace("_", "").ToLower(), BindingFlags.Public | BindingFlags.Instance);
                if (targetProp == null)
                    targetProp = targetType.GetProperty(sourceProp.Name.Replace("_", "").ToUpper(), BindingFlags.Public | BindingFlags.Instance);


                if (targetProp == null)
                {
                    foreach (var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (prop.Name.Equals(sourceProp.Name.ToLower(), StringComparison.OrdinalIgnoreCase) ||
                            prop.Name.Equals(sourceProp.Name.ToUpper(), StringComparison.OrdinalIgnoreCase) ||
                            prop.Name.Equals(sourceProp.Name.Replace("_", "").ToLower(), StringComparison.OrdinalIgnoreCase) ||
                            prop.Name.Equals(sourceProp.Name.Replace("_", "").ToUpper(), StringComparison.OrdinalIgnoreCase))
                        {
                            targetProp = prop;
                            break;
                        }
                    }
                }

                if (targetProp != null && sourceProp.PropertyType == targetProp.PropertyType)
                {
                    targetProp.SetValue(target, sourceProp.GetValue(source));
                }
            }

            return target;
        }
    }
}