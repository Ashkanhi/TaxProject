SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.OmidInvoiceSubmission', N'U') IS NULL
    THROW 51000, N'Table dbo.OmidInvoiceSubmission does not exist.', 1;

IF COL_LENGTH(N'dbo.OmidInvoiceSubmission', N'LastErrorStage') IS NULL
    ALTER TABLE dbo.OmidInvoiceSubmission
        ADD LastErrorStage NVARCHAR(40) NULL;

IF COL_LENGTH(N'dbo.OmidInvoiceSubmission', N'LastInquiryResponseJson') IS NULL
    ALTER TABLE dbo.OmidInvoiceSubmission
        ADD LastInquiryResponseJson NVARCHAR(MAX) NULL;

IF COL_LENGTH(N'dbo.OmidInvoiceSubmission', N'LastInquiryUtc') IS NULL
    ALTER TABLE dbo.OmidInvoiceSubmission
        ADD LastInquiryUtc DATETIME2(3) NULL;

IF OBJECT_ID(N'dbo.OmidApiAttemptLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OmidApiAttemptLog
    (
        Id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_OmidApiAttemptLog PRIMARY KEY,
        InvoiceId UNIQUEIDENTIFIER NULL,
        Operation NVARCHAR(40) NOT NULL,
        Endpoint NVARCHAR(200) NOT NULL,
        HttpStatusCode INT NULL,
        Outcome NVARCHAR(30) NOT NULL,
        ErrorMessage NVARCHAR(2000) NOT NULL,
        CreatedUtc DATETIME2(3) NOT NULL
            CONSTRAINT DF_OmidApiAttemptLog_CreatedUtc
            DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_OmidApiAttemptLog_InvoiceId_CreatedUtc
        ON dbo.OmidApiAttemptLog (InvoiceId, CreatedUtc DESC);
END;

COMMIT TRANSACTION;
