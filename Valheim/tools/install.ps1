# Copies the built EasySpawner (zh) DLL into BepInEx/plugins/EasySpawner/.
# Usage:
#   powershell tools/install.ps1
#   powershell tools/install.ps1 -GameRoot "D:\SteamLibrary\steamapps\common\Valheim"
param(
    [string]$GameRoot = $env:VALHEIM_GAME_ROOT
)

$ErrorActionPreference = "Stop"

if (-not $GameRoot) {
    throw "Game root not set. Pass -GameRoot or define VALHEIM_GAME_ROOT."
}

$pluginDir = Join-Path $GameRoot "BepInEx\plugins\EasySpawner"
$dll = Join-Path $PSScriptRoot "..\src\EasySpawner\bin\Release\EasySpawner.dll"
$dll = [System.IO.Path]::GetFullPath($dll)

if (-not (Test-Path $dll)) {
    throw "Build output not found: $dll. Run 'dotnet build -c Release' first."
}

New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null

$target = Join-Path $pluginDir "EasySpawner.dll"
if (Test-Path $target) {
    $hashOld = (Get-FileHash $target).Hash
    $hashNew = (Get-FileHash $dll).Hash
    if ($hashOld -ne $hashNew) {
        Copy-Item $target "$target.bak" -Force
        Write-Host "Backed up existing DLL to $target.bak"
    }
}

Copy-Item $dll $target -Force
Write-Host "Installed: $target"
