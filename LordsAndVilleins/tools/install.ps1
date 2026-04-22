param(
    [string]$Configuration = "Release",
    [string]$GameRoot = $env:LAV_GAME_ROOT
)
if (-not $GameRoot) { $GameRoot = "E:\SteamLibrary\steamapps\common\Lords & Villeins" }

$ErrorActionPreference = "Stop"
$repoRoot   = Split-Path -Parent $PSScriptRoot
$srcDll     = Join-Path $repoRoot "src/LordsAndVilleinsCheats/bin/$Configuration/netstandard2.1/LordsAndVilleinsCheats.dll"
$pluginDir  = Join-Path $GameRoot "BepInEx/plugins/LordsAndVilleinsCheats"

if (-not (Test-Path $srcDll)) {
    throw "Build artifact not found: $srcDll. Run 'dotnet build -c $Configuration' first."
}
New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
Copy-Item -Force $srcDll $pluginDir
Write-Host "Installed $srcDll -> $pluginDir"
