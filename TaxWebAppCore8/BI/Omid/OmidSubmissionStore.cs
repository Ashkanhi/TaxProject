using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TaxServiceAndApp.Models.SharedDTO;
using System.Collections.Generic;


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

        /// <summary>
        /// شناسهٔ تمام فاکتورهایی را که برای امید ثبت شده‌اند می‌خواند.
        /// حتی رکوردهای در حال ارسال یا با نتیجهٔ نامشخص هم نباید دوباره در فهرست ارسال بیایند.
        /// </summary>
        public async Task<HashSet<Guid>> GetSubmittedInvoiceIdsAsync(
            CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT InvoiceId FROM dbo.OmidInvoiceSubmission;";
            var invoiceIds = new HashSet<Guid>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
                invoiceIds.Add(reader.GetGuid(0));

            return invoiceIds;
        }
        // وضعیت ثبت‌شدهٔ یک فاکتور در دیتابیس محلی.


        /// <summary>
        /// فاکتورهایی را برمی‌گرداند که از امید شناسهٔ پیگیری گرفته‌اند
        /// و می‌توان آن‌ها را در صفحهٔ استعلام امید نمایش داد.
        /// این متد فقط دیتابیس محلی را می‌خواند و به سرویس امید درخواست نمی‌فرستد.
        /// </summary>
        public async Task<List<OmidSubmissionStatus>> GetTrackableAsync(
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT InvoiceId, State, TrackingId, InvoiceNumber,
       Fingerprint, LastError, CreatedUtc, UpdatedUtc
FROM dbo.OmidInvoiceSubmission
WHERE TrackingId IS NOT NULL
  AND LTRIM(RTRIM(TrackingId)) <> N''
  AND State NOT IN (N'Prepared', N'Sending')
ORDER BY UpdatedUtc DESC;";

            var result = new List<OmidSubmissionStatus>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new OmidSubmissionStatus
                {
                    InvoiceId = reader.GetGuid(0),
                    State = reader.GetString(1),
                    TrackingId = reader.IsDBNull(2) ? null : reader.GetString(2),
                    InvoiceNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Fingerprint = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LastError = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreatedUtc = reader.GetDateTime(6),
                    UpdatedUtc = reader.GetDateTime(7)
                });
            }

            return result;
        }

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
        public async Task<bool> TrySaveOmidInvoiceSubmissionAsync(
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
        /// شناسهٔ همهٔ فاکتورهای دسته را در جدول OmidInvoiceSubmission ذخیره می‌کند.
        /// اگر یکی از شناسه‌ها قبلاً ثبت شده باشد، ذخیرهٔ کل دسته لغو می‌شود.
        /// این متد هیچ درخواستی به سرویس امید نمی‌فرستد.
        /// </summary>
        public async Task<bool> TrySaveOmidInvoiceSubmissionBatchAsync(
            IReadOnlyCollection<Guid> invoiceIds,
            CancellationToken cancellationToken = default)
        {
            if (invoiceIds == null || invoiceIds.Count == 0)
                throw new ArgumentException(
                    "فهرست فاکتورها خالی است.",
                    nameof(invoiceIds));

            var uniqueIds = new HashSet<Guid>();

            foreach (Guid invoiceId in invoiceIds)
            {
                if (invoiceId == Guid.Empty || !uniqueIds.Add(invoiceId))
                    throw new ArgumentException(
                        "شناسهٔ خالی یا تکراری در فهرست فاکتورها وجود دارد.",
                        nameof(invoiceIds));
            }

            var orderedIds = new List<Guid>(uniqueIds);
            orderedIds.Sort();

            const string sql = @"
INSERT INTO dbo.OmidInvoiceSubmission (InvoiceId, State)
VALUES (@InvoiceId, N'Prepared');";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // یا همهٔ رکوردها ذخیره می‌شوند، یا هیچ‌کدام.
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqlCommand(sql, connection, transaction);

                var idParameter = command.Parameters.Add(
                    "@InvoiceId",
                    SqlDbType.UniqueIdentifier);

                foreach (Guid invoiceId in orderedIds)
                {
                    idParameter.Value = invoiceId;

                    int inserted = await command.ExecuteNonQueryAsync(
                        cancellationToken);

                    if (inserted != 1)
                        throw new InvalidOperationException(
                            $"ذخیرهٔ فاکتور {invoiceId} انجام نشد.");
                }

                transaction.Commit();
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // دست‌کم یک فاکتور از قبل در جدول وجود دارد.
                transaction.Rollback();
                return false;
            }
            catch
            {
                transaction.Rollback();
                throw;
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
        /// وضعیت همهٔ فاکتورهای یک دسته را یکجا از Prepared به Sending تغییر می‌دهد.
        /// اگر حتی یک فاکتور Prepared نباشد، تغییر وضعیت کل دسته لغو می‌شود.
        /// این متد هنوز هیچ درخواستی به امید نمی‌فرستد.
        /// </summary>
        public async Task MarkBatchSendingAsync(
            IReadOnlyCollection<Guid> invoiceIds,
            CancellationToken cancellationToken = default)
        {
            if (invoiceIds == null || invoiceIds.Count == 0)
                throw new ArgumentException(
                    "فهرست فاکتورها خالی است.",
                    nameof(invoiceIds));

            var uniqueIds = new HashSet<Guid>();

            foreach (Guid invoiceId in invoiceIds)
            {
                if (invoiceId == Guid.Empty || !uniqueIds.Add(invoiceId))
                    throw new ArgumentException(
                        "شناسهٔ خالی یا تکراری در فهرست فاکتورها وجود دارد.",
                        nameof(invoiceIds));
            }

            var orderedIds = new List<Guid>(uniqueIds);
            orderedIds.Sort();

            const string sql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Sending',
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId
  AND State = N'Prepared';";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqlCommand(sql, connection, transaction);

                var idParameter = command.Parameters.Add(
                    "@InvoiceId",
                    SqlDbType.UniqueIdentifier);

                foreach (Guid invoiceId in orderedIds)
                {
                    idParameter.Value = invoiceId;

                    int changed = await command.ExecuteNonQueryAsync(
                        cancellationToken);

                    if (changed != 1)
                        throw new InvalidOperationException(
                            $"فاکتور {invoiceId} در وضعیت Prepared پیدا نشد.");
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



        /// <summary>
        /// فقط رکوردهای Prepared همین دسته را حذف می‌کند.
        /// Sending، Pending و Unknown هرگز حذف نمی‌شوند.
        /// </summary>
        public async Task<int> DeletePreparedOmidInvoiceSubmissionBatchAsync(
            IReadOnlyCollection<Guid> invoiceIds,
            CancellationToken cancellationToken = default)
        {
            if (invoiceIds == null || invoiceIds.Count == 0)
                throw new ArgumentException(
                    "فهرست فاکتورها خالی است.",
                    nameof(invoiceIds));

            var uniqueIds = new HashSet<Guid>();

            foreach (Guid invoiceId in invoiceIds)
            {
                if (invoiceId == Guid.Empty || !uniqueIds.Add(invoiceId))
                    throw new ArgumentException(
                        "شناسهٔ خالی یا تکراری در فهرست فاکتورها وجود دارد.",
                        nameof(invoiceIds));
            }

            var orderedIds = new List<Guid>(uniqueIds);
            orderedIds.Sort();

            const string sql = @"
DELETE FROM dbo.OmidInvoiceSubmission
WHERE InvoiceId = @InvoiceId
  AND State = N'Prepared';";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqlCommand(sql, connection, transaction);

                var idParameter = command.Parameters.Add(
                    "@InvoiceId",
                    SqlDbType.UniqueIdentifier);

                int deletedCount = 0;

                foreach (Guid invoiceId in orderedIds)
                {
                    idParameter.Value = invoiceId;
                    deletedCount += await command.ExecuteNonQueryAsync(
                        cancellationToken);
                }

                transaction.Commit();
                return deletedCount;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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
