
using System;

namespace TaxServiceAndApp.Models.SharedDTO
{
    public class SearchParameters
    {
        public string InvoiceNumberFrom { get; set; }

        public DateTime? DateFrom { get; set; }
        public string InvoiceNumberTo { get; set; }

        public DateTime? DateTo { get; set; }
        /// <summary>
        ///   مقدار 0 برای ارسال نشده 1 ارسال نامعلوم و 2 ارسال موفق 
        /// </summary>
        public byte? Status { get; set; }
        public bool? JustLast { get; set; }
        public Guid? CompanyID { get; set; }
        public int? StorID { set; get; }
        public string DateFromStr
        {
            get
            {
                try
                {
                    if (DateFrom == null)
                        return "";
                    return new MD.PersianDateTime.PersianDateTime((DateTime)DateFrom).ToShortDateString();
                }
                catch (Exception)
                {

                    return "";
                }

            }
            set
            {
                if (value != null)
                    DateFrom = MD.PersianDateTime.PersianDateTime.Parse(value).ToDateTime();
            }
        }
        public string DateToStr
        {
            get
            {
                try
                {
                    if (DateTo == null)
                        return "";
                    return new MD.PersianDateTime.PersianDateTime((DateTime)DateTo).ToShortDateString();
                }
                catch (Exception)
                {

                    return "";
                }

            }
            set
            {
                if (value != null)
                    DateTo = MD.PersianDateTime.PersianDateTime.Parse(value).ToDateTime();
            }
        }

    }
}
