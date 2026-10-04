#Requires -Version 7
param(
  [string]$GameRoot = $env:ZEDZONE_GAME_ROOT,
  [string]$Configuration = "Release"
)
if ([string]::IsNullOrEmpty($GameRoot)) {
  $GameRoot = "D:\Program Files (x86)\Steam\steamapps\common\ZED ZONE"
}
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$dll = Join-Path $here "..\src\ZedZoneHpLock\bin\$Configuration\net6.0\ZedZoneHpLock.dll"
if (-not (Test-Path $dll)) { throw "build first: dotnet build $here\..\src\ZedZoneHpLock -c $Configuration" }
$dest = Join-Path $GameRoot "BepInEx\Plugins\ZedZoneHpLock.dll"
for ($i = 0; $i -lt 15; $i++) {
  try { Copy-Item $dll $dest -Force; Write-Host "installed"; break }
  catch { Write-Host ("try " + $i + " locked, game may be running"); Start-Sleep 2 }
}
