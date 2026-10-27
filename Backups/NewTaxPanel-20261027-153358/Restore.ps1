$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$files = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'files.txt')
foreach ($file in $files) {
    $source = Join-Path $PSScriptRoot (Split-Path $file -Leaf)
    if (-not (Test-Path -LiteralPath $source)) { throw "Missing backup: $source" }
}
foreach ($file in $files) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot (Split-Path $file -Leaf)) -Destination (Join-Path $projectRoot $file) -Force
}
Write-Output 'Previous NewTax invoice panel restored. Rebuild the application to apply Razor changes.'
