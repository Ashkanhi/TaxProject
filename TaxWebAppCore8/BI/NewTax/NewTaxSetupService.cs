using System.Data;
using System.Data.SqlClient;

namespace TaxWebAppCore8.BI.NewTax;

public sealed class NewTaxSetupCheck
{
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Description { get; init; } = "";
    public bool Exists { get; set; }
    public List<string> Errors { get; } = [];
    public bool Ready => Exists && Errors.Count == 0;
    public int ColumnCount { get; init; }
}

// Inspection only: never executes stored procedures or changes the customer's schema/data.
public sealed class NewTaxSetupService(string connectionString)
{
    public async Task<List<NewTaxSetupCheck>> CheckAsync(CancellationToken ct)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        var results = new List<NewTaxSetupCheck>();
        foreach (var item in NewTaxSetupCatalog.Objects)
        {
            var result = new NewTaxSetupCheck { Name = item.Name, Kind = item.Kind,
                Description = item.Description, ColumnCount = item.Columns.Length };
            results.Add(result);
            try
            {
                using var exists = new SqlCommand("SELECT type FROM sys.objects WHERE object_id=OBJECT_ID(@name)", connection);
                exists.Parameters.Add("@name", SqlDbType.NVarChar, 256).Value = "dbo." + item.Name;
                var kind = (await exists.ExecuteScalarAsync(ct))?.ToString()?.Trim();
                result.Exists = kind != null;
                if (kind == null) { result.Errors.Add("موجود نیست یا مجوز مشاهدهٔ ساختار وجود ندارد."); continue; }
                if (kind != item.Kind) { result.Errors.Add("نوع موجودیت با نوع مورد انتظار متفاوت است."); continue; }
                if (item.Kind == "P")
                {
                    using var parameters = new SqlCommand("SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID(@name) AND parameter_id>0", connection);
                    parameters.Parameters.Add("@name", SqlDbType.NVarChar, 256).Value = "dbo." + item.Name;
                    var count = Convert.ToInt32(await parameters.ExecuteScalarAsync(ct));
                    if (count != item.ArgumentCount) result.Errors.Add($"تعداد پارامترها: انتظار {item.ArgumentCount}، فعلی {count}.");
                    using var permission = new SqlCommand("SELECT HAS_PERMS_BY_NAME(@name, 'OBJECT', 'EXECUTE')", connection);
                    permission.Parameters.Add("@name", SqlDbType.NVarChar, 256).Value = "dbo." + item.Name;
                    if (Convert.ToInt32(await permission.ExecuteScalarAsync(ct)) != 1)
                        result.Errors.Add("مجوز اجرای رویه برای حساب اتصال موجود نیست.");
                    continue;
                }
                // TOP(0) validates read access and view dependencies without retrieving invoices.
                using var probe = new SqlCommand($"SELECT TOP (0) * FROM [dbo].[{item.Name}]", connection);
                using var reader = await probe.ExecuteReaderAsync(ct);
                var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var column in item.Columns)
                    if (!columns.Contains(column)) result.Errors.Add($"ستون مورد نیاز موجود نیست: {column}");
            }
            catch (SqlException ex) { result.Errors.Add($"خطای SQL {ex.Number}: {ex.Message}"); }
        }
        foreach (var item in NewTaxSetupCatalog.Objects)
            foreach (var dependency in item.Dependencies ?? [])
                if (!results.Single(r => r.Name == dependency).Ready)
                    results.Single(r => r.Name == item.Name).Errors.Add($"وابستگی آماده نیست: dbo.{dependency}");
        return results;
    }
}
