-- افزودن RequestId امید به جدول محلی ارسال‌ها.
-- این شناسه از پاسخ SendInvoice می‌آید و برای Invoiceinquiry لازم است.
IF OBJECT_ID(N'dbo.OmidInvoiceSubmission', N'U') IS NULL
BEGIN
    THROW 51000, N'Table dbo.OmidInvoiceSubmission does not exist.', 1;
END;
GO

IF COL_LENGTH(N'dbo.OmidInvoiceSubmission', N'RequestId') IS NULL
BEGIN
    ALTER TABLE dbo.OmidInvoiceSubmission
        ADD RequestId NVARCHAR(100) NULL;
END;
GO
