param(
    [string]$Configuration = "Release",
    [string]$GameRoot = $env:FTK_GAME_ROOT
)
if (-not $GameRoot) { $GameRoot = "<FTK_GAME_ROOT>" }

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src/ForTheKingCheats/ForTheKingCheats.csproj"
$srcDll = Join-Path $repoRoot "src/ForTheKingCheats/bin/$Configuration/net35/ForTheKingCheats.dll"
$gameExe = Join-Path $GameRoot "FTK.exe"
$loaderDll = Join-Path $GameRoot "winhttp.dll"
$coreDll = Join-Path $GameRoot "BepInEx/core/BepInEx.dll"
$pluginDir = Join-Path $GameRoot "BepInEx/plugins/ForTheKingCheats"

if (-not (Test-Path $gameExe)) {
    throw "For The King executable not found: $gameExe"
}

dotnet build $project -c $Configuration /p:GameRoot="$GameRoot"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path $srcDll)) {
    throw "Build artifact not found: $srcDll"
}

if ((-not (Test-Path $loaderDll)) -or (-not (Test-Path $coreDll))) {
    Write-Warning "BepInEx loader is incomplete. The DLL was copied for later but will not load until BepInEx 5 x64 is installed and the game is launched once."
}

New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
Copy-Item -Force $srcDll $pluginDir
Write-Host "Installed $srcDll -> $pluginDir"
