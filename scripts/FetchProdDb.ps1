param(
    [string]$BACKEND_URL = $env:BACKEND_URL,
    [string]$BACKEND_TOKEN = $env:BACKEND_TOKEN
)

if (-not $BACKEND_URL) {
    $BACKEND_URL = "http://127.0.0.1:8000"
}

if (-not $BACKEND_TOKEN) {
    Write-Error "BACKEND_TOKEN environment variable is required"
    exit 1
}

$dbEndpoint = "$BACKEND_URL/thai/db"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
$dbTarget = Join-Path $repoRoot "ThaiReference\Resources\Raw\thai_sqlite.db"

Write-Host "Fetching DB copy from $dbEndpoint to $dbTarget"

try {
    Invoke-WebRequest -Uri $dbEndpoint -Headers @{ "Authorization" = "Bearer $BACKEND_TOKEN" } -OutFile $dbTarget -UseBasicParsing
    Write-Host "Download complete."
} catch {
    Write-Error "Failed to download DB: $($_.Exception.Message)"
    exit 1
}

