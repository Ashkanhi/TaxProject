using System.Data.SqlClient;

namespace TaxWebAppCore8.BI;

public static class SettingsDatabaseConnection
{
    public static string Build(IReadOnlyDictionary<string, string> values, string savedPassword)
    {
        string Value(string key) => values.GetValueOrDefault(key, "")?.Trim() ?? "";
        if (new[] { "DataBaseServer", "DbName", "UserId" }.Any(k => string.IsNullOrWhiteSpace(Value(k))))
            throw new ArgumentException("آدرس سرور، نام دیتابیس و نام کاربری را وارد کنید.");
        if (!int.TryParse(Value("ConnectionTimeOut"), out var timeout) || timeout is < 1 or > 3600)
            throw new ArgumentException("مهلت اتصال باید بین ۱ تا ۳۶۰۰ ثانیه باشد.");
        var password = values.GetValueOrDefault("UserPass", "");
        return new SqlConnectionStringBuilder
        {
            DataSource = Value("DataBaseServer"), InitialCatalog = Value("DbName"),
            UserID = Value("UserId"), Password = string.IsNullOrEmpty(password) ? savedPassword : password,
            TrustServerCertificate = true, ConnectTimeout = Math.Min(timeout, 10),
            Pooling = false, PersistSecurityInfo = false, ApplicationName = "Baran360.SettingsTest"
        }.ConnectionString;
    }

    public static async Task TestAsync(string connectionString, CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(12));
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(deadline.Token);
        using var command = new SqlCommand("SELECT 1", connection) { CommandTimeout = 3 };
        await command.ExecuteScalarAsync(deadline.Token);
    }
}
