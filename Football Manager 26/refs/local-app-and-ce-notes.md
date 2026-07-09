# Local App And CE Notes

These notes are local research facts observed for this machine. They are not
proof that the three cheats are implemented.

## FM26 app

Path:

```text
/Users/zhuangtao/Library/Application Support/Steam/steamapps/common/Football Manager 26/fm.app
```

Observed bundle facts:

- `CFBundleName`: `Football Manager 26`
- `CFBundleIdentifier`: `com.SportsInteractive.FootballManager26`
- `CFBundleShortVersionString`: `26.3.2.2329565`
- `CFBundleGetInfoString`: `Unity Player version 6000.0.52f1-fm26-05f1 (87a0370e9917)`
- Executable format: universal Mach-O with `x86_64` and `arm64`
- Runtime: hardened runtime, notarized
- Unity data: IL2CPP metadata and arch-specific IL2CPP data under `Resources/Data/il2cpp_data`

## Live attach checks

With FM26 running and a save loaded:

- `probe` found PID `35912` and the expected executable path.
- User-mode `task_for_pid` failed with kern return `5`.
- Root/admin `task_for_pid` failed with the same kern return `5`.
- `/usr/bin/lldb -p 35912` failed with `Not allowed to attach to process`.
- `codesign -d --entitlements :- fm.app` reported an invalid entitlements blob
  that the OS ignores.

Current conclusion: the external memory trainer path is blocked by macOS
debug/attach policy for this hardened FM26 process unless the target app is made
debuggable.

## BepInEx route check

BepInEx v6 Unity IL2CPP docs currently state IL2CPP builds are available only
for Windows and Wine. That makes a stock macOS BepInEx IL2CPP plugin route
unavailable for this target.

## CE archive

Local archive inspected:

```text
/Users/zhuangtao/Downloads/FM26_26_3_0_fm.exe_upd_5_all_platforms.rar
```

The user provided the archive password in chat; it is intentionally not recorded
in this repo.

Extracted table path during research:

```text
/private/tmp/fm26_ce/FM26_26_3_0_fm.exe_upd_5_all_platforms/Football Manager 26 Cheat Table by tdg6661/FMCET.CT
```

Important readable CE entries:

- `Improve Team Condition` calls `improveTeamCondition('ptrClub', 10000, true)`.
- Freezer logic periodically calls `improveTeamCondition(Club, 10000, false)`.
- `Remove All Injuries` calls `removeTeamInjuries('ptrClub', 10000, true)`.
- Player-specific `Remove All Injuries` calls `removePlayerInjuries('ptrPerson', 10000)`.
- `Remove All Unhappiness` calls `removeTeamUnhappiness('ptrClub', true)`.
- Player-specific `Remove All Unhappiness` calls `removePlayerUnhappiness('ptrPerson')`.
- `Maximise Existing Training Happiness` calls `maxTeamTrainingHappiness('ptrClub', true)` and `maxTrainingHappiness('ptrPerson')`.

Readable symbolic fields:

- Club morale: `ptrClub`, offsets `cluo.Cmle`, `cluo.Cint`, byte.
- Player morale: `ptrPlayer + plao.Pmor`, byte, CE dropdown max value `20`.
- Player overall physical condition: `ptrPlayer + plao.Popc`, 2 bytes.
- Player match sharpness: `ptrPlayer + plao.Pmsh`, 2 bytes.
- Player fatigue: `ptrPlayer + plao.Pftg`, 2 bytes.
- Person happiness: `ptrPerson -> pero.Pflc -> pero.Pmsq + 0x6`.
- Person playing-time happiness: `ptrPerson -> pero.Pflc -> pero.Pmsq + 0x8`.

The core CE helper functions are encoded through Lua `decodeFunction(...)`, so
their exact writes were not verified from static text alone.
