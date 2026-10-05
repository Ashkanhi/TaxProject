using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestTemp
{
    public partial class Form1 : Form
    {

        public class class1
        {
            public int property1 { get; set; }
            public int Property2 { get; set; }
        }
        public class class2
        {
            public int Property1 { get; set; }
            public int property2 { get; set; }
        }
        public Form1()
        {
            InitializeComponent();

            class2 class22 = new class2()
            {
                Property1 = 123,
                property2 = 1234
            };
            class1 class11 = new class1() { };
            class11 = ObjectMapper.TargetObject<class1>
                                (class22);

            MessageBox.Show(class11.Property2.ToString());
        }
    }


    public static class ObjectMapper
    {
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
