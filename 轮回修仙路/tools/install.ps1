param(
    [string]$GameRoot = $env:LUNHUI_GAME_ROOT
)

if (-not $GameRoot) {
    Write-Error "GameRoot not set. Pass -GameRoot or set `$env:LUNHUI_GAME_ROOT."
    exit 1
}

$src = "$PSScriptRoot\..\src\LunHuiCheats\bin\Release\netstandard2.1\LunHuiCheats.dll"
$dest = "$GameRoot\BepInEx\plugins\LunHuiCheats\LunHuiCheats.dll"

if (-not (Test-Path $src)) {
    Write-Error "Build output not found: $src. Run 'dotnet build -c Release' first."
    exit 1
}

$destDir = Split-Path $dest
if (-not (Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

Copy-Item -Path $src -Destination $dest -Force
Write-Host "Installed to $dest"
