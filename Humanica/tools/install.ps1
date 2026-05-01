# install.ps1 — 编译并安装 HumanicaCheats 到游戏 Mods 目录
param(
    [string]$Config = "Debug",
    [string]$GameRoot = "E:\Games\Humanica"
)

$proj = "$PSScriptRoot\..\src\HumanicaCheats\HumanicaCheats.csproj"
$dest = "$GameRoot\Mods"

Write-Host "Building $Config..."
& "dotnet" build $proj -c $Config /p:GameRoot=$GameRoot
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed."; exit 1 }

Write-Host "Installed to $dest\HumanicaCheats.dll"
