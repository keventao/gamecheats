# try-trainer-current.ps1 — runbook (for opencode / Windows operator)

## Why
We want the third-party trainer's **feature catalog** — its categories, sorting,
and feature list — to clone the UX into `LunHuiCheats`. That catalog is encrypted
inside the trainer's Agile.NET VM, so we can't read it statically. But the **panel
UI is independent of the game version**: even on the current build, where the
cheats themselves likely no longer work, the menu should still render. Launching
it once and screenshotting the menu gets us the catalog cheaply.

## What the script does (non-destructive)
1. Backs up the save folder to `%TEMP%\lunhui-trainer-test\save-backup`.
2. **Mirrors** the game into `%TEMP%\lunhui-trainer-test\game` (a throwaway copy).
3. Strips any existing BepInEx/doorstop from the copy.
4. Extracts the bundled trainer (`基础功能库 1.0.4`) onto the copy.
5. Launches the copy and captures `BepInEx/LogOutput.log`.

Undo everything by deleting `%TEMP%\lunhui-trainer-test`. The real game install
is untouched unless you pass `-NoCopy`.

## Run
```powershell
# from the repo root, on the Windows machine with the game installed
powershell -ExecutionPolicy Bypass -File "轮回修仙路/tools/try-trainer-current.ps1" -GameRoot "<STEAM>\steamapps\common\轮回修仙路"
```
- Steam client should be running (the sandbox copy needs it for `steam_api`).
- First IL2CPP launch builds interop assemblies — can take several minutes; the
  window may look frozen. Bump `-TimeoutSec 360` if the log looks unfinished.
- The trainer needs its SenseShield license; clear any activation prompt.

## What to capture and hand back
1. **If the panel opens in-game** — the trainer's toggle key is unknown; try, in
   order: `Insert`, `Home`, `Delete`, `F1`–`F4`, `RightShift`, `` ` `` (Backquote).
   Then screenshot **every** category tab, the sort control, and the item/feature
   browser. Save shots to `%TEMP%\lunhui-trainer-test\out`.
2. **Always** hand back `%TEMP%\lunhui-trainer-test\game\BepInEx\LogOutput.log`
   (the script also copies it to `...\out`).
3. **If nothing opens** — the log explains why (most likely the 2022 BepInEx
   build won't load on the current Unity/IL2CPP version). Paste the log back.

## Fallback
If the sandbox copy refuses to start (Steam DRM kills a copied exe), rerun with
`-NoCopy` to install into the real game dir and launch via Steam. The save is
still backed up; remove the trainer files afterward (`BepInEx`, `winhttp.dll`,
`doorstop_config.ini`, `.doorstop_version`, `dotnet`, `mono`, `SenseShield`,
`steam_appid.txt`).
