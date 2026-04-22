param(
    [string]$GameRoot = $env:LAV_GAME_ROOT
)
if (-not $GameRoot) { $GameRoot = "E:\SteamLibrary\steamapps\common\Lords & Villeins" }

$log = Join-Path $GameRoot "BepInEx\LogOutput.log"
if (-not (Test-Path $log)) { throw "Log file not found: $log. Launch the game once first." }

Write-Host "Tailing $log (Ctrl+C to stop)..." -ForegroundColor Cyan
Get-Content -Path $log -Wait -Tail 30
