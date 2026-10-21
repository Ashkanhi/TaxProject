using System.Text.RegularExpressions;

namespace TaxWebAppCore8.BI.Omid;

public sealed record SetupColumn(string Name, string? Type = null, bool? Nullable = null,
    bool Identity = false, bool PrimaryKey = false, string? Default = null);
public sealed record SetupObject(string Name, string Kind, string Description,
    IReadOnlyList<SetupColumn> Columns, string? Sql = null, string[]? Dependencies = null);

// Only definitions in this catalog can be executed by the setup endpoint.
public static class OmidSetupCatalog
{
    public static IReadOnlyList<SetupObject> Objects { get; } = Build();

    private static SetupObject Table(string name, string description, params SetupColumn[] columns)
    {
        var definitions = columns.Select(c => $"[{c.Name}] {c.Type}" +
            (c.Identity ? " IDENTITY(1,1)" : "") + (c.Nullable == true ? " NULL" : " NOT NULL") +
            (c.PrimaryKey ? " PRIMARY KEY" : "") + (c.Default == null ? "" : $" DEFAULT ({c.Default})"));
        return new(name, "U", description, columns,
            $"CREATE TABLE [dbo].[{name}] ({string.Join(",", definitions)});");
    }

    private static List<SetupObject> Build()
    {
        var items = new List<SetupObject>
        {
            Table("OmidInvoiceSubmission", "وضعیت ارسال و استعلام امید",
                new("InvoiceId", "uniqueidentifier", false, PrimaryKey: true),
                new("State", "nvarchar(20)", false),
                new("RequestId", "nvarchar(100)", true), new("TrackingId", "nvarchar(100)", true),
                new("InvoiceNumber", "nvarchar(100)", true), new("Fingerprint", "nvarchar(100)", true),
                new("LastError", "nvarchar(1000)", true), new("LastErrorStage", "nvarchar(40)", true),
                new("LastInquiryResponseJson", "nvarchar(max)", true), new("LastInquiryUtc", "datetime2(3)", true),
                new("CreatedUtc", "datetime2(3)", false, Default: "SYSUTCDATETIME()"),
                new("UpdatedUtc", "datetime2(3)", false, Default: "SYSUTCDATETIME()")),
            Table("OmidApiAttemptLog", "تاریخچه تلاش‌های ارتباط با امید",
                new("Id", "bigint", false, true, true), new("InvoiceId", "uniqueidentifier", true),
                new("Operation", "nvarchar(40)", false), new("Endpoint", "nvarchar(200)", false),
                new("HttpStatusCode", "int", true), new("Outcome", "nvarchar(30)", false),
                new("ErrorMessage", "nvarchar(2000)", false),
                new("CreatedUtc", "datetime2(3)", false, Default: "SYSUTCDATETIME()")),
            Table("TaxHistory", "فهرست فاکتورها",
                new("InvoiceId", "uniqueidentifier", false), new("StoreName", "nvarchar(50)", true),
                new("CustomerName", "nvarchar(152)", true), new("InvoiceNumberSt", "nvarchar(50)", true),
                new("TotalPrice", "decimal(38,0)", true), new("TotalDiscount", "decimal(38,0)", true),
                new("TotalTax", "decimal(38,0)", true), new("NetPrice", "decimal(38,0)", true),
                new("BusinessDate", "date", true), new("StoreID", "int", true), new("fID", "bigint", false, true, true)),
            Table("tTaxHistoryStateParallel", "وضعیت مشترک فاکتورها",
                new("fID", "bigint", false, true, true), new("fInvoiceID", "uniqueidentifier", false),
                new("fSendState", "int", false), new("fListOfErrorCode", "nvarchar(max)", false),
                new("fListOfErrorCaption", "nvarchar(max)", false), new("ReferenceNumber", "nvarchar(max)", true),
                new("fLastSendDate", "datetime", true), new("fTaxID", "nvarchar(max)", true))
        };
        items[1] = items[1] with { Sql = items[1].Sql +
            " CREATE INDEX IX_OmidApiAttemptLog_InvoiceId_CreatedUtc ON dbo.OmidApiAttemptLog(InvoiceId, CreatedUtc DESC);" };
        items.Add(Table("tTisTaxErrorType", "فهرست مشترک خطاهای کارتابل",
            new("fID", "bigint", false, true, true), new("fType", "smallint", false, Default: "0"),
            new("fCaption", "nvarchar(max)", false), new("fDescription", "nvarchar(max)", false),
            new("fCreatedDateTime", "datetime", false, Default: "SYSDATETIME()"), new("fCode", "nvarchar(max)", false)));

        void External(string name, string kind, string columns) => items.Add(new(name, kind,
            "وابستگی دیتابیس فروش؛ نصب یا اصلاح با اسکریپت معتبر سامانه فروش",
            columns.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(n => new SetupColumn(n)).ToArray()));
        External("Store", "U", "StoreID Name StatusID TypeID ParentID");
        External("SaleInvoice", "U", "InvoiceID InvoiceDate CustomerID ExpectedAmount InvoiceTypeID BusinessDate InvoiceNumber BookerStoreID BookerWorkstationID");
        External("SaleInvoiceCustomField", "U", "InvoiceID TaxInnoNumber IrtaxID BookerWorkstationID");
        External("StockDocument", "U", "DocumentID DocumentDate");
        External("SaleInvoiceLineItem", "U", "InvoiceID LineItemID ItemID ManualDiscount PriceAmount Quantity BonusQuantity DiscountAmount InvoiceDiscount IsPriceWithTax TypeID");
        External("Item", "U", "ItemID Name");
        External("ItemCustomField", "U", "ItemID CommodityID");
        External("Department", "U", "DepartmentID TaxAmount TollAmount");
        External("ItemDepartmentAssignmentView", "V", "ItemID DepartmentID TypeID");
        External("CustomerView", "V", "CustomerID LanguageID CustomerName IsIndividual");
        External("UV_TaxInfo_BulkSale", "V", "DocumentID BookerStoreID BookerWorkstationID");

        const string header = """
            CREATE VIEW dbo.vw_TaxGovermentInvoiceHeaderForShow AS
            SELECT TH.InvoiceId, TH.StoreName, TH.CustomerName, TH.InvoiceNumberSt,
                TH.TotalPrice, TH.TotalDiscount, TH.TotalTax, TH.NetPrice,
                TH.BusinessDate AS CreateDate, SF.TaxInnoNumber AS InvoiceDescription,
                TH.StoreID, ST.fSendState, ST.fListOfErrorCode, ST.fListOfErrorCaption,
                ST.ReferenceNumber AS fTaxHistoryReferenceNumber, ST.fLastSendDate,
                TH.fID, SF.BookerWorkstationID, ST.fTaxID, SI.InvoiceDate
            FROM dbo.TaxHistory TH
            LEFT JOIN dbo.tTaxHistoryStateParallel ST ON TH.InvoiceId = ST.fInvoiceID
            LEFT JOIN dbo.SaleInvoiceCustomField SF ON SF.InvoiceID = TH.InvoiceId
            LEFT JOIN dbo.SaleInvoice SI ON SI.InvoiceID = TH.InvoiceId
            """;
        items.Add(new("vw_TaxGovermentInvoiceHeaderForShow", "V", "ویوی کارتابل ارسال",
            "InvoiceId StoreName CustomerName InvoiceNumberSt TotalPrice TotalDiscount TotalTax NetPrice CreateDate InvoiceDescription StoreID fSendState fListOfErrorCode fListOfErrorCaption fTaxHistoryReferenceNumber fLastSendDate fID BookerWorkstationID fTaxID InvoiceDate"
                .Split(' ').Select(n => new SetupColumn(n)).ToArray(), header,
            ["TaxHistory", "tTaxHistoryStateParallel", "SaleInvoiceCustomField", "SaleInvoice"]));

        // Keep the reviewed procedure as a resource, not a user supplied file/path/SQL command.
        var assembly = typeof(OmidSetupCatalog).Assembly;
        using var stream = assembly.GetManifestResourceStream("OmidSetup.Detail.sql")
            ?? throw new InvalidOperationException("اسکریپت جزئیات فاکتور در برنامه موجود نیست.");
        using var reader = new StreamReader(stream);
        var source = reader.ReadToEnd();
        // The bulk branch selects these source columns directly. Keep its checklist in sync
        // with the embedded procedure, including columns not qualified with the TB alias.
        var bulk = Regex.Match(source, @"UNION\s+ALL\s+SELECT(?<columns>[\s\S]+?)FROM\s+\[UV_TaxInfo_BulkSale\]", RegexOptions.IgnoreCase);
        if (!bulk.Success) throw new InvalidOperationException("تعریف شاخهٔ فروش عمده قابل شناسایی نیست.");
        var bulkColumns = bulk.Groups["columns"].Value.Split(',')
            .Select(c => c.Trim()).Where(c => Regex.IsMatch(c, @"^\w+$"))
            .Distinct(StringComparer.OrdinalIgnoreCase).Select(c => new SetupColumn(c)).ToArray();
        var bulkIndex = items.FindIndex(o => o.Name == "UV_TaxInfo_BulkSale");
        items[bulkIndex] = items[bulkIndex] with { Columns = bulkColumns };
        var start = source.IndexOf("ALTER PROCEDURE", StringComparison.OrdinalIgnoreCase);
        var procedure = Regex.Split(source[start..], @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)[0];
        procedure = Regex.Replace(procedure, "^ALTER PROCEDURE", "CREATE PROCEDURE", RegexOptions.IgnoreCase);
        items.Add(new("Usp_TaxCartable_DetailInfo", "P", "روال خواندن جزئیات برای ارسال امید", [], procedure,
            ["SaleInvoice", "StockDocument", "SaleInvoiceLineItem", "Item", "ItemCustomField",
             "SaleInvoiceCustomField", "Department", "ItemDepartmentAssignmentView", "CustomerView", "UV_TaxInfo_BulkSale"]));
        return items;
    }
}
