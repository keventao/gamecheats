# Build Script for 轮回修仙路 Cheats
# Run this in PowerShell on Windows

$ErrorActionPreference = "Stop"

# Set game root (adjust to your Steam install, or pre-set $env:LUNHUI_GAME_ROOT)
if (-not $env:LUNHUI_GAME_ROOT) {
    $env:LUNHUI_GAME_ROOT = "<STEAM>\steamapps\common\轮回修仙路"
}

Write-Host "Building LunHuiCheats..." -ForegroundColor Cyan
Write-Host "GameRoot: $env:LUNHUI_GAME_ROOT"

# Navigate to project directory
$projectDir = Split-Path -Parent $PSScriptRoot
$srcDir = Join-Path $projectDir "src\LunHuiCheats"

cd $srcDir

# Restore and build
dotnet restore
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

Write-Host ""
Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

# Install to game
$pluginDir = "$env:LUNHUI_GAME_ROOT\BepInEx\plugins\LunHuiCheats"
if (-not (Test-Path $pluginDir)) {
    New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
}

$dllSource = "$srcDir\bin\Release\netstandard2.1\LunHuiCheats.dll"
$dllDest = "$pluginDir\LunHuiCheats.dll"

Copy-Item -Path $dllSource -Destination $dllDest -Force
Write-Host "Installed to: $dllDest" -ForegroundColor Green
Write-Host ""
Write-Host "Next: Launch the game from Steam and press P to toggle the cheat panel." -ForegroundColor Cyan
