using System.Data.SqlClient;

namespace TaxWebAppCore8.BI.Omid;

public sealed class OmidDashboardSummary
{
    public long Total { get; set; }
    public long SentSuccessfully { get; set; }
    public long InquiryErrors { get; set; }
    public long InquirySucceeded { get; set; }
    public long OmidErrors { get; set; }
    public long PieAwaitingInquiry { get; set; }
    public long PieInquiryErrors { get; set; }
    public long PieAccepted { get; set; }
    public long PieOmidErrors { get; set; }
    public long PieOther { get; set; }
    public DateTimeOffset CheckedAt { get; set; }

    // Metrics overlap: a confirmed invoice also has a successful send receipt.
    public void Add(string state, string? errorStage, bool hasReceipt, bool latestInquiryError, long count)
    {
        Total += count;
        if (state is "Prepared" or "Sending") { PieOther += count; return; }
        bool inquiryError = (state == "Failed" && (errorStage == "Inquiry" || (hasReceipt && errorStage == null))) || (hasReceipt && latestInquiryError);
        bool sendError = (state is "Failed" or "Unknown") && errorStage == "SendInvoice";
        // Disjoint distribution: an accepted result takes precedence over a later connection error.
        if (state == "Accepted") PieAccepted += count;
        else if (sendError) PieOmidErrors += count;
        else if (inquiryError) PieInquiryErrors += count;
        else if (hasReceipt) PieAwaitingInquiry += count;
        else PieOther += count;
        if (hasReceipt) SentSuccessfully += count;
        if (state == "Accepted") InquirySucceeded += count;
        if (inquiryError)
            InquiryErrors += count;
        if (sendError) OmidErrors += count;
    }
}

public sealed class OmidDashboardService(string connectionString)
{
    public async Task<OmidDashboardSummary> ReadAsync(CancellationToken ct)
    {
        // Count invoices, not attempts. An error older than the latest saved outcome or
        // a new send must not remain an active inquiry error. No remote API is called.
        const string sql = """
            WITH InvoiceFlags AS
            (
                SELECT s.State, s.LastErrorStage,
                    CASE WHEN NULLIF(LTRIM(RTRIM(s.RequestId)), N'') IS NOT NULL THEN 1 ELSE 0 END AS HasReceipt,
                    CASE WHEN EXISTS
                    (
                        SELECT 1 FROM dbo.OmidApiAttemptLog a
                        WHERE a.InvoiceId = s.InvoiceId AND a.Operation = N'Inquiry' AND a.Outcome = N'Error'
                            AND a.CreatedUtc >= s.UpdatedUtc
                            AND (s.LastInquiryUtc IS NULL OR a.CreatedUtc > s.LastInquiryUtc)
                    ) THEN 1 ELSE 0 END AS LatestInquiryError
                FROM dbo.OmidInvoiceSubmission s
            )
            SELECT State, LastErrorStage, HasReceipt, LatestInquiryError, COUNT_BIG(*)
            FROM InvoiceFlags
            GROUP BY State, LastErrorStage, HasReceipt, LatestInquiryError;
            """;
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        await connection.OpenAsync(ct);
        using var reader = await command.ExecuteReaderAsync(ct);
        var summary = new OmidDashboardSummary();
        while (await reader.ReadAsync(ct))
            summary.Add(reader.GetString(0), await reader.IsDBNullAsync(1, ct) ? null : reader.GetString(1),
                reader.GetInt32(2) == 1, reader.GetInt32(3) == 1, reader.GetInt64(4));
        summary.CheckedAt = DateTimeOffset.UtcNow;
        return summary;
    }
}
