using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using TaxCollectData.Library.Models;
namespace TaxServiceAndApp.Models.TaxDTO
{
    public class GeneralInvoiceHeaderViewModel
    {
        // Tax Straight INQ Result
        public InquiryResultModel ResultModel { get; set; }

        // Tiss INQ Result
        public InquiryByRef ResultModelInquiryByRef { get; set; }
        public string fTaxID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        //RimaInq Result
        public InvoiceInquiryViewModel? RimaResultModelInquiry { get; set; }

        public string InqMessage { get; set; } = "ارسال نشده است!";
        public bool WasQueriedFromTaxService { get; set; }
        public bool InquiryFailed { get; set; }
        public bool HasValidInquiryReference
        {
            get
            {
                var reference = !string.IsNullOrWhiteSpace(fTaxHistoryReferenceNumber)
                    ? fTaxHistoryReferenceNumber
                    : !string.IsNullOrWhiteSpace(taxReferenceNumber)
                        ? taxReferenceNumber
                        : ReferenceNumber;
                return Guid.TryParse(reference, out _);
            }
        }
        public string InquirySource => WasQueriedFromTaxService
            ? "استعلام‌شده از سامانه مودیان"
            : "وضعیت ثبت‌شده محلی";
        public string ReferenceNumber { get; set; }
        public bool IsSelected { get; set; } = false;
        public string SendStatStr
        {
            get
            {
                if (fSendState == 1) return "ارسال نشده";
                if (fSendState == 2) return "ارسال شده(وضعیت نامشخص)";
                if (fSendState == 3) return "ارسال شده با خطا";
                if (fSendState == 4) return "ارسال موفق";

                return "";
            }
        }
        public int? BookerWorkstationID { get; set; }
        public string InvoiceId { get; set; }
        public int InvoiceNumber { get; set; }
        public string InvoiceNumberSt { set; get; }
        public string RealInvoiceNumber
        {
            get
            {
                string s = "";
                try
                {
                    s = InvoiceNumber > 0 ? InvoiceNumber.ToString() : InvoiceNumberSt;
                }
                catch
                { }
                return s;
            }
        }
        public DateTime CreateDate { get; set; }
        public DateTime SaveDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string TotalPriceWithSeperator
        {
            get
            {
                return TotalPrice.ToString("#,0");
            }
        }
        public decimal TotalDiscount { get; set; } = 0;
        public decimal TotalTax { get; set; }
        public string PersianCreateDate
        {
            get
            {
                try
                {
                    if (InvoiceDate == null)
                        return "";
                    return new MD.PersianDateTime.PersianDateTime((DateTime)InvoiceDate).ToShortDateString();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }
        //مبلغ خالص حاصل جمع بلغ کل منهای تخفیف
        public decimal NetPrice { get; set; }
        public string NetPriceWithSeperator
        {
            get
            {
                return NetPrice.ToString("#,0");
            }
        }
        //برای مشتریان فروشگاهی نام مشتری فروشگاهی است و غیرفروشگاهی نام مشتری 
        public string CustomerName { set; get; }
        public string InvoiceDescription { set; get; }





        public bool? Done { get; set; }

        public string fListOfErrorCaption { get; set; }

        public string fListOfErrorCode { get; set; }

        public int? StoreID { get; set; }

        [StringLength(100)]
        public string taxReferenceNumber { get; set; }
        [StringLength(50)]
        public string StoreName { get; set; }

        public int? fSendState { get; set; }

        public string fTaxHistoryReferenceNumber { get; set; }

        public DateTime? fLastSendDate { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long fID { get; set; }
    }
}
