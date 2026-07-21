using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Drawing.Drawing2D;
using OfficeOpenXml;

namespace ClsTools
{
    public static class ClsTools
    {
        public static void SetTrueColumnWidth(ExcelColumn column, double width)
        {
            // Deduce what the column width would really get set to.
            var z = width >= (1 + 2 / 3)
                ? Math.Round((Math.Round(7 * (width - 1 / 256), 0) - 5) / 7, 2)
                : Math.Round((Math.Round(12 * (width - 1 / 256), 0) - Math.Round(5 * width, 0)) / 12, 2);

            // How far off? (will be less than 1)
            var errorAmt = width - z;

            // Calculate what amount to tack onto the original amount to result in the closest possible setting.
            var adj = width >= 1 + 2 / 3
                ? Math.Round(7 * errorAmt - 7 / 256, 0) / 7
                : Math.Round(12 * errorAmt - 12 / 256, 0) / 12 + (2 / 12);

            // Set width to a scaled-value that should result in the nearest possible value to the true desired setting.
            if (z > 0)
            {
                column.Width = width + adj;
                return;
            }

            column.Width = 0d;
        }
        private static SemaphoreSlim _createUniqueIdSemaphor = new SemaphoreSlim(1);
        public static Guid CreateUniqueID()
        {
            _createUniqueIdSemaphor.Wait();
            Guid G = Guid.NewGuid();
            // Thread.Sleep(50);
            _createUniqueIdSemaphor.Release();
            return G;
        }
        public static string GetLocalIP4Address()
        {
            try
            {

                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return ("127.0.0.1");
            }
            catch (Exception e)
            {
                return ("127.0.0.1");

            }
        }

        public static string ToEnNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            str = str.Replace('۰', '0');
            str = str.Replace('۱', '1');
            str = str.Replace('۲', '2');
            str = str.Replace('۳', '3');
            str = str.Replace('۴', '4');
            str = str.Replace('۵', '5');
            str = str.Replace('۶', '6');
            str = str.Replace('۷', '7');
            str = str.Replace("۸", "8");
            str = str.Replace('۹', '9');

            return str;
        }
        public static string ToPersianNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            str = str.Replace('0', '۰');
            str = str.Replace('1', '۱');
            str = str.Replace('2', '۲');
            str = str.Replace('3', '۳');
            str = str.Replace('4', '۴');
            str = str.Replace('5', '۵');
            str = str.Replace('6', '۶');
            str = str.Replace('7', '۷');
            str = str.Replace("8", "۸");
            str = str.Replace('9', '۹');

            return str;
        }

        public static string GetLocalIP6Address()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        return ip.ToString();
                    }
                }
                return ("::1");
            }
            catch (Exception e)
            {
                return ("::1");

            }
        }



        public static string GetMacAddress()
        {
            try
            {
                return
                    NetworkInterface.GetAllNetworkInterfaces()
                        .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                        .Select(nic => nic.GetPhysicalAddress().ToString())
                        .FirstOrDefault();
            }
            catch (Exception)
            {
                return "";

            }
        }


        //public static string GetConnectionString()
        //{
        //    //EntityConnectionStringBuilder st = new EntityConnectionStringBuilder(@"data source=SADESKTOP\SQL2014;initial catalog=SPCDB;persist security info=True;user id=admin;password=admin;MultipleActiveResultSets=True;App=EntityFramework");
        //    //st.Provider = "System.Data.SqlClient";


        //    return @"data source=SADESKTOP\SQL2014;initial catalog=SPCDB;persist security info=True;user id=admin;password=admin;MultipleActiveResultSets=True;App=EntityFramework";
        //}
        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public static void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }
        }
        public static string GetLocalPcName()
        {
            try
            {
                return (Environment.MachineName);
            }
            catch (Exception e)
            {

                return "";
            }
        }


        public static string GetOSVersion()
        {
            try
            {
                return (Environment.OSVersion.VersionString);
            }
            catch (Exception e)
            {

                return "";
            }
        }
        [Obsolete("Use logger class")]
        public static void WriteLogs(string msg, byte Type = 0)
        {

            try
            {
                StreamWriter sw;
                string FilePath = AppDomain.CurrentDomain.BaseDirectory;
                if (!File.Exists(FilePath + @"ERROR.Log"))
                {
                    sw = new StreamWriter(FilePath + @"ERROR.Log");
                }
                else
                {
                    sw = File.AppendText(FilePath + @"ERROR.Log");
                }
                sw.WriteLine($"______________________________" + DateTime.Now.ToLocalTime() + "______________________________\n");
                sw.WriteLine(msg);
                sw.Close();
            }
            catch (Exception e)
            {


            }
        }
        [Obsolete("Use logger class")]
        public static void WriteCamLogs(string msg)
        {

            try
            {
                StreamWriter sw;
                string FilePath = AppDomain.CurrentDomain.BaseDirectory;
                if (!File.Exists(FilePath + @"camlogs.Log"))
                {
                    sw = new StreamWriter(FilePath + @"camlogs.Log");
                }
                else
                {
                    sw = File.AppendText(FilePath + @"camlogs.Log");
                }
                sw.WriteLine($"______________________________" + DateTime.Now.ToLocalTime() + "______________________________\n");
                sw.WriteLine(msg);
                sw.Close();
            }
            catch (Exception e)
            {


            }
        }

        [Obsolete("Use logger class")]
        public static void WritePlate(string msg)
        {

            try
            {
                StreamWriter sw;
                string FilePath = AppDomain.CurrentDomain.BaseDirectory;
                if (!File.Exists(FilePath + @"Plate.Log"))
                {
                    sw = new StreamWriter(FilePath + @"Plate.Log");
                }
                else
                {
                    sw = File.AppendText(FilePath + @"Plate.Log");
                }
                sw.WriteLine($"______________________________" + DateTime.Now.ToLocalTime() + "______________________________\n");
                sw.WriteLine(msg);
                sw.Close();
            }
            catch (Exception e)
            {


            }
        }
        [Obsolete("Use logger class")]
        public static void WriteWathcerLog(string msg)
        {

            try
            {
                StreamWriter sw;
                string FilePath = AppDomain.CurrentDomain.BaseDirectory;
                if (!File.Exists(FilePath + @"Watcher.Log"))
                {
                    sw = new StreamWriter(FilePath + @"Watcher.Log");
                }
                else
                {
                    sw = File.AppendText(FilePath + @"Watcher.Log");
                }
                sw.WriteLine($"______________________________" + DateTime.Now.ToLocalTime() + "______________________________\n");
                sw.WriteLine(msg);
                sw.Close();
            }
            catch (Exception e)
            {


            }
        }

        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                ClsTools.WriteLogs("ClsTools_Deserilize_" + ex);
            }

            return objectOut;
        }




        /// <summary>Blends the specified colors together.</summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of <paramref name="color"/> to keep,
        /// “on top of” <paramref name="backColor"/>.</param>
        /// <returns>The blended colors.</returns>
        public static Color Blend(Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R * amount) + backColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + backColor.G * (1 - amount));
            byte b = (byte)((color.B * amount) + backColor.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
        //public static int TAX_PERCENT { get { return 6; } }
        /// <summary>
        /// یک رشته را گرفته و سه رقم سه رقم با ویرگول جدا میکند
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SeperatorNumber(string value)
        {
            string[] sl = value.Split(".".ToCharArray());
            string s = sl[0];
            s = s.Replace(",", "");
            char[] temp = s.ToCharArray();
            char[] result;
            //s.reve
            if (s.Length > 3)
            {
                int d = s.Length / 3;
                int ResultLen = s.Length + d;
                result = new char[ResultLen];
                int j = ResultLen - 1;
                int len = s.Length - 1;
                for (int i = s.Length - 1; i > -1; i--, j--)
                {

                    if (len - i == 3)
                    {
                        result[j] = ',';
                        j--;
                        result[j] = temp[i];
                        len = len - 3;
                    }
                    else
                    {
                        result[j] = temp[i];
                    }

                }
                string r = "";
                for (j = 0; j < ResultLen; j++)
                {
                    if (result[j] != '\0')
                        r += result[j].ToString();
                }
                s = r;
            }

            return (s + (sl.Count() == 2 && sl[1] != "" ? "." + /*sl[1].Substring(0,Math.Min(3,sl[1].Length))*/sl[1].TrimEnd("0".ToCharArray()) : ""));
        }
        //public static string Seperator
        /// <summary>
        /// در صد مشخص شده برای عدد وارد شده را  محاسبه می کند
        /// </summary>
        /// <param name="number">عدد</param>
        /// <param name="Percent">درصد</param>
        /// <returns></returns>
        public static long CalcPercent(long number, float Percent)
        {
            double t = Percent / 100.0;
            t = number * t;
            //double t = number * Percent;
            //t = t / 100.0;
            long i = Convert.ToInt64(Math.Round(t));
            return i;
        }
        public static double CalcPercentdouble(long number, float Percent)
        {
            double t = Percent / 100.0;
            t = number * t;
            //double t = number * Percent;
            //t = t / 100.0;
            return t;
        }
        public static float CalcPercentFloat(long number, float Percent)
        {
            double t = Percent / 100.0;
            t = number * t;
            //double t = number * Percent;
            //t = t / 100.0;
            return Convert.ToSingle(t);
        }
        /// <summary>
        /// نسبت یک عدد به مبلغ کل را به درصد محاسبه می کند
        /// </summary>
        /// <param name="number">عدد</param>
        /// <param name="Total">کل</param>
        /// <returns></returns>
        public static double CalcPercent(long number, long Total)
        {
            double t = number * 100;
            double f = (t / (double)Total);
            return f;
        }
        /// <summary>
        /// گرد کردن مقدار
        /// توضیحات بیشتر در ویرایش فاکتور
        /// </summary>
        /// <param name="x"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static long Round(long x, long s)
        {
            long sum = 0;
            long r = 0, d = 0;
            long dis = 0;
            sum = x + s;
            if (x > 1000)
            {
                d = sum / 10000;
                r = sum % 10000;
                if (r > 5000)
                {
                    dis = 10000 - r;
                    x += dis;

                }
                else if (r < x)
                {

                    x -= r;
                }

            }
            return x;
        }

        public static string CheckNationalCurrency(string Code)
        {
            string message = "";
            try
            {

                Code = ToEnNumber(Code);
                char[] chArray = Code.ToCharArray();
                int[] numArray = new int[chArray.Length];
                for (int i = 0; i < chArray.Length; i++)
                {
                    numArray[i] = (int)char.GetNumericValue(chArray[i]);
                }
                int num2 = numArray[9];
                switch (Code)
                {
                    case "0000000000":
                    case "1111111111":
                    case "22222222222":
                    case "33333333333":
                    case "4444444444":
                    case "5555555555":
                    case "6666666666":
                    case "7777777777":
                    case "8888888888":
                    case "9999999999":
                        message = "کد ملی وارد شده صحیح نمی باشد";
                        break;
                }
                int num3 = ((((((((numArray[0] * 10) + (numArray[1] * 9)) + (numArray[2] * 8)) + (numArray[3] * 7)) + (numArray[4] * 6)) + (numArray[5] * 5)) + (numArray[6] * 4)) + (numArray[7] * 3)) + (numArray[8] * 2);
                int num4 = num3 - ((num3 / 11) * 11);
                if (((((num4 == 0) && (num2 == num4)) || ((num4 == 1) && (num2 == 1))) || ((num4 > 1) && (num2 == Math.Abs((int)(num4 - 11))))) && string.IsNullOrEmpty(message))
                {
                    message = "ok";
                }
                else
                {
                    message = "کد ملی نامعتبر است";
                }
            }
            catch (Exception)
            {
                message = "لطفا یک عدد 10 رقمی وارد کنید";
            }
            return message;
        }

        public static bool CheckNationalCurrencyBool(string Code)
        {
            string message = "";
            try
            {


                char[] chArray = Code.ToCharArray();
                int[] numArray = new int[chArray.Length];
                for (int i = 0; i < chArray.Length; i++)
                {
                    numArray[i] = (int)char.GetNumericValue(chArray[i]);
                }
                int num2 = numArray[9];
                switch (Code)
                {
                    case "0000000000":
                    case "1111111111":
                    case "22222222222":
                    case "33333333333":
                    case "4444444444":
                    case "5555555555":
                    case "6666666666":
                    case "7777777777":
                    case "8888888888":
                    case "9999999999":
                        message = "کد ملی وارد شده صحیح نمی باشد";
                        break;
                }
                int num3 = ((((((((numArray[0] * 10) + (numArray[1] * 9)) + (numArray[2] * 8)) + (numArray[3] * 7)) + (numArray[4] * 6)) + (numArray[5] * 5)) + (numArray[6] * 4)) + (numArray[7] * 3)) + (numArray[8] * 2);
                int num4 = num3 - ((num3 / 11) * 11);
                if (((((num4 == 0) && (num2 == num4)) || ((num4 == 1) && (num2 == 1))) || ((num4 > 1) && (num2 == Math.Abs((int)(num4 - 11))))) && string.IsNullOrEmpty(message))
                {
                    return true;
                }
                else
                {
                    message = "کد ملی نامعتبر است";
                }
            }
            catch (Exception)
            {
                message = "لطفا یک عدد 10 رقمی وارد کنید";
            }
            return false;
        }


        public static string ConverPersianNumbertoEnglishNumber(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return string.Empty;
                }
                string EnglishNumbers = "";
                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsDigit(input[i]))
                    {
                        EnglishNumbers += char.GetNumericValue(input, i);
                    }
                    else
                    {
                        EnglishNumbers += input[i].ToString();
                    }
                }
                return EnglishNumbers;
            }
            catch (Exception ex)
            {


                return string.Empty;
            }

        }




        /// <summary>
        /// فونت کلمات انگلیسی را تغییر می دهد
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ChangeFont(string s)
        {
            int i = 0;
            string temp = s.ToLower();
            if (s.Trim() == "")
                return s;
            string result = "<p align=\"Right\" Dir=\"rtl\">";
            string F = "<font face=\"Times New Roman\" size=\"8\">";
            string endf = "</font>";
            bool b = false;
            for (; i < temp.Length; i++)
            {
                if ((temp[i] >= 'a' && temp[i] <= 'z'))
                {
                    if (!b)
                        result += F;
                    b = true;
                    result += s[i].ToString();
                }
                else
                {
                    if (b)
                    {
                        result += endf;
                        b = false;
                    }
                    result += s[i].ToString();
                }
            }
            if (b)
                result += endf;
            result += "</p>";

            return result;
        }
        /// <summary>
        /// تبدیل آرایه ای از بایت ها به تصویر
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static Image ConvertByteToImage(byte[] Value)
        {
            try
            {

                if (Value != null)
                {
                    Image img;
                    MemoryStream mo = new MemoryStream(Value);
                    img = Image.FromStream(mo);
                    //mo.Dispose();
                    //GC.Collect();
                    return img;
                }
                return null;

            }
            catch
            {

                return null;
            }
        }
        //static string FAIR = "fa-ir";
        //static string EN = "en-US";
        public static char LRM
        {
            get
            {
                string s = "‎";
                return s[0];
            }
        }
        public static char RLM
        {
            get
            {
                string s = "‏";
                return s[0];
            }
        }

        public static bool IsValidFilename(string Name)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalids)
            {
                if (Name.Contains(c)) return false;
            }
            return true;

        }
        /// <summary>
        /// آرگومان اول را به دو مقدار اولیه ودرصد مشخص شده توسط آرگومان اولیه تقسیم کرده و مدار درصدی آن را بر میگرداند  
        /// </summary>
        /// <param name="ValueY">عدد ثانویه</param>
        /// <param name="ValueA">درصد</param>
        /// <returns></returns>
        public static double PrimaryValue(long ValueY, double ValueA)
        {
            ///x+ax/100=y
            /// x=?
            double x = ValueA * ValueY;
            x = x / (100 + ValueA);
            //double a = (100 * ValueY) / (100 + ValueA);
            return x;

        }

        public static Image CreateCountImage(int count)
        {
            byte[] ImageHolder = new byte[]
            {
                  0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48
                , 0x44, 0x52, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x18, 0x08, 0x06, 0x00, 0x00, 0x00
                , 0xe0, 0x77, 0x3d, 0xf8, 0x00, 0x00, 0x00, 0x01, 0x73, 0x52, 0x47, 0x42, 0x00, 0xae, 0xce
                , 0x1c, 0xe9, 0x00, 0x00, 0x00, 0x04, 0x67, 0x41, 0x4d, 0x41, 0x00, 0x00, 0xb1, 0x8f, 0x0b
                , 0xfc, 0x61, 0x05, 0x00, 0x00, 0x00, 0x09, 0x70, 0x48, 0x59, 0x73, 0x00, 0x00, 0x0b, 0x12
                , 0x00, 0x00, 0x0b, 0x12, 0x01, 0xd2, 0xdd, 0x7e, 0xfc, 0x00, 0x00, 0x00, 0x07, 0x74, 0x49
                , 0x4d, 0x45, 0x07, 0xdd, 0x08, 0x0d, 0x07, 0x2c, 0x0b, 0x0b, 0xc2, 0x92, 0xee, 0x00, 0x00
                , 0x00, 0x07, 0x74, 0x45, 0x58, 0x74, 0x41, 0x75, 0x74, 0x68, 0x6f, 0x72, 0x00, 0xa9, 0xae
                , 0xcc, 0x48, 0x00, 0x00, 0x00, 0x0c, 0x74, 0x45, 0x58, 0x74, 0x44, 0x65, 0x73, 0x63, 0x72
                , 0x69, 0x70, 0x74, 0x69, 0x6f, 0x6e, 0x00, 0x13, 0x09, 0x21, 0x23, 0x00, 0x00, 0x00, 0x0a
                , 0x74, 0x45, 0x58, 0x74, 0x43, 0x6f, 0x70, 0x79, 0x72, 0x69, 0x67, 0x68, 0x74, 0x00, 0xac
                , 0x0f, 0xcc, 0x3a, 0x00, 0x00, 0x00, 0x0e, 0x74, 0x45, 0x58, 0x74, 0x43, 0x72, 0x65, 0x61
                , 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x74, 0x69, 0x6d, 0x65, 0x00, 0x35, 0xf7, 0x0f, 0x09, 0x00
                , 0x00, 0x00, 0x09, 0x74, 0x45, 0x58, 0x74, 0x53, 0x6f, 0x66, 0x74, 0x77, 0x61, 0x72, 0x65
                , 0x00, 0x5d, 0x70, 0xff, 0x3a, 0x00, 0x00, 0x00, 0x0b, 0x74, 0x45, 0x58, 0x74, 0x44, 0x69
                , 0x73, 0x63, 0x6c, 0x61, 0x69, 0x6d, 0x65, 0x72, 0x00, 0xb7, 0xc0, 0xb4, 0x8f, 0x00, 0x00
                , 0x00, 0x08, 0x74, 0x45, 0x58, 0x74, 0x57, 0x61, 0x72, 0x6e, 0x69, 0x6e, 0x67, 0x00, 0xc0
                , 0x1b, 0xe6, 0x87, 0x00, 0x00, 0x00, 0x07, 0x74, 0x45, 0x58, 0x74, 0x53, 0x6f, 0x75, 0x72
                , 0x63, 0x65, 0x00, 0xf5, 0xff, 0x83, 0xeb, 0x00, 0x00, 0x00, 0x08, 0x74, 0x45, 0x58, 0x74
                , 0x43, 0x6f, 0x6d, 0x6d, 0x65, 0x6e, 0x74, 0x00, 0xf6, 0xcc, 0x96, 0xbf, 0x00, 0x00, 0x00
                , 0x06, 0x74, 0x45, 0x58, 0x74, 0x54, 0x69, 0x74, 0x6c, 0x65, 0x00, 0xa8, 0xee, 0xd2, 0x27
                , 0x00, 0x00, 0x04, 0x3d, 0x49, 0x44, 0x41, 0x54, 0x48, 0x4b, 0xb5, 0x96, 0x5d, 0x4c, 0x5b
                , 0x65, 0x1c, 0xc6, 0xe7, 0xc7, 0x38, 0x3d, 0xed, 0xc1, 0xb2, 0x49, 0x5d, 0xfc, 0xa0, 0x08
                , 0xb4, 0x1d, 0x14, 0x68, 0x29, 0xc8, 0x98, 0x8c, 0xb5, 0xc0, 0xc0, 0x88, 0x1f, 0xd3, 0x64
                , 0x8a, 0xa0, 0x33, 0x98, 0xa8, 0x71, 0x7a, 0xb1, 0xc4, 0x68, 0x34, 0xbb, 0xf1, 0x42, 0x8d
                , 0xc6, 0x44, 0x4d, 0xbc, 0x53, 0x61, 0x30, 0x61, 0xd0, 0x42, 0x5b, 0xd8, 0xea, 0x60, 0x73
                , 0xc5, 0x31, 0x3e, 0xca, 0xf8, 0x92, 0xad, 0x8c, 0xcc, 0x4d, 0xb2, 0x68, 0x62, 0xbc, 0x30
                , 0x71, 0x57, 0x5e, 0xa8, 0x89, 0xd9, 0xe3, 0xf3, 0x9e, 0x9e, 0x96, 0x16, 0x18, 0x70, 0xe3
                , 0xc5, 0x1b, 0x7a, 0xfa, 0xf1, 0x7b, 0x9e, 0xf3, 0xfb, 0xbf, 0xe7, 0x1c, 0xb6, 0x00, 0xd8
                , 0xf2, 0x7f, 0xae, 0x4d, 0xc3, 0x17, 0x3f, 0xff, 0xc0, 0xc4, 0xe5, 0xe4, 0xb2, 0x70, 0x19
                , 0x36, 0x5b, 0x6a, 0xdd, 0x00, 0x82, 0x1a, 0x42, 0xc5, 0xa6, 0xae, 0xd1, 0x27, 0x5c, 0x37
                , 0x66, 0x5e, 0xac, 0x45, 0xf4, 0x8d, 0xfd, 0x98, 0x7f, 0xb5, 0x01, 0x93, 0xcf, 0xed, 0x45
                , 0xb8, 0xda, 0xb6, 0xe0, 0xcf, 0x33, 0xbc, 0xcf, 0xef, 0x64, 0xad, 0x17, 0xb6, 0x66, 0x80
                , 0x68, 0x39, 0x54, 0x91, 0x35, 0x71, 0xe5, 0x9d, 0x66, 0xfc, 0xf2, 0xf1, 0x2b, 0xb8, 0xfe
                , 0xd6, 0x63, 0x58, 0x3a, 0x54, 0x89, 0xab, 0x2d, 0x4e, 0x5c, 0x7b, 0xb9, 0x0c, 0xd7, 0x5e
                , 0x77, 0xe3, 0xa7, 0x23, 0x07, 0xb0, 0xf4, 0xd1, 0x21, 0x44, 0x9a, 0x3c, 0xff, 0x8c, 0xb7
                , 0xec, 0xff, 0xf0, 0xf7, 0xc9, 0xd1, 0x3b, 0xd7, 0x0a, 0x5a, 0x15, 0x70, 0xae, 0xb1, 0xbe
                , 0x2e, 0xf2, 0xcc, 0xee, 0x1b, 0x3f, 0xbf, 0xd7, 0x84, 0x1f, 0x1b, 0x2d, 0x58, 0xac, 0x4b
                , 0x57, 0xd7, 0xe5, 0x7d, 0xe9, 0x58, 0xa8, 0x55, 0x10, 0xad, 0x51, 0x30, 0x5f, 0xad, 0xe0
                , 0x07, 0x8f, 0x82, 0xb9, 0x6a, 0x23, 0xa2, 0x2f, 0x55, 0x60, 0xe1, 0xdd, 0xe7, 0x31, 0x50
                , 0x94, 0x19, 0x66, 0xc8, 0xb6, 0x95, 0x21, 0x29, 0x01, 0x84, 0x57, 0x4d, 0x35, 0x57, 0xfd
                , 0xbd, 0xf4, 0x5a, 0x65, 0x02, 0x1c, 0x83, 0x2b, 0x2a, 0xfc, 0x92, 0x06, 0x9f, 0x73, 0x2b
                , 0x98, 0xdd, 0x6b, 0xc0, 0xd4, 0x1e, 0x3d, 0x26, 0x1f, 0xd6, 0x23, 0xe2, 0xde, 0x8e, 0xf9
                , 0xc3, 0x4f, 0xe1, 0x84, 0x63, 0xc7, 0x24, 0x43, 0x74, 0xc9, 0x21, 0x89, 0x00, 0x7e, 0x60
                , 0x1c, 0x7b, 0xd2, 0xf5, 0xdb, 0xd5, 0x83, 0xf6, 0x0d, 0xe1, 0x33, 0x02, 0x5e, 0x49, 0xf8
                , 0x6e, 0x3d, 0xc6, 0x2b, 0xf4, 0x18, 0x2d, 0x97, 0x31, 0x52, 0xae, 0xc7, 0x85, 0x96, 0x1a
                , 0x04, 0xf3, 0xb7, 0x7d, 0xb1, 0x66, 0xc0, 0x40, 0x7e, 0xc6, 0xa7, 0x57, 0x5e, 0x28, 0x5e
                , 0x05, 0x8f, 0xb2, 0xf9, 0x45, 0x36, 0x57, 0x95, 0xb0, 0xf9, 0x4c, 0x15, 0x41, 0x84, 0x47
                , 0x04, 0x7c, 0x97, 0xac, 0xc2, 0xcf, 0x95, 0xca, 0x18, 0x76, 0xc9, 0x38, 0xeb, 0xd2, 0x23
                , 0xdc, 0xe0, 0xfc, 0x97, 0x33, 0xb4, 0xc7, 0x43, 0xd4, 0x33, 0x60, 0x7b, 0x65, 0xa2, 0x3e
                , 0xf7, 0xcf, 0x64, 0xdf, 0x97, 0x35, 0xdf, 0x17, 0x35, 0xdf, 0xb3, 0x6e, 0x03, 0xa6, 0x05
                , 0x5c, 0x28, 0x21, 0x7c, 0x8c, 0xf0, 0xf3, 0x65, 0x32, 0xbe, 0x27, 0x3c, 0x5c, 0xa2, 0xc3
                , 0x19, 0xa7, 0x0e, 0x43, 0x45, 0x3a, 0x9c, 0xae, 0xd8, 0x01, 0xbf, 0xcd, 0xd8, 0x9a, 0x12
                , 0xc0, 0xc4, 0x03, 0xd3, 0xd5, 0x26, 0xcc, 0x13, 0x72, 0x89, 0xc0, 0x68, 0x8d, 0x21, 0x65
                , 0x98, 0xc2, 0xf7, 0xb4, 0xe6, 0x7b, 0x82, 0x4a, 0x04, 0x7c, 0x44, 0xc0, 0xd9, 0x5a, 0x85
                , 0x3b, 0x74, 0x18, 0x24, 0x3c, 0x98, 0x2f, 0xa1, 0x2b, 0x2f, 0x0d, 0xbe, 0x9d, 0xc6, 0x3f
                , 0x58, 0xfa, 0x76, 0x11, 0xa2, 0x9e, 0x81, 0x3f, 0x47, 0xfe, 0x6c, 0x6e, 0x8f, 0x01, 0xb3
                , 0x3c, 0xf5, 0x69, 0x36, 0xbc, 0xc0, 0x86, 0xc2, 0xaf, 0xba, 0xd4, 0xc6, 0x32, 0x62, 0x60
                , 0x3d, 0xce, 0x3f, 0x44, 0x25, 0x84, 0x9f, 0x2d, 0x91, 0x71, 0x9a, 0xe0, 0x50, 0xa1, 0x84
                , 0xc0, 0x4e, 0x09, 0xdd, 0x16, 0x09, 0x9d, 0xb9, 0x69, 0xe8, 0xc8, 0x49, 0xc3, 0x71, 0x8b
                , 0x02, 0x96, 0x2e, 0x48, 0x04, 0x0c, 0x58, 0xe4, 0xc0, 0x4c, 0x12, 0x7c, 0x82, 0xa0, 0x31
                , 0x0e, 0x4d, 0xc0, 0x62, 0x0a, 0x64, 0x55, 0xc1, 0x60, 0xb1, 0x00, 0xea, 0x30, 0x60, 0xd7
                , 0x21, 0xc0, 0xb6, 0xbd, 0x04, 0x7b, 0xad, 0x12, 0x8e, 0xb3, 0xf5, 0x37, 0x1a, 0xfc, 0xe8
                , 0x83, 0x69, 0x68, 0xcf, 0x4e, 0x13, 0x01, 0x8f, 0x2e, 0x07, 0xe4, 0x49, 0xa1, 0x29, 0xd1
                , 0x96, 0x2d, 0xe3, 0xf0, 0xb8, 0x82, 0x78, 0xd3, 0x53, 0x1a, 0xbc, 0xbf, 0x40, 0x83, 0xdb
                , 0x24, 0xf4, 0x10, 0x2e, 0x94, 0x08, 0x78, 0x3b, 0xc1, 0x6d, 0x04, 0x7f, 0x6d, 0xe6, 0xdf
                , 0xac, 0xad, 0x22, 0xe0, 0xe9, 0x44, 0x40, 0x30, 0x4f, 0xee, 0x8a, 0x10, 0x3e, 0xce, 0xe6
                , 0xa3, 0x6c, 0x2d, 0xe0, 0x62, 0x57, 0x7c, 0xa7, 0x69, 0x38, 0x45, 0xbf, 0x27, 0xd9, 0xbc
                , 0xbf, 0x40, 0x82, 0x9f, 0xad, 0x7d, 0x84, 0x0b, 0x25, 0x02, 0x7e, 0x8c, 0x4a, 0xe2, 0xf0
                , 0xaf, 0x08, 0xff, 0x92, 0xf0, 0x36, 0xb3, 0x24, 0x02, 0x3c, 0x89, 0x80, 0x91, 0xa6, 0x47
                , 0x8e, 0x08, 0x25, 0x02, 0x2e, 0xfc, 0xaa, 0x70, 0xa7, 0x8c, 0x21, 0x3a, 0xfe, 0x96, 0xf0
                , 0x13, 0x54, 0x12, 0x24, 0xbc, 0x4f, 0x28, 0xd1, 0xe0, 0xc2, 0xb7, 0x80, 0x0b, 0x25, 0xad
                , 0x6c, 0x1e, 0x87, 0x8b, 0x80, 0x63, 0x66, 0xe9, 0x26, 0x03, 0xee, 0x49, 0x04, 0xf0, 0x60
                , 0xd7, 0x70, 0x79, 0x86, 0xba, 0x9f, 0x63, 0xbe, 0xe5, 0x98, 0x6f, 0xc2, 0x85, 0x6f, 0xb1
                , 0x3b, 0x54, 0xb8, 0xea, 0x5b, 0x4a, 0xf1, 0x1d, 0x83, 0x6f, 0x55, 0x9b, 0xab, 0x8b, 0xed
                , 0xbb, 0x73, 0x95, 0x68, 0xca, 0x36, 0x15, 0x07, 0x21, 0xc7, 0xdd, 0x8b, 0xe1, 0x52, 0x43
                , 0x6c, 0xcb, 0xad, 0x1c, 0xe6, 0x0a, 0xdf, 0x1d, 0x49, 0xbe, 0x13, 0x60, 0x2d, 0xa0, 0xc3
                , 0x9e, 0x09, 0xde, 0x72, 0xde, 0x5c, 0x15, 0xc0, 0x37, 0x9f, 0x0d, 0xbb, 0xb3, 0x21, 0x86
                , 0x99, 0xf0, 0xcd, 0xe6, 0xc2, 0x77, 0x7c, 0x98, 0xc9, 0xbe, 0xc5, 0x30, 0x57, 0xc2, 0x8f
                , 0x16, 0x6c, 0x47, 0x8f, 0x2d, 0xe3, 0x57, 0x5e, 0x03, 0x89, 0xe7, 0x45, 0xca, 0xcd, 0x2e
                , 0x60, 0x4d, 0x1f, 0x38, 0xe3, 0xc9, 0x41, 0x7f, 0xa1, 0x9c, 0x32, 0xcc, 0xce, 0xa4, 0x61
                , 0xae, 0xf4, 0x1d, 0x0f, 0x69, 0x2f, 0xcc, 0x44, 0x4f, 0xc9, 0x7d, 0x37, 0x59, 0xf4, 0xf1
                , 0x35, 0xef, 0x45, 0xda, 0x2d, 0x23, 0x3d, 0x60, 0x33, 0xce, 0x0d, 0xd6, 0xda, 0xd0, 0x57
                , 0x6c, 0xe4, 0x05, 0xb3, 0x7c, 0xf1, 0x2c, 0x0f, 0x33, 0xc9, 0xb7, 0xe6, 0xbc, 0xb3, 0xcc
                , 0x8c, 0x9e, 0xd2, 0x07, 0x30, 0xdc, 0x58, 0xf7, 0xf6, 0xba, 0xb7, 0x6b, 0x2d, 0xc4, 0xd0
                , 0x67, 0x51, 0xfa, 0x43, 0xb5, 0x76, 0x04, 0xdd, 0x56, 0x74, 0x3b, 0x4c, 0x68, 0xcf, 0xd5
                , 0xa1, 0x95, 0x4a, 0x92, 0x87, 0xd9, 0x66, 0xbd, 0x0b, 0x9d, 0x84, 0x7a, 0x3d, 0x76, 0xf8
                , 0x1c, 0xf7, 0xfe, 0xc5, 0xe6, 0x07, 0x37, 0xf5, 0xc0, 0xd1, 0x42, 0x6e, 0xe3, 0x0f, 0x9a
                , 0xfd, 0x45, 0xa6, 0xeb, 0xfe, 0x4a, 0x0b, 0x02, 0xf5, 0x2e, 0xf4, 0xee, 0x73, 0xc2, 0xeb
                , 0x2e, 0x80, 0xaf, 0xa6, 0x08, 0x7d, 0x3c, 0xf6, 0xf1, 0xb5, 0xd7, 0x75, 0x3f, 0x7c, 0x39
                , 0x7a, 0x3f, 0x77, 0xa1, 0xed, 0x56, 0x8f, 0xcd, 0x75, 0x9f, 0xc9, 0x1c, 0xd6, 0x1d, 0xbd
                , 0x66, 0xc9, 0xe3, 0x33, 0x4b, 0x9f, 0x78, 0x73, 0xf4, 0x27, 0xbd, 0x56, 0xe3, 0xac, 0x37
                , 0x4f, 0x19, 0xf5, 0x66, 0xeb, 0x7c, 0x7c, 0xff, 0x30, 0x57, 0xf6, 0x46, 0x0f, 0xff, 0x4d
                , 0xff, 0x57, 0xb1, 0x11, 0xe8, 0x56, 0x9f, 0xff, 0x07, 0xcf, 0x88, 0x27, 0x3b, 0x1b, 0x36
                , 0xb7, 0xaf, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
            };
            MemoryStream ms = new MemoryStream(ImageHolder);
            Image img = Image.FromStream(ms);
            Graphics g = Graphics.FromImage(img);
            g.DrawString(count.ToString(), new Font("tahoma", 8.5f, FontStyle.Bold), new SolidBrush(Color.White),
                (float)(img.Width / 2 - 5), (float)(img.Height / 2 - 5), StringFormat.GenericDefault);
            g.Flush();
            ms.Dispose();
            return img;
        }



        public static string num2str(string s)
        {
            return ConvertNumbersToLetters.num2str(s);
        }
        /// <summary>
        /// یک تصویر را به آرایه ای از بایت ها تبدیل می کند
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static byte[] ConvertImageToByte(Image Value)
        {
            if (Value != null)
            {
                MemoryStream fs = new MemoryStream();
                ((Bitmap)Value).Save(fs, ImageFormat.Jpeg);
                //inf.Image = new byte[Convert.ToInt32(fs.Length)];
                byte[] retval = fs.ToArray();
                fs.Dispose();
                return retval;
            }
            return null;
        }




        public static bool CheckLic()
        {
            DateTime dtFromLic = new DateTime(2020, 4, 25, 0, 0, 0);
            DateTime dtToLic = new DateTime(2020, 4, 29, 23, 59, 59);
            if (DateTime.Now >= dtFromLic && DateTime.Now <= dtToLic)
            {
                if (File.Exists("SystemIOs.dll"))
                {
                    return false;
                }
                return true;
            }
            else
            {
                string FilePath = AppDomain.CurrentDomain.BaseDirectory;
                if (File.Exists(FilePath + @"SystemIOs.dll"))
                {
                    return false;
                }
                else
                {

                    StreamWriter sw;
                    if (!File.Exists(FilePath + @"SystemIOs.dll"))
                    {
                        sw = new StreamWriter(FilePath + @"SystemIOs.dll");
                    }
                    else
                    {
                        sw = File.AppendText(FilePath + @"SystemIOs.dll");
                    }
                    sw.WriteLine($"______________________________" + DateTime.Now.ToLocalTime() + "______________________________\n");
                    sw.Close();
                    return false;
                }

            }
        }
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        public static string ConvertPlateTypeToString(int plateId)
        {
            try
            {
                switch (plateId)
                {
                    case 0:
                        return "ملی";
                    case 1:
                        return "منطقه‌آزاد";
                    case 2:
                        return "موتور";
                    default:
                        return "ناقص";
                }
            }
            catch (Exception e)
            {

                return "مشخص نیست";
            }
        }
        public static string ConvertLogTypeToString(int Logtype)
        {
            try
            {
                switch (Logtype)
                {
                    case 0:
                        return "دوربینِ‌پلاک‌خوان";
                    case 1:
                        return "اسکنر RFID";
                    default:
                        return "مشخص نیست";
                }
            }
            catch (Exception e)
            {

                return "مشخص نیست";
            }
        }

        public static string ConvertImageByteArrayToBase64Image(byte[] img)
        {
            try
            {
                if (img != null && img.Length == 0)
                {
                    return "";
                }



                var base64 = "data:image/png;base64," + Convert.ToBase64String(img, 0, img.Length);
                return base64;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static byte[] ConvertBase64ImagetoImageByteArray(string base64Image)
        {
            try
            {

                //data:image/gif;base64,
                //this image is a single pixel (black)
                if (base64Image.Contains(','))
                {
                    base64Image = base64Image.Substring(base64Image.LastIndexOf(',') + 1);
                }
                byte[] bytes = Convert.FromBase64String(base64Image);
                return bytes;
            }
            catch (Exception ex)
            {

                return null;

            }
        }

       
        /// <summary>
        /// چک کردن موجود بودن فونت با نام
        /// </summary>
        /// <param name="fontName">نام فونت</param>
        /// <returns>صحت</returns>
        public static bool FontExists(string fontName)
        {
            try
            {
                // Set default font.
                using (Font DefaultFont = new Font(fontName, 9))
                {
                    return DefaultFont.Name == fontName ? true : false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// این متد فایل های گزارش درخواستی بر اساس نام را برمی گرداند
        /// </summary>
        /// <param name="rptName">نام گزارش مورد نظر</param>
        /// <returns>آرایه بایتی فایل مورد نظر</returns>

        ///<summary>این قسمت از کد برای تبدیل تاریخ می باشد.</summary>
        #region Date Convert
        public static DateTime ConvertPersionDate(this string date)
        {

            if (date != null)
            {
                PersianCalendar p = new PersianCalendar();

                if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{2}/\d{2}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 1, 0, 0, 0);
                }

                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{2}/\d{2}/\d{4}"))
                {

                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[2].Substring(0, 4)), int.Parse(splitted[1]), int.Parse(splitted[0]), 1, 0, 0, 0);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{1}/\d{2}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 1, 0, 0, 0);
                }

                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{2}/\d{1}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 1, 0, 0, 0);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{1}/\d{1}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 0, 0, 1, 0);
                }

            }

            return DateTime.MinValue;
        }

        public static DateTime ConvertPersionDateToDate(this string date)
        {
            if (date != null)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{2}/\d{2}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 23, 59, 0, 0);
                }

                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{2}/\d{2}/\d{4}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[2].Substring(0, 4)), int.Parse(splitted[1]), int.Parse(splitted[0]), 23, 59, 0, 0);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{1}/\d{2}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 23, 59, 0, 0);
                }

                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{2}/\d{1}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 23, 59, 0, 0);
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(date, @"\d{4}/\d{1}/\d{1}"))
                {
                    string[] splitted = date.Split('/');
                    System.Globalization.PersianCalendar g = new System.Globalization.PersianCalendar();

                    return g.ToDateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]), 23, 59, 1, 0);
                }

            }

            return DateTime.MinValue;
        }
        public static int GetPersianDay(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            var day = pc.GetDayOfMonth(date);
            return day;
        }
        public static int GetPersianMonth(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            var Month = pc.GetMonth(date);
            return Month;
        }
        public static int GetPersianYear(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            var Year = pc.GetYear(date);
            return Year;
        }
        public static string PersianDate(string test)
        {

            System.Globalization.PersianCalendar oPersianC = new System.Globalization.PersianCalendar();
            string Day, Month, Year, Date = "";

            if (string.IsNullOrEmpty(test))
            {
                Year = oPersianC.GetYear(System.DateTime.Now).ToString();
                Month = oPersianC.GetMonth(System.DateTime.Now).ToString();
                Day = oPersianC.GetDayOfMonth(System.DateTime.Now).ToString();
                string day = DateTime.Now.DayOfWeek.ToString();
                switch (day)
                {
                    case "Sunday":
                        day = "یکشنبه";
                        break;
                    case "Monday":
                        day = "دوشنبه";
                        break;
                    case "Tuesday":
                        day = "سه شنبه";
                        break;
                    case "Wednesday":
                        day = "چهارشنبه";
                        break;
                    case "Thursday":
                        day = "پنج شنبه";
                        break;
                    case "Friday":
                        day = "جمعه";
                        break;
                    case "Saturday":
                        day = "شنبه";
                        break;

                }
                switch (Month)
                {
                    case "1":
                        Month = "فروردین";
                        break;
                    case "2":
                        Month = "اردیبهشت";
                        break;
                    case "3":
                        Month = "خرداد";
                        break;
                    case "4":
                        Month = "تیر";
                        break;
                    case "5":
                        Month = "مرداد";
                        break;
                    case "6":
                        Month = "شهریور";
                        break;
                    case "7":
                        Month = "مهر";
                        break;
                    case "8":
                        Month = "آبان";
                        break;
                    case "9":
                        Month = "آذر";
                        break;
                    case "10":
                        Month = "دی";
                        break;
                    case "11":
                        Month = "بهمن";
                        break;
                    case "12":
                        Month = "اسفند";
                        break;
                    default:
                        break;
                }
                Date = day + " " + Day + "  " + Month + "  " + Year;
            }
            else
            {
            }
            return Date;
        }
        public static string ToPersianDate(this DateTime date)
        {
            PersianCalendar g = new PersianCalendar();
            return string.Format("{0}/{1}/{2}", g.GetYear(date), g.GetMonth(date).ToString("00"), g.GetDayOfMonth(date).ToString("00"));
        }
        public static string Persiantime()
        {
            string day = DateTime.Now.DayOfWeek.ToString();
            switch (day)
            {
                case "Sunday":
                    day = "یکشنبه";
                    break;
                case "Monday":
                    day = "دوشنبه";
                    break;
                case "Tuesday":
                    day = "سه شنبه";
                    break;
                case "Wednesday":
                    day = "چهارشنبه";
                    break;
                case "Thursday":
                    day = "پنج شنبه";
                    break;
                case "Friday":
                    day = "جمعه";
                    break;
                case "Saturday":
                    day = "شنبه";
                    break;

            }

            string now = DateTime.Now.ToShortTimeString();
            if (now.Contains("PM"))
            {
                now = now.Replace("PM", "بعد از ظهر");
            }
            if (now.Contains("AM"))
            {
                now = now.Replace("PM", "صبح");
            }
            return day + "  " + now;
        }
        #endregion





        public static string ToArabicCharacter(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            str = str.Replace('ک', 'ك');
            str = str.Replace('ی', 'ي');
            str = str.Replace('ی', 'ي');
            str = str.Replace('١', '۱');
            str = str.Replace('٢', '۲');
            str = str.Replace('٣', '٣');
            str = str.Replace('٤', '۴');
            str = str.Replace('٥', '۵');
            str = str.Replace('٦', '۶');
            str = str.Replace('٦', '۶');


            return str;
            //        var obj = {
            //    'ك' :'ک',
            //    'دِ': 'د',
            //    'بِ': 'ب',
            //    'زِ': 'ز',
            //    'ذِ': 'ذ',
            //    'شِ': 'ش',
            //    'سِ': 'س',
            //    'ى' :'ی',
            //    'ي' :'ی',
            //    '١' :'۱',
            //    '٢' :'۲',
            //    '٣' :'۳',
            //    '٤' :'۴',
            //    '٥' :'۵',
            //    '٦' :'۶',
            //    '٧' :'۷',
            //    '٨' :'۸',
            //    '٩' :'۹',
            //    '٠' :'۰',
            //};
        }

        public static string ToPersianCharacter(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            str = str.Replace('ك', 'ک');
            str = str.Replace('ي', 'ی');
            str = str.Replace('۱', '١');
            str = str.Replace('۲', '٢');
            str = str.Replace('٣', '٣');
            str = str.Replace('۴', '٤');
            str = str.Replace('۵', '٥');
            str = str.Replace('۶', '٦');
            str = str.Replace('۶', '٦');


            return str;
            //        var obj = {
            //    'ك' :'ک',
            //    'دِ': 'د',
            //    'بِ': 'ب',
            //    'زِ': 'ز',
            //    'ذِ': 'ذ',
            //    'شِ': 'ش',
            //    'سِ': 'س',
            //    'ى' :'ی',
            //    'ي' :'ی',
            //    '١' :'۱',
            //    '٢' :'۲',
            //    '٣' :'۳',
            //    '٤' :'۴',
            //    '٥' :'۵',
            //    '٦' :'۶',
            //    '٧' :'۷',
            //    '٨' :'۸',
            //    '٩' :'۹',
            //    '٠' :'۰',
            //};
        }
    }
    /// <summary>
    /// تبدیل رشته به عدد
    /// </summary>
    class ConvertNumbersToLetters
    {
        /// <summary>
        /// یکان
        /// </summary>
        private static string[] yekan = new string[10] { "صفر", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };
        /// <summary>
        /// دهگان
        /// </summary>
        private static string[] dahgan = new string[10] { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };
        /// <summary>
        /// اعداد بین ده تا نوزده
        /// </summary>
        private static string[] dahyek = new string[10] { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
        //array[10..19]
        /// <summary>
        /// صدگان
        /// </summary>
        private static string[] sadgan = new string[10] { "", "یکصد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد" };
        /// <summary>
        /// با توجه به موقعیت مجموع سه تایی جدا شده از رشته اصلی یکی از گزینه های این مجموعه را بر می گرداند
        /// " "
        /// هزار
        /// میلیون
        /// میلیارد
        /// تریلیون
        /// </summary>
        private static string[] basex = new string[5] { "", "هزار", "میلیون", "میلیارد", "تریلیون" };

        /// <summary>
        /// یک عدد سه رقمی را گرفته و رشته حرفی معادل آن را برمیگرداند
        /// </summary>
        /// <param name="num3"> عدد سه رقمی</param>
        /// <returns></returns>
        private static string getnum3(int num3)
        {
            string s = "";
            int d3, d12;
            ///دورقم سمت راست آن را جدا می کند
            ///یکان و دهگان
            d12 = num3 % 100;
            /// صدگان را جدا می کند
            d3 = num3 / 100;
            if (d3 != 0)
                /// صدگان به عنوان ایندکس استفاده شده و مکان نمایش داده شده توسط این متغیر محتوی رشته فارسی معادل آن است
                s = sadgan[d3] + " و ";
            /// بررسی دو رقم سمت چپ که جدا شده بود
            /// عدد بین ده تا نوزده
            if ((d12 >= 10) && (d12 <= 19))
            {
                /// ده تا از عدد کم کرده رقم به دست آمده به عنوان ایندکس در آرایه زیر استفاده شده و معادل فارسی آن را بر میگرداند
                s = s + dahyek[d12 - 10];
            }
            else
            {
                /// اعداد بزرگتر از نوزده
                /// دهگان را جدا میکنیم
                int d2 = d12 / 10;
                if (d2 != 0)
                    /// معادل دهگان آن را استفاده می کنیم
                    s = s + dahgan[d2] + " و ";
                /// یکان آن را پیدا کرده
                int d1 = d12 % 10;
                if (d1 != 0)
                    ///معادل فارسی آن را  به رشته خروجی اضافه میکند
                    s = s + yekan[d1] + " و ";
                ///  به علت اینکه در هر بار اضافه شدن رشته معادل عدد به خروجی عبارت " و" به آخر آن اضافه میشود
                ///  برای آخرین معادل حروفی اضافه شده، یک " و " اضافه درج می شود که آن را حذف می کنیم
                s = s.Substring(0, s.Length - 3);
            };
            return s;
        }
        /// <summary>
        /// رشته ای از اعداد را گرفته 
        /// و به معادل فارسی آن تبدیل میکند
        /// </summary>
        /// <param name="snum">رشته ای از اعداد</param>
        /// <returns></returns>
        public static string num2str(string snum)
        {
            string stotal = "";
            /// اگر رشته ورودی برابر صفر باشد
            if (snum == "0")
            {
                ///عبارت صفر برگردانده می شود
                return yekan[0];
            }
            else
            {
                /// با اضافه کردن رشته صفر به سمت چپ رشته اعداد وارد شده تعداد آن را مضربی از سه می کنیم
                snum = snum.PadLeft(((snum.Length - 1) / 3 + 1) * 3, '0');
                /// تعداد مجموعه های سه تایی را مشخص می کنیم
                /// مثال 001،222،153
                /// L=9/3-1 ------>> 2
                int L = snum.Length / 3 - 1;
                for (int i = 0; i <= L; i++)
                {
                    /// سه رقم سه رقم جدا کرده 
                    /// آن را تبدیل به int
                    /// می کنیم 
                    int b = int.Parse(snum.Substring(i * 3, 3));
                    if (b != 0)
                        ///
                        stotal = stotal + getnum3(b) + " " + basex[L - i] + " و ";
                }
                ///  به علت اینکه در هر بار اضافه شدن رشته معادل عدد به خروجی عبارت " و" به آخر آن اضافه میشود
                ///  برای آخرین معادل حروفی اضافه شده، یک " و " اضافه درج می شود که آن را حذف می کنیم
                stotal = stotal.Substring(0, stotal.Length - 3);
            }
            return stotal;
        }




    }


}
