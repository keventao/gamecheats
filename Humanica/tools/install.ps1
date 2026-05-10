# install.ps1 — 编译并安装 HumanicaCheats 到游戏 Mods 目录
# csproj 的 CopyToMods AfterTargets="Build" 会自动复制 dll，本脚本只验证产物到位。
param(
    [string]$Config = "Debug",
    [string]$GameRoot = $env:HUMANICA_GAME_ROOT
)
if (-not $GameRoot) { throw "Set -GameRoot or HUMANICA_GAME_ROOT to your Humanica install folder." }

$proj = "$PSScriptRoot\..\src\HumanicaCheats\HumanicaCheats.csproj"
$dest = "$GameRoot\Mods\HumanicaCheats.dll"

# 优先 PATH 上的 dotnet，回退到默认安装路径
$dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue)?.Source
if (-not $dotnet) { Write-Error "dotnet 未找到 — 检查 PATH 或安装 .NET SDK"; exit 1 }

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
