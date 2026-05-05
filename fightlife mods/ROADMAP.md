# FightLife Vanguard Mods Roadmap

Last updated: 2026-05-05

## Current Status

Status: packaged Windows install files are present; source for `CheatMenu.dll` is not in this repository.

Game/runtime:

- FightLife Vanguard
- Unity Mono backend
- Verified against Unity 6000.2.6f2 per install notes
- Install style: managed DLL plus Unity runtime initialization JSON files

Package files:

- `CheatMenu.dll`
- `RuntimeInitializeOnLoads.json`
- `ScriptingAssemblies.json`
- `README-安装说明.txt`

## Implemented Features

The packaged `CheatMenu.dll` exposes a bottom-right always-on UI with four controls:

1. `Heal Team` / `K` hotkey
   - Heals team to full HP.
   - Restores shield/defense per install notes.
2. `+9999 Gold`
   - Adds 9999 gold per click.
3. `3x Speed`
   - Toggles `Time.timeScale = 3` behavior.
4. `3x Damage`
   - Toggles 3x damage for friendly units known at the time of enabling.

## Verified

From `README-安装说明.txt`:

- Unity Mono backend is required.
- Unity 6000.2.6f2 was verified.
- Success marker is the bottom-right UI showing four buttons after launch.
- Log location is documented:

```text
%USERPROFILE%\AppData\LocalLow\StartImpulse\FightLife Vanguard\Player.log
```

- Search marker:

```text
[CheatMenu]
```

## Needs Smoke Test

- Confirm game still uses Unity Mono backend and has `FightLife Vanguard_Data/Managed`.
- Install package on a disposable copy or after backing up original files.
- Confirm the bottom-right UI appears.
- Test each button:
  - Heal Team heals damaged living friendly units.
  - `+9999 Gold` stacks on repeated clicks.
  - `3x Speed` toggles on/off and can be re-enabled after pause/menu resets.
  - `3x Damage` applies to current friendly units.
- Check `Player.log` for `[CheatMenu]` errors.

## Known Risks

- The source code for `CheatMenu.dll` is not present, so future changes require recovering or recreating source.
- `3x Damage` only applies to friendly units present when toggled on; new units, new levels, or reloads may need OFF/ON reapply.
- `3x Speed` can be reset by pause menus or game systems.
- Heal does not revive dead units with `CurHP=0`.
- Heal does not lower HP if a buff already pushed HP above base max.
- Install overwrites Unity runtime JSON files, so backups are required.
- Committing DLLs is normally avoided by root `.gitignore`, but this package already tracks `CheatMenu.dll` as a released artifact.

## Next Work

1. Recover or recreate source code for `CheatMenu.dll` and store it under a proper source directory.
2. Add a checksum/version note for the packaged DLL.
3. Add a smoke checklist file with install, feature, uninstall, and log checks.
4. Add a safer installer/uninstaller script to back up and restore JSON files automatically.
5. Re-test after each FightLife Vanguard update.

## Project Links

- `README-安装说明.txt` - Chinese install and usage notes.
- `RuntimeInitializeOnLoads.json` - Unity runtime initialization registration.
- `ScriptingAssemblies.json` - managed assembly registration.
