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
            public string? RequestId { get; set; }
            public string? TrackingId { get; set; }
            public string? InvoiceNumber { get; set; }
            public string? Fingerprint { get; set; }
            public string? LastError { get; set; }
            public string? LastErrorStage { get; set; }
            public string? LastInquiryResponseJson { get; set; }
            public DateTime? LastInquiryUtc { get; set; }
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
       Fingerprint, LastError, CreatedUtc, UpdatedUtc, RequestId,
       LastErrorStage, LastInquiryResponseJson, LastInquiryUtc
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
                UpdatedUtc = reader.GetDateTime(7),
                RequestId = reader.IsDBNull(8) ? null : reader.GetString(8),
                LastErrorStage = reader.IsDBNull(9) ? null : reader.GetString(9),
                LastInquiryResponseJson = reader.IsDBNull(10) ? null : reader.GetString(10),
                LastInquiryUtc = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
            };
        }

        /// <summary>
        /// شناسهٔ فاکتورهایی را می‌خواند که نباید دوباره ارسال شوند.
        /// فاکتور ردشده از فهرست حذف نمی‌شود تا پس از اصلاح دوباره قابل ارسال باشد.
        /// </summary>
        public async Task<HashSet<Guid>> GetSubmittedInvoiceIdsAsync(
            CancellationToken cancellationToken = default)
        {
            // فاکتورهای ردشده دوباره در کارتابل ارسال دیده می‌شوند؛
            // وضعیت‌های Pending، Sending و Unknown همچنان از ارسال مجدد محافظت می‌کنند.
            const string sql = @"
SELECT InvoiceId
FROM dbo.OmidInvoiceSubmission
WHERE State <> N'Failed';";
            var invoiceIds = new HashSet<Guid>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
                invoiceIds.Add(reader.GetGuid(0));

            return invoiceIds;
        }

        /// <summary>
        /// شرح رد قطعی امید را برای نمایش کنار فاکتورهای قابل ارسال می‌خواند.
        /// </summary>
        public async Task<Dictionary<Guid, string>> GetFailedErrorsAsync(
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT InvoiceId, LastError
FROM dbo.OmidInvoiceSubmission
WHERE State = N'Failed'
  AND LastError IS NOT NULL
  AND LTRIM(RTRIM(LastError)) <> N'';";

            var errors = new Dictionary<Guid, string>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync(cancellationToken);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
                errors[reader.GetGuid(0)] = reader.GetString(1);

            return errors;
        }
        // وضعیت ثبت‌شدهٔ یک فاکتور در دیتابیس محلی.


        /// <summary>
        /// فاکتورهایی را برمی‌گرداند که از امید requestId گرفته‌اند
        /// و می‌توان آن‌ها را در صفحهٔ استعلام امید نمایش داد.
        /// این متد فقط دیتابیس محلی را می‌خواند و به سرویس امید درخواست نمی‌فرستد.
        /// </summary>
        public async Task<List<OmidSubmissionStatus>> GetTrackableAsync(
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT InvoiceId, State, TrackingId, InvoiceNumber,
       Fingerprint, LastError, CreatedUtc, UpdatedUtc, RequestId,
       LastErrorStage, LastInquiryResponseJson, LastInquiryUtc
FROM dbo.OmidInvoiceSubmission
WHERE RequestId IS NOT NULL
  AND LTRIM(RTRIM(RequestId)) <> N''
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
                    UpdatedUtc = reader.GetDateTime(7),
                    RequestId = reader.IsDBNull(8) ? null : reader.GetString(8),
                    LastErrorStage = reader.IsDBNull(9) ? null : reader.GetString(9),
                    LastInquiryResponseJson = reader.IsDBNull(10) ? null : reader.GetString(10),
                    LastInquiryUtc = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
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
        /// خطای هر تلاش امید را برای سابقهٔ ممیزی در جدول جدا ذخیره می‌کند.
        /// JSON کامل درخواست و پاسخ در فایل OmidApiTrace ثبت می‌شود.
        /// </summary>
        public async Task LogAttemptErrorAsync(
            Guid? invoiceId,
            string operation,
            string endpoint,
            int? httpStatusCode,
            string outcome,
            string errorMessage,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO dbo.OmidApiAttemptLog
    (InvoiceId, Operation, Endpoint, HttpStatusCode, Outcome, ErrorMessage)
VALUES
    (@InvoiceId, @Operation, @Endpoint, @HttpStatusCode, @Outcome, @ErrorMessage);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = (object?)invoiceId ?? DBNull.Value;
            command.Parameters.Add("@Operation", SqlDbType.NVarChar, 40)
                .Value = operation.Length <= 40 ? operation : operation[..40];
            command.Parameters.Add("@Endpoint", SqlDbType.NVarChar, 200)
                .Value = endpoint.Length <= 200 ? endpoint : endpoint[..200];
            command.Parameters.Add("@HttpStatusCode", SqlDbType.Int)
                .Value = (object?)httpStatusCode ?? DBNull.Value;
            command.Parameters.Add("@Outcome", SqlDbType.NVarChar, 30)
                .Value = outcome.Length <= 30 ? outcome : outcome[..30];
            command.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, 2000)
                .Value = string.IsNullOrWhiteSpace(errorMessage)
                    ? "خطای نامشخص امید"
                    : errorMessage.Length <= 2000
                        ? errorMessage
                        : errorMessage[..2000];

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// شناسهٔ فاکتور را پیش از ارسال به امید رزرو می‌کند.
        /// true یعنی این فاکتور برای اولین بار رزرو شد.
        /// فاکتور جدید یا فاکتور ردشده در مرحلهٔ ارسال/استعلام را برای تلاش مجدد آماده می‌کند.
        /// وضعیت‌های Pending، Sending و Unknown قابل ارسال مجدد نیستند.
        /// </summary>
        public async Task<bool> TrySaveOmidInvoiceSubmissionAsync(
            Guid invoiceId,
            CancellationToken cancellationToken = default)
        {
            if (invoiceId == Guid.Empty)
                throw new ArgumentException(
                    "شناسهٔ فاکتور معتبر نیست.",
                    nameof(invoiceId));

            const string reactivateSql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Prepared',
    RequestId = NULL,
    TrackingId = NULL,
    InvoiceNumber = NULL,
    Fingerprint = NULL,
    LastError = NULL,
    LastErrorStage = NULL,
    LastInquiryResponseJson = NULL,
    LastInquiryUtc = NULL,
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId
  AND State = N'Failed'
  AND (RequestId IS NULL OR LastErrorStage = N'Inquiry');";

            const string insertSql = @"
INSERT INTO dbo.OmidInvoiceSubmission (InvoiceId, State)
VALUES (@InvoiceId, N'Prepared');";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var transaction = connection.BeginTransaction();
            try
            {
                using var reactivate = new SqlCommand(
                    reactivateSql, connection, transaction);
                reactivate.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                    .Value = invoiceId;

                if (await reactivate.ExecuteNonQueryAsync(cancellationToken) == 1)
                {
                    transaction.Commit();
                    return true;
                }

                using var insert = new SqlCommand(
                    insertSql, connection, transaction);
                insert.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                    .Value = invoiceId;

                // کلید اصلی جدول باعث می‌شود حتی دو درخواست هم‌زمان
                // نتوانند یک فاکتور را دو بار رزرو کنند.
                bool saved = await insert.ExecuteNonQueryAsync(cancellationToken) == 1;
                transaction.Commit();
                return saved;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // رکورد این فاکتور از قبل وجود دارد.
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

            const string reactivateSql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Prepared',
    RequestId = NULL,
    TrackingId = NULL,
    InvoiceNumber = NULL,
    Fingerprint = NULL,
    LastError = NULL,
    LastErrorStage = NULL,
    LastInquiryResponseJson = NULL,
    LastInquiryUtc = NULL,
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId
  AND State = N'Failed'
  AND (RequestId IS NULL OR LastErrorStage = N'Inquiry');";

            const string insertSql = @"
INSERT INTO dbo.OmidInvoiceSubmission (InvoiceId, State)
VALUES (@InvoiceId, N'Prepared');";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // یا همهٔ رکوردها ذخیره می‌شوند، یا هیچ‌کدام.
            using var transaction = connection.BeginTransaction();

            try
            {
                using var reactivate = new SqlCommand(
                    reactivateSql, connection, transaction);
                using var insert = new SqlCommand(
                    insertSql, connection, transaction);

                var idParameter = insert.Parameters.Add(
                    "@InvoiceId",
                    SqlDbType.UniqueIdentifier);

                var reactivateIdParameter = reactivate.Parameters.Add(
                    "@InvoiceId",
                    SqlDbType.UniqueIdentifier);

                foreach (Guid invoiceId in orderedIds)
                {
                    reactivateIdParameter.Value = invoiceId;
                    int inserted = await reactivate.ExecuteNonQueryAsync(
                        cancellationToken);

                    if (inserted == 0)
                    {
                        idParameter.Value = invoiceId;
                        inserted = await insert.ExecuteNonQueryAsync(
                            cancellationToken);
                    }

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
        /// پس از دریافت رسید معتبر امید، requestId و وضعیت Pending را ثبت می‌کند.
        /// Pending یعنی امید درخواست را برای پردازش پذیرفته؛ نتیجهٔ نهایی هنوز معلوم نیست.
        /// </summary>
        public async Task MarkPendingAsync(
            Guid invoiceId,
            OmidSendReceipt receipt,
            CancellationToken cancellationToken = default)
        {
            if (receipt == null || string.IsNullOrWhiteSpace(receipt.RequestId))
                throw new ArgumentException(
                    "رسید امید باید requestId داشته باشد.",
                    nameof(receipt));

            // اندازه‌ها باید با ستون‌های جدول هماهنگ باشند.
            if (receipt.RequestId.Length > 100 ||
                (receipt.TrackingId?.Length ?? 0) > 100 ||
                (receipt.InvoiceNumber?.Length ?? 0) > 100 ||
                (receipt.Fingerprint?.Length ?? 0) > 100)
            {
                throw new InvalidOperationException(
                    "یکی از مقادیر رسید امید از ظرفیت ستون دیتابیس طولانی‌تر است.");
            }

            const string sql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Pending',
    RequestId = @RequestId,
    TrackingId = @TrackingId,
    InvoiceNumber = @InvoiceNumber,
    Fingerprint = @Fingerprint,
    LastError = NULL,
    LastErrorStage = NULL,
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId AND State = N'Sending';";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;
            command.Parameters.Add("@RequestId", SqlDbType.NVarChar, 100)
                .Value = receipt.RequestId;
            command.Parameters.Add("@TrackingId", SqlDbType.NVarChar, 100)
                .Value = (object?)receipt.TrackingId ?? DBNull.Value;
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
        /// آخرین نتیجهٔ استعلام را برای نمایش در کارتابل ذخیره می‌کند.
        /// </summary>
        public async Task<(string State, string? Message)?> SaveInquiryResultAsync(
            Guid invoiceId,
            string expectedRequestId,
            string state,
            string? message,
            string responseJson,
            CancellationToken cancellationToken = default)
        {
            if (invoiceId == Guid.Empty)
                throw new ArgumentException(
                    "شناسهٔ فاکتور معتبر نیست.", nameof(invoiceId));

            if (state != "Pending" && state != "Accepted" && state != "Failed")
                throw new ArgumentException(
                    "وضعیت نتیجهٔ استعلام معتبر نیست.", nameof(state));

            if (!Guid.TryParse(expectedRequestId, out Guid parsedRequestId) || parsedRequestId == Guid.Empty)
                throw new ArgumentException("requestId استعلام معتبر نیست.", nameof(expectedRequestId));

            const string sql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = CASE WHEN @State = N'Pending' AND State IN (N'Accepted', N'Failed') THEN State ELSE @State END,
    LastError = CASE WHEN @State = N'Pending' AND State IN (N'Accepted', N'Failed') THEN LastError ELSE @LastError END,
    LastErrorStage = CASE
        WHEN @State = N'Pending' AND State IN (N'Accepted', N'Failed') THEN LastErrorStage
        WHEN @State = N'Failed' AND @LastError IS NOT NULL THEN N'Inquiry'
        ELSE NULL
    END,
    LastInquiryResponseJson = @ResponseJson,
    LastInquiryUtc = SYSUTCDATETIME(),
    UpdatedUtc = SYSUTCDATETIME()
OUTPUT inserted.State, inserted.LastError
WHERE InvoiceId = @InvoiceId
  AND RequestId = @ExpectedRequestId
  AND State NOT IN (N'Prepared', N'Sending');";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;
            command.Parameters.Add("@ExpectedRequestId", SqlDbType.NVarChar, 100)
                .Value = expectedRequestId.Trim();
            command.Parameters.Add("@State", SqlDbType.NVarChar, 20)
                .Value = state;
            command.Parameters.Add("@LastError", SqlDbType.NVarChar, 1000)
                .Value = string.IsNullOrWhiteSpace(message)
                    ? DBNull.Value
                    : message.Length <= 1000 ? message : message[..1000];
            command.Parameters.Add("@ResponseJson", SqlDbType.NVarChar, -1)
                .Value = string.IsNullOrWhiteSpace(responseJson)
                    ? DBNull.Value
                    : responseJson;

            await connection.OpenAsync(cancellationToken);

            // صفر ردیف یعنی پاسخ مربوط به رسید فعلی نیست؛ از بازنویسی آن جلوگیری می‌کنیم.
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;
            return (reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1));
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
    LastErrorStage = N'SendInvoice',
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

        /// <summary>
        /// وقتی امید پاسخ روشنِ رد شدن برگرداند، فاکتور را Failed می‌کند.
        /// فاکتور ردشده پس از اصلاح داده می‌تواند دوباره ارسال شود.
        /// </summary>
        public async Task MarkFailedAsync(
            Guid invoiceId,
            string error,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE dbo.OmidInvoiceSubmission
SET State = N'Failed',
    LastError = @LastError,
    LastErrorStage = N'SendInvoice',
    UpdatedUtc = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId AND State = N'Sending';";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@InvoiceId", SqlDbType.UniqueIdentifier)
                .Value = invoiceId;
            command.Parameters.Add("@LastError", SqlDbType.NVarChar, 1000)
                .Value = string.IsNullOrWhiteSpace(error)
                    ? "امید صورتحساب را نپذیرفت."
                    : error.Length <= 1000 ? error : error[..1000];

            await connection.OpenAsync(cancellationToken);
            if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw new InvalidOperationException(
                    "فاکتور در وضعیت Sending نبود؛ وضعیت Failed ثبت نشد.");
        }
    }
}
