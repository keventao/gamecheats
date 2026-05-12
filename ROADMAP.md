# gamecheats Roadmap

Last updated: 2026-05-12

This root roadmap is an index and cross-project priority view. Detailed status, implemented features, verification state, risks, and next work live in each project-level `ROADMAP.md`.

## Current Projects

| Project | Type | Current Status | Roadmap |
|---|---|---|---|
| `Humanica/` | MelonLoader + HarmonyX IL2CPP mod | v0.1.1 stable local build; warehouse workflow accepted; unlock still needs deeper smoke test. | `Humanica/ROADMAP.md` |
| `LordsAndVilleins/` | BepInEx + Harmony Unity Mono mod | v0.1.1 partially in-game verified; Economy, Time, Royalty pass; Pawn and Build still need smoke tests. | `LordsAndVilleins/ROADMAP.md` |
| `For The King/` | BepInEx + Harmony Unity Mono mod | v0.1.0 skeleton with Time and Player HP controls; in-game smoke pending. | `For The King/ROADMAP.md` |
| `Timberborn/` | Official Blueprint JSON mod | `KKDoubleResources` validates; `all-in-one-gen` v0.1.4 uses faction-specific output lists in a dedicated `KK` tool group. In-game disposable-save smoke pending. | `Timberborn/ROADMAP.md` |
| `spacehaven/` | Save editor + Space Haven Mod Loader XML workspace | Save editor present; `KK Resource Tuning x2` XML mod generated and locally installed for testing. | `spacehaven/ROADMAP.md` |
| `fightlife mods/` | Unity Mono managed DLL package | Packaged Windows install files present; source for `CheatMenu.dll` still needs recovery/recreation. | `fightlife mods/ROADMAP.md` |

## Cross-Project Priorities

1. Smoke test Space Haven `KK Resource Tuning x2` through the modloader.
2. Finish Humanica unlock verification and expose save-hook status in-panel.
3. Run Lords & Villeins pending Pawn and Build smoke checks.
4. Smoke test For The King's BepInEx load, F1 panel, time controls, heal, and HP lock.
5. Smoke test Timberborn `KKDoubleResources` and `all-in-one-gen` in a disposable save.
6. Recover or recreate FightLife `CheatMenu.dll` source and add safer installer/uninstaller scripts.

## Shared Standards

- Keep project-specific progress in each game's `ROADMAP.md`.
- Keep generated mods, local saves, backups, game assemblies, logs, and extracted assets out of git unless intentionally tracking a packaged artifact.
- Test destructive or save-affecting behavior only on disposable or backed-up saves.
- Prefer XML/save-editor approaches before runtime code injection when the target game supports them.
- For every new feature, add at least one of:
  - lightweight no-game test
  - static validation script
  - smoke-checklist item
  - log marker expectation

## Project Roadmaps

- `Humanica/ROADMAP.md`
- `LordsAndVilleins/ROADMAP.md`
- `For The King/ROADMAP.md`
- `Timberborn/ROADMAP.md`
- `spacehaven/ROADMAP.md`
- `fightlife mods/ROADMAP.md`

## Supporting Docs

- `Humanica/README.md`
- `LordsAndVilleins/README.md`
- `For The King/README.md`
- `Timberborn/README.md`
- `spacehaven/README.md`
- `spacehaven/MODDING.md`
- `fightlife mods/README-安装说明.txt`
