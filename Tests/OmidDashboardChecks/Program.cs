using TaxWebAppCore8.BI.Omid;

static void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
}
var summary = new OmidDashboardSummary();
Check(summary.Total == 0 && summary.SentSuccessfully == 0, "Empty database is zero");
summary.Add("Pending", null, true, false, 12);
summary.Add("Accepted", null, true, false, 8);
summary.Add("Failed", "Inquiry", true, false, 3);
summary.Add("Pending", null, true, true, 2);
summary.Add("Failed", "SendInvoice", false, false, 4);
summary.Add("Unknown", "SendInvoice", false, false, 1);
summary.Add("Prepared", null, false, false, 7);
summary.Add("Sending", null, false, false, 6);
Check(summary.Total == 43, "Total includes in-progress invoices");
Check(summary.SentSuccessfully == 25, "Receipt count includes both inquiry outcomes");
Check(summary.InquirySucceeded == 8, "Only accepted is inquiry success");
Check(summary.InquiryErrors == 5, "Rejected and unresolved transport errors count invoices");
Check(summary.OmidErrors == 5, "Send rejection and unknown are separate from inquiry errors");
Check(summary.PieAwaitingInquiry == 12 && summary.PieAccepted == 8 && summary.PieInquiryErrors == 5 &&
    summary.PieOmidErrors == 5 && summary.PieOther == 13, "Pie categories partition the invoice population");
Check(summary.PieAwaitingInquiry + summary.PieAccepted + summary.PieInquiryErrors + summary.PieOmidErrors + summary.PieOther == summary.Total,
    "Pie totals contain no duplicate invoices");
var acceptedWithConnectionError = new OmidDashboardSummary();
acceptedWithConnectionError.Add("Accepted", null, true, true, 1);
Check(acceptedWithConnectionError.PieAccepted == 1 && acceptedWithConnectionError.PieInquiryErrors == 0,
    "Pie preserves accepted result over later connection error");
var bothErrors = new OmidDashboardSummary();
bothErrors.Add("Failed", "Inquiry", true, true, 1);
Check(bothErrors.InquiryErrors == 1, "One invoice with two error signals is counted once");
var retry = new OmidDashboardSummary();
retry.Add("Prepared", null, false, true, 1);
retry.Add("Sending", null, false, true, 1);
Check(retry.InquiryErrors == 0 && retry.OmidErrors == 0, "In-progress resend is not an old error");
var recovered = new OmidDashboardSummary();
recovered.Add("Accepted", null, true, false, 1);
Check(recovered.InquiryErrors == 0, "Resolved error is absent from latest-state metrics");
var legacy = new OmidDashboardSummary();
legacy.Add("Failed", null, true, false, 1);
Check(legacy.InquiryErrors == 1, "Legacy rejected receipt without error stage is included");
var large = new OmidDashboardSummary();
large.Add("Pending", null, true, false, 3_000_000_000);
Check(large.SentSuccessfully == 3_000_000_000, "Counts use 64-bit integers");
Console.WriteLine("All dashboard metric checks passed. No database or Omid API called.");
