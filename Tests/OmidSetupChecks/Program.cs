using System.Text.RegularExpressions;
using TaxWebAppCore8.BI.Omid;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var objects = OmidSetupCatalog.Objects;
Check(objects.Select(o => o.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == objects.Count, "Unique object names");
foreach (var item in objects)
{
    Check(Regex.IsMatch(item.Name, "^[A-Za-z_][A-Za-z0-9_]*$"), "Safe catalog identifier");
    Check(item.Columns.Select(c => c.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == item.Columns.Count, "Unique columns: " + item.Name);
    foreach (var dependency in item.Dependencies ?? [])
        Check(objects.Any(o => o.Name == dependency), "Known dependency: " + dependency);
    if (item.Sql != null)
    {
        Check(!Regex.IsMatch(item.Sql, @"^\s*(USE|GO|ALTER)\b", RegexOptions.Multiline | RegexOptions.IgnoreCase), "Creation stays in the selected database: " + item.Name);
        Check(item.Sql.StartsWith("CREATE ", StringComparison.OrdinalIgnoreCase), "Only create scripts");
    }
}
var invoiceId = new SetupColumn("InvoiceId", "uniqueidentifier", false, PrimaryKey: true);
Check(!OmidSetupService.CompareColumn(invoiceId, invoiceId).Any(), "Compatible column accepted");
Check(OmidSetupService.CompareColumn(invoiceId, invoiceId with { Type = "nvarchar(100)" }).Any(), "Legacy string invoice IDs rejected");
Check(OmidSetupService.CompareColumn(invoiceId, invoiceId with { PrimaryKey = false }).Any(), "Missing duplicate-send protection key detected");
Check(OmidSetupService.CompareColumn(invoiceId, invoiceId with { Nullable = true }).Any(), "Unexpected NULL detected");
var timestamp = new SetupColumn("CreatedUtc", "datetime2(3)", false, Default: "SYSUTCDATETIME()");
Check(!OmidSetupService.CompareColumn(timestamp, timestamp with { Default = "(sysutcdatetime())" }).Any(), "SQL default normalization");
Check(OmidSetupService.CompareColumn(timestamp, timestamp with { Default = null }).Any(), "Missing timestamp default detected");
Check(OmidSetupService.CompareColumn(timestamp, timestamp with { Type = "datetime2(7)" }).Any(), "Scale mismatch detected");
Check(OmidSetupService.CompareColumn(new("RequestId", "nvarchar(100)", true), new("RequestId", "nvarchar(50)", true)).Any(), "Truncation risk detected");
Check(OmidSetupService.CompareColumn(new("Id", "bigint", false, true), new("Id", "bigint", false)).Any(), "Missing identity detected");
Check(!OmidSetupService.CompareColumn(new("Name"), new("Name", "nvarchar(80)", true)).Any(), "Source column presence contracts do not invent types");
var bulk = objects.Single(o => o.Name == "UV_TaxInfo_BulkSale");
Check(bulk.Columns.Count > 70 && bulk.Columns.Any(c => c.Name == "sstid"), "Bulk source checklist covers full procedure projection");
var header = objects.Single(o => o.Name == "vw_TaxGovermentInvoiceHeaderForShow");
Check(header.Columns.Any(c => c.Name == "InvoiceDate") && header.Columns.Any(c => c.Name == "fTaxID"), "Current cartable fields included");
Check(objects.Where(o => o.Description.StartsWith("وابستگی")).All(o => o.Sql == null), "Never create empty sales-system tables");
Console.WriteLine($"All Omid setup checks passed ({objects.Count} objects). No database connection or mutation performed.");
