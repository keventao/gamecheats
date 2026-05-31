param(
    [string]$GameRoot = $env:LUNHUI_GAME_ROOT
)

if (-not $GameRoot) {
    Write-Error "GameRoot not set. Pass -GameRoot or set `$env:LUNHUI_GAME_ROOT."
    Write-Host "Example: powershell tools/install-bepinex.ps1 -GameRoot '<STEAM>\steamapps\common\轮回修仙路'"
    exit 1
}

$zipPath = "$PSScriptRoot\BepInEx-Unity.IL2CPP-win-x64.zip"
if (-not (Test-Path $zipPath)) {
    Write-Error "BepInEx zip not found: $zipPath"
    exit 1
}

Write-Host "Installing BepInEx 6 IL2CPP to: $GameRoot"
Write-Host "Extracting from: $zipPath"

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $GameRoot)

Write-Host ""
Write-Host "BepInEx installed successfully!"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Launch the game once from Steam (BepInEx will generate IL2CPP proxies, then exit)."
Write-Host "  2. Verify BepInEx/LogOutput.log shows no errors."
Write-Host "  3. Install the mod: powershell tools/install.ps1 -GameRoot '$GameRoot'"
Write-Host ""
