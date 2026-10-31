using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;

namespace TaxWebAppCore8.BI.NewTax;

public sealed record NewTaxSetupObject(string Name, string Kind, string Description,
    string[] Columns, int? ArgumentCount = null, string[]? Dependencies = null);

// Mirrors NewTaxService -> DBBridge and NewTaxStateStore; no Omid-specific objects.
public static class NewTaxSetupCatalog
{
    private static NewTaxSetupObject Model<T>(string name, string kind, string description, params string[] dependencies) =>
        new(name, kind, description, typeof(T).GetProperties()
            .Where(p => p.CanWrite && p.GetCustomAttribute<NotMappedAttribute>() == null
                && (p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[])))
            .Select(p => p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name).ToArray(), Dependencies: dependencies);
    private static NewTaxSetupObject Source(string name, string kind, string columns) =>
        new(name, kind, "وابستگی خواندن فاکتور از سامانهٔ فروش", columns.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    public static IReadOnlyList<NewTaxSetupObject> Objects { get; } = Build();
    private static IReadOnlyList<NewTaxSetupObject> Build()
    {
        var objects = new NewTaxSetupObject[]
        {
        Model<TaxHistory>("TaxHistory", "U", "اطلاعات فاکتورهای کارتابل"),
        Model<tTaxHistoryStateParallel>("tTaxHistoryStateParallel", "U", "آخرین وضعیت ارسال و استعلام NewTax"),
        Model<tSendInvoiceTaxTissLog>("tSendInvoiceTaxTissLog", "U", "ثبت هر تلاش ارسال NewTax"),
        Model<TaxGovermentInvoiceInfo>("TaxGovermentInvoiceInfo", "U", "اطلاعات پذیرش ارسال و وضعیت Done"),
        Model<TaxGovermentInvoiceFinal>("TaxGovermentInvoiceFinal", "U", "اطلاعات نهایی صورتحساب"),
        Model<TaxGovermentSendLog>("TaxGovermentSendLog", "U", "پاسخ‌های استعلام"),
        Source("Store", "U", "StoreID Name StatusID"),
        Source("SaleInvoice", "U", "InvoiceID InvoiceDate CustomerID ExpectedAmount InvoiceTypeID BusinessDate InvoiceNumber BookerStoreID BookerWorkstationID"),
        Source("SaleInvoiceCustomField", "U", "InvoiceID TaxInnoNumber IrtaxID BookerWorkstationID"),
        Source("StockDocument", "U", "DocumentID DocumentDate"),
        Source("SaleInvoiceLineItem", "U", "InvoiceID LineItemID ItemID ManualDiscount PriceAmount Quantity BonusQuantity DiscountAmount InvoiceDiscount IsPriceWithTax TypeID"),
        Source("Item", "U", "ItemID Name"), Source("ItemCustomField", "U", "ItemID CommodityID"),
        Source("Department", "U", "DepartmentID TaxAmount TollAmount"),
        Source("ItemDepartmentAssignmentView", "V", "ItemID DepartmentID TypeID"),
        Source("CustomerView", "V", "CustomerID LanguageID CustomerName IsIndividual"),
        Source("UV_TaxInfo_BulkSale", "V", "DocumentID BookerStoreID BookerWorkstationID"),
        Model<vw_TaxGovermentInvoiceHeaderForShow>("vw_TaxGovermentInvoiceHeaderForShow", "V", "فهرست ارسال، استعلام و گزارش وضعیت NewTax",
            "TaxHistory", "tTaxHistoryStateParallel", "SaleInvoiceCustomField", "SaleInvoice"),
        Model<vwTaxHistory>("vwTaxHistory", "V", "خواندن شمارهٔ پیگیری پیش از استعلام",
            "TaxHistory", "tTaxHistoryStateParallel"),
        new("Usp_TaxCartable_DetailInfo", "P", "خواندن سربرگ و ردیف‌های فاکتور برای نگاشت NewTax", [], 3,
            ["SaleInvoice", "StockDocument", "SaleInvoiceLineItem", "Item", "ItemCustomField", "SaleInvoiceCustomField", "Department", "ItemDepartmentAssignmentView", "CustomerView", "UV_TaxInfo_BulkSale"]),
        new("TaxGovermentSaveInvoiceInsert", "P", "ذخیرهٔ پاسخ پذیرش ارسال NewTax", [], 5, ["TaxGovermentInvoiceInfo"]),
        new("TaxGovermentInvoiceFinalInsertUpdate", "P", "ذخیرهٔ نتیجه و ابطال", [], 6, ["TaxGovermentInvoiceFinal"]),
        new("TaxGovermentSaveInvoiceLogInsert", "P", "ذخیرهٔ پاسخ استعلام NewTax", [], 5, ["TaxGovermentInvoiceInfo", "TaxGovermentSendLog"])
        };
        // Same reviewed detail procedure is used by DBBridge for NewTax; inspect the
        // embedded SQL for bulk-sale columns rather than maintaining a partial list.
        using var stream = typeof(NewTaxSetupCatalog).Assembly.GetManifestResourceStream("OmidSetup.Detail.sql")
            ?? throw new InvalidOperationException("اسکریپت مرجع خواندن جزئیات فاکتور موجود نیست.");
        using var reader = new StreamReader(stream);
        var bulk = Regex.Match(reader.ReadToEnd(), @"UNION\s+ALL\s+SELECT(?<columns>[\s\S]+?)FROM\s+\[UV_TaxInfo_BulkSale\]", RegexOptions.IgnoreCase);
        if (!bulk.Success) throw new InvalidOperationException("ستون‌های فروش عمده در اسکریپت مرجع شناسایی نشدند.");
        var columns = bulk.Groups["columns"].Value.Split(',').Select(c => c.Trim())
            .Where(c => Regex.IsMatch(c, @"^\w+$")).Concat(["DocumentID", "BookerStoreID", "BookerWorkstationID"])
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var index = Array.FindIndex(objects, item => item.Name == "UV_TaxInfo_BulkSale");
        objects[index] = objects[index] with { Columns = columns };
        return Array.AsReadOnly(objects);
    }
}
