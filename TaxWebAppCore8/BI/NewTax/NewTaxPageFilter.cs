using System.Globalization;
using TaxServiceAndApp.Models.SharedDTO;

namespace TaxWebAppCore8.BI.NewTax;

public sealed class NewTaxPageFilter
{
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public byte? Status { get; set; }
    public List<int> StoreIds { get; set; } = new();

    public SearchParameters ToSearch()
    {
        if (string.IsNullOrWhiteSpace(FromDate) || string.IsNullOrWhiteSpace(ToDate))
            throw new ArgumentException("بازه تاریخ را کامل وارد کنید.");
        DateTime Parse(string value)
        {
            var normalized = string.Concat(value.Trim().Select(c => c is >= '۰' and <= '۹' ? (char)('0' + c - '۰')
                : c is >= '٠' and <= '٩' ? (char)('0' + c - '٠') : c));
            var parts = normalized.Split('/');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var month)
                || !int.TryParse(parts[2], out var day) || year < 1300 || year > 1600)
                throw new ArgumentException("تاریخ را به صورت سال/ماه/روز شمسی وارد کنید.");
            try { return new PersianCalendar().ToDateTime(year, month, day, 0, 0, 0, 0); }
            catch (ArgumentOutOfRangeException) { throw new ArgumentException("تاریخ واردشده معتبر نیست."); }
        }
        var from = Parse(FromDate);
        var to = Parse(ToDate);
        if (from > to) throw new ArgumentException("تاریخ شروع باید قبل از تاریخ پایان باشد.");
        if (Status is < 1 or > 4) throw new ArgumentException("وضعیت انتخاب‌شده معتبر نیست.");
        return new() { DateFrom = from, DateTo = to, Status = Status, StorID = -1,
            StorIDs = StoreIds.Where(id => id >= 0).Distinct().ToList() };
    }
}
