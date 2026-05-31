param(
    [string]$GameRoot = $env:LUNHUI_GAME_ROOT
)

if (-not $GameRoot) {
    Write-Error "GameRoot not set. Pass -GameRoot or set `$env:LUNHUI_GAME_ROOT."
    exit 1
}

$log = "$GameRoot\BepInEx\LogOutput.log"
if (-not (Test-Path $log)) {
    Write-Error "Log not found: $log"
    exit 1
}

Get-Content -Path $log -Wait -Tail 20
