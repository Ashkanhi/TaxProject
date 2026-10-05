using System;
using System.Globalization;
namespace TaxServiceAndApp.Models.TaxDTO
    {
    public class GeneralInvoiceHeaderViewModel
        {
        public bool IsSelected { get; set; } = false;
        public int SendStat { get; set; }
        public string InvoiceId { get; set; }
        public int InvoiceNumber { get; set; }
        public string InvoiceNumberSt { set; get; }
        public string RealInvoiceNumber { get { string s = "";
                try
                    {

                        s= InvoiceNumber>0? InvoiceNumber.ToString(): InvoiceNumberSt;


                    }
                catch 
                    {

                  
                    }
                return s;
                } }
        public DateTime CreateDate { get; set; }
        public DateTime SaveDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalDiscount { get; set; } = 0;
        public decimal TotalTax { get; set; }
        public string PersianCreateDate { get
                {
                try
                    {
                    if (CreateDate == null)
                        return "";
                    return new MD.PersianDateTime.PersianDateTime((DateTime)CreateDate).ToShortDateString();
                    }
                catch (Exception)
                    {

                    return "";
                    }
                } }
        //مبلغ خالص حاصل جمع بلغ کل منهای تخفیف
        public decimal NetPrice { get; set; }
        //برای مشتریان فروشگاهی نام مشتری فروشگاهی است و غیرفروشگاهی نام مشتری 
        public string CustomerName { set; get; }
        public string InvoiceDescription { set; get; }
        }
    }
