using System.Data;
using System.Data.SqlClient;

namespace TaxWebAppCore8.BI.Omid;

public sealed class SetupCheck
{
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
    public string Description { get; init; } = "";
    public bool Exists { get; set; }
    public bool CanCreate { get; set; }
    public List<string> Errors { get; } = [];
    public bool Ready => Exists && Errors.Count == 0;
    public int ColumnCount { get; init; }
}

public sealed class OmidSetupService(string connectionString)
{
    public async Task<List<SetupCheck>> CheckAsync(CancellationToken ct)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        var results = new List<SetupCheck>();
        foreach (var item in OmidSetupCatalog.Objects)
            results.Add(await CheckObjectAsync(connection, null, item, ct));
        foreach (var item in OmidSetupCatalog.Objects)
        {
            var result = results.Single(r => r.Name == item.Name);
            foreach (var dependency in item.Dependencies ?? [])
                if (!results.Single(r => r.Name == dependency).Ready)
                {
                    result.Errors.Add($"وابستگی آماده نیست: dbo.{dependency}");
                    result.CanCreate = false;
                }
        }
        return results;
    }

    private static async Task<SetupCheck> CheckObjectAsync(SqlConnection connection,
        SqlTransaction? transaction, SetupObject item, CancellationToken ct)
    {
        var result = new SetupCheck { Name = item.Name, Kind = item.Kind,
            Description = item.Description, ColumnCount = item.Columns.Count };
        try
        {
            using var exists = new SqlCommand("SELECT type FROM sys.objects WHERE object_id = OBJECT_ID(@name)", connection, transaction);
            exists.Parameters.Add("@name", SqlDbType.NVarChar, 256).Value = "dbo." + item.Name;
            var kind = (await exists.ExecuteScalarAsync(ct))?.ToString()?.Trim();
            result.Exists = kind != null;
            if (kind == null)
            {
                result.Errors.Add("ایجاد نشده است (یا حساب اتصال مجوز مشاهدهٔ آن را ندارد).");
                result.CanCreate = item.Sql != null;
                return result;
            }
            if (kind != item.Kind)
            {
                result.Errors.Add("شیئی با همین نام و نوع متفاوت وجود دارد.");
                return result;
            }

            using var command = new SqlCommand("""
                SELECT c.name, TYPE_NAME(c.system_type_id), c.max_length, c.precision, c.scale,
                    c.is_nullable, c.is_identity, dc.definition,
                    CONVERT(bit, CASE WHEN EXISTS (SELECT 1 FROM sys.index_columns ic
                        JOIN sys.indexes i ON i.object_id=ic.object_id AND i.index_id=ic.index_id
                        WHERE ic.object_id=c.object_id AND ic.column_id=c.column_id
                            AND i.is_primary_key=1 AND i.is_disabled=0
                            AND (SELECT COUNT(*) FROM sys.index_columns keys
                                WHERE keys.object_id=i.object_id AND keys.index_id=i.index_id
                                    AND keys.key_ordinal>0)=1)
                        THEN 1 ELSE 0 END)
                FROM sys.columns c LEFT JOIN sys.default_constraints dc ON c.default_object_id=dc.object_id
                WHERE c.object_id=OBJECT_ID(@name)
                """, connection, transaction);
            command.Parameters.Add("@name", SqlDbType.NVarChar, 256).Value = "dbo." + item.Name;
            var actual = new Dictionary<string, SetupColumn>(StringComparer.OrdinalIgnoreCase);
            using (var reader = await command.ExecuteReaderAsync(ct))
                while (await reader.ReadAsync(ct))
                {
                    string type = reader.GetString(1);
                    if (type is "nvarchar" or "varchar" or "nchar" or "char" or "varbinary" or "binary")
                    {
                        int length = reader.GetInt16(2);
                        type += "(" + (length == -1 ? "max" : (length / (type.StartsWith('n') ? 2 : 1)).ToString()) + ")";
                    }
                    else if (type is "decimal" or "numeric") type = $"decimal({reader.GetByte(3)},{reader.GetByte(4)})";
                    else if (type is "datetime2" or "datetimeoffset" or "time") type += $"({reader.GetByte(4)})";
                    actual.Add(reader.GetString(0), new(reader.GetString(0), type, reader.GetBoolean(5),
                        reader.GetBoolean(6), reader.GetBoolean(8), await reader.IsDBNullAsync(7, ct) ? null : reader.GetString(7)));
                }
            foreach (var expected in item.Columns)
            {
                if (!actual.TryGetValue(expected.Name, out var found))
                    result.Errors.Add($"ستون موجود نیست: {expected.Name}");
                else result.Errors.AddRange(CompareColumn(expected, found));
            }
            if (item.Kind == "V")
            {
                // Compile without reading invoice data; catches broken view dependencies.
                using var probe = new SqlCommand($"SELECT TOP (0) * FROM [dbo].[{item.Name}]", connection, transaction);
                using var reader = await probe.ExecuteReaderAsync(ct);
            }
            if (item.Kind == "P")
            {
                using var parameters = new SqlCommand("SELECT COUNT(*) FROM sys.parameters WHERE object_id=OBJECT_ID(@name) AND name IN (N'@InvoiceID',N'@BookerStoreID',N'@BookerWorkstationID') AND ((name=N'@InvoiceID' AND system_type_id=36) OR (name<>N'@InvoiceID' AND system_type_id=56))", connection, transaction);
                parameters.Parameters.AddWithValue("@name", "dbo." + item.Name);
                if (Convert.ToInt32(await parameters.ExecuteScalarAsync(ct)) != 3)
                    result.Errors.Add("پارامترهای روال با InvoiceID(uniqueidentifier)، BookerStoreID(int) و BookerWorkstationID(int) سازگار نیستند.");
            }
        }
        catch (SqlException ex)
        {
            result.CanCreate = false;
            result.Errors.Add($"خطای SQL {ex.Number}: {ex.Message}");
        }
        return result;
    }

    public static IEnumerable<string> CompareColumn(SetupColumn expected, SetupColumn actual)
    {
        if (expected.Type != null && !string.Equals(expected.Type, actual.Type, StringComparison.OrdinalIgnoreCase))
            yield return $"{expected.Name}: نوع مورد انتظار {expected.Type}؛ نوع فعلی {actual.Type}";
        if (expected.Nullable.HasValue && expected.Nullable != actual.Nullable)
            yield return $"{expected.Name}: وضعیت NULL باید {(expected.Nullable.Value ? "مجاز" : "غیرمجاز")} باشد.";
        if (expected.Identity && !actual.Identity) yield return $"{expected.Name}: ویژگی IDENTITY موجود نیست.";
        if (expected.PrimaryKey && !actual.PrimaryKey) yield return $"{expected.Name}: کلید اصلی موجود نیست.";
        if (expected.Default != null && Normalize(expected.Default) != Normalize(actual.Default))
            yield return $"{expected.Name}: مقدار پیش‌فرض باید {expected.Default} باشد.";
    }

    private static string Normalize(string? value) => (value ?? "").Replace("(", "").Replace(")", "").Replace(" ", "").ToLowerInvariant();

    public async Task CreateAsync(string name, CancellationToken ct)
    {
        var item = OmidSetupCatalog.Objects.SingleOrDefault(o => o.Name == name);
        if (item?.Sql == null) throw new InvalidOperationException("تعریف ساخت معتبر برای این مورد وجود ندارد.");
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction();
        // Serialize creates and recheck inside the transaction to make repeated clicks safe.
        using (var guard = new SqlCommand("""
            SET XACT_ABORT ON; SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON;
            DECLARE @r int;
            EXEC @r=sys.sp_getapplock @Resource=N'OmidSetup', @LockMode=N'Exclusive',
                @LockOwner=N'Transaction', @LockTimeout=15000;
            IF @r < 0 THROW 51000, N'قفل ستاپ دریافت نشد؛ دوباره تلاش کنید.', 1;
            """, connection, transaction))
            await guard.ExecuteNonQueryAsync(ct);
        var current = await CheckObjectAsync(connection, transaction, item, ct);
        if (current.Ready) { transaction.Commit(); return; }
        if (current.Exists || !current.CanCreate)
            throw new InvalidOperationException("ایجاد مجاز نیست: " + string.Join("؛ ", current.Errors));
        foreach (var nameOfDependency in item.Dependencies ?? [])
        {
            var dependency = OmidSetupCatalog.Objects.Single(o => o.Name == nameOfDependency);
            var check = await CheckObjectAsync(connection, transaction, dependency, ct);
            if (!check.Ready) throw new InvalidOperationException($"ابتدا وابستگی {dependency.Name} را آماده کنید: " + string.Join("؛ ", check.Errors));
        }
        using (var command = new SqlCommand(item.Sql, connection, transaction))
            await command.ExecuteNonQueryAsync(ct);
        var after = await CheckObjectAsync(connection, transaction, item, ct);
        if (!after.Ready) throw new InvalidOperationException("ساخت تأیید نشد و برگشت داده شد: " + string.Join("؛ ", after.Errors));
        transaction.Commit();
    }
}
