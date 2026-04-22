param(
    [string]$GameRoot = $env:LAV_GAME_ROOT,
    [int]$WaitSeconds = 30
)
if (-not $GameRoot) { $GameRoot = "<LAV_GAME_ROOT>" }

$ErrorActionPreference = "Stop"
$exe = Join-Path $GameRoot "Lords and Villeins.exe"
$log = Join-Path $GameRoot "BepInEx\LogOutput.log"

# Reinstall to ensure latest build is in place
& "$PSScriptRoot\install.ps1" -GameRoot $GameRoot
if (-not $?) { throw "install.ps1 failed." }

# Rotate old log
if (Test-Path $log) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    Move-Item $log "$log.$stamp.bak" -Force
}

# Launch
$proc = Start-Process -FilePath $exe -PassThru
Write-Host "Game launched (PID $($proc.Id)). Waiting $WaitSeconds seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds $WaitSeconds

# Kill
if (-not $proc.HasExited) {
    Stop-Process -Id $proc.Id -Force
    Write-Host "Game terminated." -ForegroundColor Cyan
}

# Wait briefly for log flush
Start-Sleep -Seconds 2

# Parse
if (-not (Test-Path $log)) { throw "Log file was not created: $log" }
$content = Get-Content $log -Raw

$summaryMatch = [regex]::Match($content, "Patch summary: (\d+)/(\d+) ok, (\d+) broken")
$readyMatch   = [regex]::Match($content, "Lords & Villeins Cheats v[\d\.]+ ready")
$errorCount   = ([regex]::Matches($content, "\[Error|\[Fatal")).Count

Write-Host ""
Write-Host "=== run-and-check report ===" -ForegroundColor Yellow
if ($summaryMatch.Success) {
    Write-Host "Patch summary: $($summaryMatch.Groups[1].Value)/$($summaryMatch.Groups[2].Value) ok, $($summaryMatch.Groups[3].Value) broken"
} else {
    Write-Host "Patch summary line NOT found." -ForegroundColor Red
}
Write-Host "'ready' line found: $($readyMatch.Success)"
Write-Host "Error/Fatal log lines: $errorCount"

if (-not $summaryMatch.Success -or -not $readyMatch.Success -or $errorCount -gt 0) {
    Write-Host "RESULT: FAIL" -ForegroundColor Red
    exit 1
}
$brokenCount = [int]$summaryMatch.Groups[3].Value
if ($brokenCount -gt 0) {
    Write-Host "RESULT: WARN (broken patches present)" -ForegroundColor Yellow
    exit 2
}
Write-Host "RESULT: PASS" -ForegroundColor Green
exit 0
