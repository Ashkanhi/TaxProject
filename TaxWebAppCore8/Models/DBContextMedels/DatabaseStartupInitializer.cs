using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace TaxWebAppCore8.Models.DBContextMedels;

public static class DatabaseStartupInitializer
{
    private const string EnsureTaxGovermentSaveInvoiceLogInsertSql = """
        SET NOCOUNT ON;

        DECLARE @LockResult INT;
        DECLARE @Created BIT = 0;

        EXEC @LockResult = sys.sp_getapplock
            @Resource = N'TaxWebAppCore8.DatabaseStartupInitializer',
            @LockMode = N'Exclusive',
            @LockOwner = N'Session',
            @LockTimeout = 15000;

        IF @LockResult < 0
            THROW 51000, 'Could not acquire the database initialization lock.', 1;

        BEGIN TRY
            IF OBJECT_ID(N'[dbo].[TaxGovermentSaveInvoiceLogInsert]', N'P') IS NULL
            BEGIN
                EXEC sys.sp_executesql N'
                    CREATE PROCEDURE [dbo].[TaxGovermentSaveInvoiceLogInsert]
                        @ReferenceNumber NVARCHAR(100),
                        @Status NVARCHAR(50),
                        @UId NVARCHAR(100),
                        @Data NVARCHAR(MAX),
                        @PacketType NVARCHAR(50)
                    AS
                    BEGIN
                        SET NOCOUNT ON;

                        DECLARE @TaxGovermentInvoiceInfoId BIGINT;

                        SELECT @TaxGovermentInvoiceInfoId = TaxGovermentInvoiceInfoId
                        FROM [dbo].[TaxGovermentInvoiceInfo]
                        WHERE ReferenceNumber = @ReferenceNumber;

                        INSERT INTO [dbo].[TaxGovermentSendLog]
                            (TaxGovermentInvoiceInfoId, RefrenceId, [Status], [UId], [Date], Json, PacketType)
                        SELECT
                            @TaxGovermentInvoiceInfoId,
                            @ReferenceNumber,
                            @Status,
                            @UId,
                            GETDATE(),
                            @Data,
                            @PacketType;
                    END';

                SET @Created = 1;
            END;

            EXEC sys.sp_releaseapplock
                @Resource = N'TaxWebAppCore8.DatabaseStartupInitializer',
                @LockOwner = N'Session';

            SELECT @Created;
        END TRY
        BEGIN CATCH
            EXEC sys.sp_releaseapplock
                @Resource = N'TaxWebAppCore8.DatabaseStartupInitializer',
                @LockOwner = N'Session';
            THROW;
        END CATCH;
        """;

    public static void EnsureRequiredObjects(string? connectionString)
    {
        var logger = ClsTools.Logger.NPSLogger.GetLogger(
            ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogError(
                $"{nameof(DatabaseStartupInitializer)}::{MethodBase.GetCurrentMethod()?.Name}:: " +
                "Database initialization was skipped because the connection string is empty.");
            return;
        }

        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = new SqlCommand(EnsureTaxGovermentSaveInvoiceLogInsertSql, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = 30
            };

            var wasCreated = Convert.ToBoolean(command.ExecuteScalar());
            logger.LogInfoMessage(
                $"{nameof(DatabaseStartupInitializer)}::TaxGovermentSaveInvoiceLogInsert::" +
                (wasCreated ? "Created" : "AlreadyExists"));
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"{nameof(DatabaseStartupInitializer)}::{MethodBase.GetCurrentMethod()?.Name}:: " +
                $"Could not ensure TaxGovermentSaveInvoiceLogInsert: {ex.Message}",
                ex);
        }
    }
}
