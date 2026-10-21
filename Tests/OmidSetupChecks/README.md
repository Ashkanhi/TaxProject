# Omid setup checks

The Omid-only menu **ستاپ و بررسی دیتابیس** opens `/OmidSetup`.
The checklist covers the current Omid send/inquiry flow and its sales database dependencies,
not every object used by other service providers. Reading the checklist performs no DDL.

Definitions that can be created: `OmidInvoiceSubmission`, `OmidApiAttemptLog`,
`TaxHistory`, `tTaxHistoryStateParallel`, `tTisTaxErrorType`,
`vw_TaxGovermentInvoiceHeaderForShow`, `Usp_TaxCartable_DetailInfo`.
Sales-system source objects are checked but must be installed with the sales system's scripts.
Creating empty `TaxHistory` does not populate invoice history; existing data ingestion still applies.

Existing objects are never dropped, altered, or emptied. Column mismatches require a separate,
reviewed migration. Table contracts check SQL type/length/precision, nullability, identity,
single-column primary keys and required defaults. Source dependencies check required column names;
views are also compiled with `SELECT TOP (0)` to catch broken dependencies. Procedure checks
validate its three input parameters; they do not execute it or prove business correctness.
The database login needs metadata visibility/read access for checks and DDL privileges for creation.

The only SQL accepted for creation is the server-side catalog. Requests require an antiforgery
token, a valid system-tools password session, and `ServiceType=Omid`. Creation uses a transaction and an application lock, checks
dependencies, verifies the created object, and rolls back on failure. Set `SystemTools:Password`
in the web project's `appsettings.json`; a blank password disables access. Each tools menu click
asks for the password. Grants are tool-specific and expire after 15 minutes. The existing
localhost-only restriction on system logs also remains in place.

Run after building the web project:

```powershell
dotnet run --project Tests/OmidSetupChecks/OmidSetupChecks.csproj
node --check TaxWebAppCore8/wwwroot/Content/MyJS/OmidSetup.js
```

These tests do not connect to SQL Server. Before rollout, use a disposable copy of the customer's
sales database to verify missing objects, incorrect columns, insufficient permissions, repeated
creation, missing dependencies, and rollback. Do not start the web application against a test target
without accounting for its pre-existing `DatabaseStartupInitializer`, which runs DDL at startup.
