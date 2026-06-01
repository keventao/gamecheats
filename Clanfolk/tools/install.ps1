# install.ps1 — 编译并安装 ClanfolkCheats 到游戏 Mods 目录
param(
    [string]$Config = "Debug",
    [string]$GameRoot = $env:CLANFOLK_GAME_ROOT
)
if (-not $GameRoot) { throw "Set -GameRoot or CLANFOLK_GAME_ROOT to your Clanfolk install folder." }

$proj = "$PSScriptRoot\..\src\ClanfolkCheats\ClanfolkCheats.csproj"
$dest = "$GameRoot\Mods\ClanfolkCheats.dll"

$dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue)?.Source
if (-not $dotnet) { Write-Error "dotnet not found — check PATH or install .NET SDK"; exit 1 }

Write-Host "Building $Config (using $dotnet)..."
& $dotnet build $proj -c $Config /p:GameRoot=$GameRoot
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed."; exit 1 }

if (Test-Path $dest) {
    $size = (Get-Item $dest).Length
    Write-Host "Installed: $dest ($size bytes)"
} else {
    Write-Error "Build succeeded but $dest not found — check csproj CopyToMods target"
    exit 1
}
