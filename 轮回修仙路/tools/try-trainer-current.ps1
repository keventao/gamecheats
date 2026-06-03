<#
.SYNOPSIS
    Lay the bundled third-party trainer (基础功能库 1.0.4) onto the CURRENT game
    version in a throwaway sandbox, launch it, and capture evidence.

.DESCRIPTION
    Goal: find out whether the trainer's panel (categories / sorting / feature
    list) still renders on the current game build, even if the cheats themselves
    no longer work. The panel UI does not depend on the game version, so a
    successful launch gives us the trainer's feature CATALOG — the thing we want
    to clone into LunHuiCheats.

    This script is non-destructive by default:
      * It MIRRORS the game into %TEMP%\lunhui-trainer-test\game (a copy).
      * It BACKS UP the save folder to %TEMP%\lunhui-trainer-test\save-backup.
      * It never touches the real game install unless you pass -NoCopy.
      * With -NoCopy, existing loader files are preserved unless you also pass
        -AllowRealInstallOverwrite; overwritten files are moved to a backup first.

    Delete %TEMP%\lunhui-trainer-test to undo everything.

.PARAMETER GameRoot
    The real install of the CURRENT game version. Defaults to $env:LUNHUI_GAME_ROOT.

.PARAMETER TimeoutSec
    Seconds to wait after launch before tailing the log. First-run BepInEx 6
    IL2CPP generates interop assemblies and can take several minutes; bump this
    if the log looks unfinished. Default 180.

.PARAMETER NoCopy
    Install the trainer directly into the real GameRoot instead of a sandbox
    copy, and launch via Steam. Use only if the sandboxed copy refuses to start
    (Steam DRM). The save is still backed up first.

.PARAMETER AllowRealInstallOverwrite
    Required with -NoCopy when existing loader artifacts are present. Existing
    artifacts are moved to a timestamped backup under %TEMP%\lunhui-trainer-test
    before the trainer package is extracted.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/try-trainer-current.ps1 -GameRoot "<STEAM>\steamapps\common\轮回修仙路"
#>
param(
    [string]$GameRoot = $env:LUNHUI_GAME_ROOT,
    [int]$TimeoutSec = 180,
    [switch]$NoCopy,
    [switch]$AllowRealInstallOverwrite
)

$ErrorActionPreference = "Stop"

function Info($m)  { Write-Host "[try-trainer] $m" -ForegroundColor Cyan }
function Warn($m)  { Write-Host "[try-trainer] $m" -ForegroundColor Yellow }
function Good($m)  { Write-Host "[try-trainer] $m" -ForegroundColor Green }

function Get-Sha256($path) {
    (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
}

# ---------------------------------------------------------------------------
# 0. Validate inputs
# ---------------------------------------------------------------------------
if (-not $GameRoot) {
    Write-Error "GameRoot not set. Pass -GameRoot or set `$env:LUNHUI_GAME_ROOT."
    Write-Host "Example: powershell tools/try-trainer-current.ps1 -GameRoot '<STEAM>\steamapps\common\轮回修仙路'"
    exit 1
}
if (-not (Test-Path $GameRoot)) {
    Write-Error "GameRoot does not exist: $GameRoot"
    exit 1
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$trainerZip = Get-ChildItem -Path $repoRoot -Filter "*基础功能库*_BepInEx版.zip" -File |
    Select-Object -First 1
if (-not $trainerZip) {
    Write-Error "Trainer zip (*基础功能库*_BepInEx版.zip) not found in repo root: $repoRoot"
    exit 1
}
Info "Trainer zip:  $($trainerZip.FullName)"
Info "Trainer SHA256: $(Get-Sha256 $trainerZip.FullName)"
Info "Game root:    $GameRoot"

# Workspace under %TEMP% so nothing pollutes git or the real install.
$base       = Join-Path $env:TEMP "lunhui-trainer-test"
$sandbox    = Join-Path $base "game"
$saveBackup = Join-Path $base "save-backup"
$outDir     = Join-Path $base "out"
New-Item -ItemType Directory -Path $base, $outDir -Force | Out-Null
Info "Workspace:    $base   (delete this folder to undo everything)"

# ---------------------------------------------------------------------------
# 1. Back up the save folder
# ---------------------------------------------------------------------------
$saveDir = Join-Path $env:USERPROFILE "AppData\LocalLow\烟水寒\轮回修仙路"
if (Test-Path $saveDir) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $dest  = Join-Path $saveBackup $stamp
    Info "Backing up save: $saveDir -> $dest"
    robocopy $saveDir $dest /E /NFL /NDL /NJH /NJS /NP | Out-Null
    Good "Save backed up."
} else {
    Warn "Save folder not found (game may not have run yet): $saveDir"
}

# ---------------------------------------------------------------------------
# 2. Decide install target
# ---------------------------------------------------------------------------
if ($NoCopy) {
    $target = $GameRoot
    Warn "-NoCopy: installing trainer INTO the real game dir: $target"
} else {
    $target = $sandbox
    Info "Mirroring game into sandbox (this can be several GB / minutes)..."
    New-Item -ItemType Directory -Path $sandbox -Force | Out-Null
    # /MIR mirrors; /XJ skips junctions; quiet flags keep the log readable.
    robocopy $GameRoot $sandbox /MIR /XJ /NFL /NDL /NJH /NJS /NP /R:1 /W:1 | Out-Null
    if ($LASTEXITCODE -ge 8) {
        Write-Error "robocopy failed mirroring the game (exit $LASTEXITCODE)."
        exit 1
    }
    Good "Game mirrored to sandbox."
}

# ---------------------------------------------------------------------------
# 3. Strip any existing mod loader from the target
# ---------------------------------------------------------------------------
Info "Removing any existing loader artifacts from target..."
$loaderArtifacts = @(
    "BepInEx", "dotnet", "winhttp.dll", "doorstop_config.ini",
    ".doorstop_version", "changelog.txt", ".doorstop_version.txt"
)
$existingArtifacts = @()
foreach ($a in $loaderArtifacts) {
    $p = Join-Path $target $a
    if (Test-Path $p) { $existingArtifacts += $p }
}
if ($NoCopy -and $existingArtifacts.Count -gt 0 -and -not $AllowRealInstallOverwrite) {
    Write-Error @"
-NoCopy would replace existing loader artifacts in the real game install:
$($existingArtifacts -join "`n")

Re-run with -AllowRealInstallOverwrite to move them to a timestamped backup first,
or omit -NoCopy to use the sandbox copy.
"@
    exit 1
}
if ($NoCopy -and $existingArtifacts.Count -gt 0) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $artifactBackup = Join-Path $base "real-install-loader-backup-$stamp"
    New-Item -ItemType Directory -Path $artifactBackup -Force | Out-Null
    Warn "Backing up existing real-install loader artifacts to: $artifactBackup"
    foreach ($p in $existingArtifacts) {
        Move-Item -LiteralPath $p -Destination (Join-Path $artifactBackup (Split-Path -Leaf $p)) -Force
    }
    Good "Existing loader artifacts moved to backup."
} else {
    foreach ($p in $existingArtifacts) {
        Remove-Item -LiteralPath $p -Recurse -Force -ErrorAction Stop
    }
}

# ---------------------------------------------------------------------------
# 4. Extract trainer package into the target
# ---------------------------------------------------------------------------
Info "Extracting trainer package into target..."
Expand-Archive -LiteralPath $trainerZip.FullName -DestinationPath $target -Force
# Help a non-Steam launch of the sandbox copy find the running Steam client.
Set-Content -Path (Join-Path $target "steam_appid.txt") -Value "1993150" -NoNewline -Encoding ascii
Good "Trainer files in place."

# ---------------------------------------------------------------------------
# 5. Locate the game exe (Unity convention: <Name>.exe sits next to <Name>_Data)
# ---------------------------------------------------------------------------
$dataDir = Get-ChildItem -Path $target -Directory -Filter "*_Data" |
    Where-Object { $_.Name -ne "MonoBleedingEdge_Data" } |
    Select-Object -First 1
$gameExe = $null
if ($dataDir) {
    $exeName = ($dataDir.Name -replace "_Data$", "") + ".exe"
    $candidate = Join-Path $target $exeName
    if (Test-Path $candidate) { $gameExe = $candidate }
}
if (-not $gameExe) {
    # Fallback: largest exe that is not a known helper.
    $gameExe = Get-ChildItem -Path $target -Filter *.exe -File |
        Where-Object { $_.Name -notmatch "UnityCrashHandler|vcredist|crashpad|notification_helper" } |
        Sort-Object Length -Descending | Select-Object -First 1 | ForEach-Object FullName
}
if (-not $gameExe) {
    Write-Error "Could not locate the game .exe in $target"
    exit 1
}
Info "Game exe: $gameExe"

# ---------------------------------------------------------------------------
# 6. Launch
# ---------------------------------------------------------------------------
if ($NoCopy) {
    Warn "Launching via Steam (steam://run/1993150). Make sure Steam is running."
    Start-Process "steam://run/1993150"
} else {
    Warn "Launching the sandbox copy directly. Steam client should be running so steam_api can init."
    Start-Process -FilePath $gameExe -WorkingDirectory $target | Out-Null
}

# ---------------------------------------------------------------------------
# 7. Wait, then tail + collect the BepInEx log
# ---------------------------------------------------------------------------
$logPath = Join-Path $target "BepInEx\LogOutput.log"
Info "Waiting up to $TimeoutSec s for BepInEx to write its log..."
Info "(First IL2CPP run generates interop assemblies and can take minutes — the window may look frozen.)"
$elapsed = 0
while ($elapsed -lt $TimeoutSec) {
    Start-Sleep -Seconds 5
    $elapsed += 5
    if (Test-Path $logPath) {
        Write-Host "  ...log present ($elapsed s)" -ForegroundColor DarkGray
    }
}

if (Test-Path $logPath) {
    $logCopy = Join-Path $outDir ("LogOutput-" + (Get-Date -Format "yyyyMMdd-HHmmss") + ".log")
    Copy-Item $logPath $logCopy -Force
    Good "Log copied to: $logCopy"
    Write-Host ""
    Write-Host "===== last 200 lines of BepInEx/LogOutput.log =====" -ForegroundColor Magenta
    Get-Content $logPath -Tail 200
    Write-Host "===================================================" -ForegroundColor Magenta
} else {
    Warn "No BepInEx/LogOutput.log was produced."
    Warn "Likely: the 2022 BepInEx build does not load on this game's Unity/IL2CPP version,"
    Warn "or the SenseShield license prompt blocked startup."
}

# ---------------------------------------------------------------------------
# 8. Next steps for whoever is driving this machine (human or agent)
# ---------------------------------------------------------------------------
Write-Host ""
Good "Done. NEXT — capture the catalog:"
Write-Host "  1. If the trainer panel opened in-game: screenshot EVERY category tab,"
Write-Host "     the sorting control, and the item/feature browser. Save shots to:"
Write-Host "       $outDir"
Write-Host "  2. Always hand back: $logPath"
Write-Host "  3. If nothing opened, the log tells us why (paste it back)."
Write-Host "  4. To undo everything, delete: $base"
