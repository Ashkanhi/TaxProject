$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$source = Get-Content (Join-Path $repo 'TaxWebAppCore8/BI/Omid/OmidSubmissionStore.cs') -Raw
$method = $source.Substring($source.IndexOf('public async Task<(string State, string? Message)?> SaveInquiryResultAsync'))
$sql = [regex]::Match($method, 'const string sql = @"(?<sql>[\s\S]*?)";').Groups['sql'].Value
if (-not $sql.StartsWith("`r`nUPDATE") -and -not $sql.StartsWith("`nUPDATE")) { throw 'Unexpected storage query' }
# تنها جدول موقت همین اتصال تغییر می‌کند؛ هیچ رکورد واقعی خوانده یا ویرایش نمی‌شود.
$sql = $sql.Replace('dbo.OmidInvoiceSubmission', '#SubmissionCheck')
[xml]$config = Get-Content (Join-Path $repo 'TaxWebAppCore8/bin/Debug/net8.0/Resources/CustomerID/CustomerInformation.xml') -Raw
$settings = $config.DTOServerInfo
$builder = [System.Data.SqlClient.SqlConnectionStringBuilder]::new()
$builder['Data Source'] = $settings.DataBaseServer
$builder['Initial Catalog'] = $settings.DbName
$builder['User ID'] = $settings.UserId
$builder['Password'] = $settings.UserPass
$builder['TrustServerCertificate'] = $true
$builder['Connect Timeout'] = 10
$connection = [System.Data.SqlClient.SqlConnection]::new($builder.ConnectionString)
try {
    $connection.Open()
    $create = $connection.CreateCommand()
    $create.CommandText = @'
CREATE TABLE #SubmissionCheck (
 InvoiceId uniqueidentifier NOT NULL, RequestId nvarchar(100), State nvarchar(20),
 LastError nvarchar(1000), LastErrorStage nvarchar(30), LastInquiryResponseJson nvarchar(max),
 LastInquiryUtc datetime2, UpdatedUtc datetime2);
'@
    $null = $create.ExecuteNonQuery()
    $id = [Guid]::NewGuid()
    $receipt = [Guid]::NewGuid().ToString()
    foreach ($case in @(
        @{ Before='Accepted'; After='Pending'; Expected='Accepted'; Stale=$false },
        @{ Before='Failed'; After='Pending'; Expected='Failed'; Stale=$false },
        @{ Before='Pending'; After='Failed'; Expected='Failed'; Stale=$false },
        @{ Before='Pending'; After='Accepted'; Expected='Pending'; Stale=$true },
        @{ Before='Sending'; After='Accepted'; Expected='Sending'; Stale=$false }
    )) {
        $seed = $connection.CreateCommand()
        $seed.CommandText = 'DELETE FROM #SubmissionCheck; INSERT INTO #SubmissionCheck (InvoiceId,RequestId,State,LastError,LastErrorStage) VALUES (@id,@receipt,@state,N''previous error'',N''Inquiry'');'
        $null=$seed.Parameters.AddWithValue('@id',$id)
        $null=$seed.Parameters.AddWithValue('@receipt',$receipt)
        $null=$seed.Parameters.AddWithValue('@state',$case.Before)
        $null=$seed.ExecuteNonQuery()
        $update=$connection.CreateCommand(); $update.CommandText=$sql
        $null=$update.Parameters.AddWithValue('@InvoiceId',$id)
        $wanted=if($case.Stale){[Guid]::NewGuid().ToString()}else{$receipt}
        $null=$update.Parameters.AddWithValue('@ExpectedRequestId',$wanted)
        $null=$update.Parameters.AddWithValue('@State',$case.After)
        $null=$update.Parameters.AddWithValue('@LastError','new result')
        $null=$update.Parameters.AddWithValue('@ResponseJson','{"test":true}')
        $null=$update.ExecuteScalar()
        $read=$connection.CreateCommand(); $read.CommandText='SELECT State,LastError,LastInquiryResponseJson FROM #SubmissionCheck;'
        $reader=$read.ExecuteReader(); $null=$reader.Read()
        if($reader.GetString(0) -ne $case.Expected){throw 'State guard failed'}
        if($case.After -eq 'Pending' -and $reader.GetString(1) -ne 'previous error'){throw 'Previous result lost'}
        if($case.Stale -and -not $reader.IsDBNull(2)){throw 'Stale response changed current receipt'}
        $reader.Close()
        Write-Output "PASS: $($case.Before) -> $($case.After), stale=$($case.Stale), stored=$($case.Expected)"
    }
} finally { $connection.Dispose() }
