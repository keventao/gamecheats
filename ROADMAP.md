# gamecheats Roadmap

Last updated: 2026-05-03

## Current Projects

| Project | Type | Status |
|---|---|---|
| `LordsAndVilleins/` | BepInEx + Harmony mod | v0.1.1 partially in-game verified. Pawn and Build still need smoke checks. |
| `For The King/` | BepInEx + Harmony mod | Project scaffold and design docs present. See `For The King/README.md`. |
| `Humanica/` | MelonLoader + HarmonyX mod | v0.1.1 partially verified. GUI, time, resources pass; village and unlock need smoke checks. |
| `fightlife mods/` | Unity Mono managed DLL injector | Packaged Windows install files and README added. |
| `spacehaven/` | Save editor | macOS app bundle plus Windows Python launcher added. |

## Recently Added

### FightLife Vanguard

- `CheatMenu.dll` managed DLL package.
- `RuntimeInitializeOnLoads.json` and `ScriptingAssemblies.json` install files.
- Chinese install and uninstall notes in `fightlife mods/README-安装说明.txt`.
- Features documented: heal team, add gold, 3x speed, 3x damage.

### Space Haven

- Offline save editor for bank, crew, and ship resources.
- macOS double-click app under `spacehaven/mac/SpaceHavenEditor.app`.
- Windows Python launcher under `spacehaven/windows/运行.bat`.
- Resource name tables generated from `spacehaven.jar`.
- Save writes create timestamped backups.

## Near-Term Tasks

- Add project-specific smoke notes for `fightlife mods/` after another game launch check.
- Add Space Haven sample-save regression checks for XML parsing and resource edits.
- Decide whether packaged app bundles should stay in git or move to GitHub Releases once size grows.
- Keep `.bak`, save files, game assemblies, local paths, and OS metadata out of commits.

## Project Roadmaps

- `LordsAndVilleins/ROADMAP.md`
- `Humanica/ROADMAP.md`
- `For The King/README.md`
- `spacehaven/README.md`
- `fightlife mods/README-安装说明.txt`
