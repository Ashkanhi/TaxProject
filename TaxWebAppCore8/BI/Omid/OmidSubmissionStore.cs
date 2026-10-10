using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TaxServiceAndApp.Models.SharedDTO;

namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidSubmissionStore
    {
        private readonly string _connectionString;


        public sealed class OmidSubmissionStatus
        {
            public Guid InvoiceId { get; set; }
            public string State { get; set; } = string.Empty;
            public string? TrackingId { get; set; }
            public string? InvoiceNumber { get; set; }
            public string? Fingerprint { get; set; }
            public string? LastError { get; set; }
            public DateTime CreatedUtc { get; set; }
            public DateTime UpdatedUtc { get; set; }
        }

        /// <summary>
        /// وضعیت محلی فاکتور را می‌خواند؛ هیچ درخواست HTTP و هیچ تغییری
        /// در دیتابیس انجام نمی‌دهد. اگر رکوردی نباشد، null برمی‌گرداند.
        /// </summary>
        public async Task<OmidSubmissionStatus?> GetAsync(
            Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            if (invoiceId == Guid.Empty)
                throw new ArgumentException(
                    "شناسهٔ فاکتور معتبر نیست.",
                    nameof(invoiceId));

            const string sql = @"
SELECT InvoiceId, State, TrackingId, InvoiceNumber,
       Fingerprint, LastError, CreatedUtc, UpdatedUtc
FROM dbo.OmidInvoiceSubmission
WHERE InvoiceId = @InvoiceId;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;

            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new OmidSubmissionStatus
            {
                InvoiceId = reader.GetGuid(0),
                State = reader.GetString(1),
                TrackingId = reader.IsDBNull(2) ? null : reader.GetString(2),
                InvoiceNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                Fingerprint = reader.IsDBNull(4) ? null : reader.GetString(4),
                LastError = reader.IsDBNull(5) ? null : reader.GetString(5),
                CreatedUtc = reader.GetDateTime(6),
                UpdatedUtc = reader.GetDateTime(7)
            };
        }
        // وضعیت ثبت‌شدهٔ یک فاکتور در دیتابیس محلی.

        public OmidSubmissionStore(DTOServerInfo serverInfo)
        {



            if (serverInfo == null)
                throw new ArgumentNullException(nameof(serverInfo));

            // از همان دیتابیسی استفاده می‌کنیم که پروژه برای این مشتری تنظیم کرده است.
            _connectionString = serverInfo.DBConn;
        }

        /// <summary>
        /// شناسهٔ فاکتور را پیش از ارسال به امید رزرو می‌کند.
        /// true یعنی این فاکتور برای اولین بار رزرو شد.
        /// false یعنی قبلاً رکوردی برای آن وجود داشته و نباید دوباره ارسال شود.
        /// </summary>
        public async Task<bool> TryReserveAsync(
            Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            if (invoiceId == Guid.Empty)
                throw new ArgumentException(
                    "شناسهٔ فاکتور معتبر نیست.",
                    nameof(invoiceId));

            const string sql = @"
INSERT INTO dbo.OmidInvoiceSubmission (InvoiceId, State)
VALUES (@InvoiceId, N'Prepared');";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            // مقدار شناسه را پارامتری می‌فرستیم تا SQL از متن ورودی ساخته نشود.
            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;

            await connection.OpenAsync(cancellationToken);

            try
            {
                // کلید اصلی جدول باعث می‌شود حتی دو درخواست هم‌زمان
                // نتوانند یک فاکتور را دو بار رزرو کنند.
                return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // رکورد این فاکتور از قبل وجود دارد.
                return false;
            }
        }


        /// <summary>
        /// درست پیش از درخواست HTTP، وضعیت فاکتور را Sending می‌کند.
        /// فقط فاکتوری که قبلاً Prepared شده باشد می‌تواند وارد این وضعیت شود.
        /// </summary>
        public async Task MarkSendingAsync(
            Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Sending',
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId AND State = N'Prepared';";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;

            await connection.OpenAsync(cancellationToken);

            if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw new InvalidOperationException(
                    "فاکتور برای ارسال به امید در وضعیت Prepared پیدا نشد.");
        }

        /// <summary>
        /// پس از دریافت رسید معتبر امید، trackingId و وضعیت Pending را ثبت می‌کند.
        /// Pending یعنی امید درخواست را برای پردازش پذیرفته؛ نتیجهٔ نهایی هنوز معلوم نیست.
        /// </summary>
        public async Task MarkPendingAsync(
            Guid invoiceId,
            OmidSendReceipt receipt,
            CancellationToken cancellationToken = default)
        {
            if (receipt == null || string.IsNullOrWhiteSpace(receipt.TrackingId))
                throw new ArgumentException(
                    "رسید امید باید trackingId داشته باشد.",
                    nameof(receipt));

            // اندازه‌ها باید با ستون‌های جدول هماهنگ باشند.
            if (receipt.TrackingId.Length > 100 ||
                (receipt.InvoiceNumber?.Length ?? 0) > 100 ||
                (receipt.Fingerprint?.Length ?? 0) > 100)
            {
                throw new InvalidOperationException(
                    "یکی از مقادیر رسید امید از ظرفیت ستون دیتابیس طولانی‌تر است.");
            }

            const string sql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Pending',
    TrackingId = @TrackingId,
    InvoiceNumber = @InvoiceNumber,
    Fingerprint = @Fingerprint,
    LastError = NULL,
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId AND State = N'Sending';";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;
            command.Parameters.Add("@TrackingId", SqlDbType.NVarChar, 100)
                .Value = receipt.TrackingId;
            command.Parameters.Add("@InvoiceNumber", SqlDbType.NVarChar, 100)
                .Value = (object?)receipt.InvoiceNumber ?? DBNull.Value;
            command.Parameters.Add("@Fingerprint", SqlDbType.NVarChar, 100)
                .Value = (object?)receipt.Fingerprint ?? DBNull.Value;

            await connection.OpenAsync(cancellationToken);

            if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw new InvalidOperationException(
                    "فاکتور در وضعیت Sending پیدا نشد؛ رسید امید ثبت نشد.");
        }

        /// <summary>
        /// وقتی پس از شروع ارسال، نتیجهٔ آن نامشخص بماند، وضعیت را Unknown می‌کند.
        /// فاکتور Unknown نباید خودکار دوباره ارسال شود.
        /// </summary>
        public async Task MarkUnknownAsync(
            Guid invoiceId,
            string error,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Unknown',
    LastError = @LastError,
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId AND State = N'Sending';";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;
            command.Parameters.Add("@LastError", SqlDbType.NVarChar, 1000)
                .Value = string.IsNullOrWhiteSpace(error)
                    ? "نتیجهٔ ارسال مشخص نیست."
                    : error.Length <= 1000 ? error : error[..1000];

            await connection.OpenAsync(cancellationToken);

            if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw new InvalidOperationException(
                    "فاکتور در وضعیت Sending پیدا نشد؛ وضعیت Unknown ثبت نشد.");
        }
    }
}