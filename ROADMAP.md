# gamecheats Roadmap

Last updated: 2026-05-05

## Current Projects

| Project | Type | Status |
|---|---|---|
| `LordsAndVilleins/` | BepInEx + Harmony mod | v0.1.1 partially in-game verified. Pawn and Build still need smoke checks. |
| `For The King/` | BepInEx + Harmony mod | Project scaffold and design docs present. See `For The King/README.md`. |
| `Humanica/` | MelonLoader + HarmonyX mod | v0.1.1 partially verified. GUI, time, resources pass; village and unlock need smoke checks. |
| `fightlife mods/` | Unity Mono managed DLL injector | Packaged Windows install files and README added. |
| `spacehaven/` | Save editor + XML mod workspace | Save editor present; `KK Resource Tuning x2` XML mod generator implemented and locally installed for testing. |

## Recently Added

### FightLife Vanguard

- `CheatMenu.dll` managed DLL package.
- `RuntimeInitializeOnLoads.json` and `ScriptingAssemblies.json` install files.
- Local install and uninstall notes in `fightlife mods/`.
- Features documented: heal team, add gold, 3x speed, 3x damage.

### Space Haven

- Offline save editor for bank, crew, and ship resources.
- macOS double-click app under `spacehaven/mac/SpaceHavenEditor.app`.
- Windows Python launcher under `spacehaven/windows/`.
- Resource name tables generated from `spacehaven.jar`.
- Save writes create timestamped backups.
- Modloader workflow notes added in `spacehaven/MODDING.md`.
- XML runtime mod generator added: `spacehaven/tools/build_resource_yield_mod.py`.
- Generated test mod: `KK Resource Tuning x2`.
- Current XML test features:
  - Crop and Process output `howMuch` x2.
  - Crop stage `time` divided by 2.
  - Crop `<needs>` `consumeEvery` intervals x2.
  - Process `<needs>` `consumeEvery` intervals x2.
- Local generated output is ignored under `spacehaven/generated/`.
- Installed local test copy: `<SPACEHAVEN_GAME_ROOT>\mods\kk-resource-tuning-x2`.

## Near-Term Tasks

- Add project-specific smoke notes for `fightlife mods/` after another game launch check.
- Add Space Haven sample-save regression checks for XML parsing and resource edits.
- Smoke test `KK Resource Tuning x2` through Space Haven Mod Loader and record the modloader log marker.
- Verify in-game effects for doubled crop/process outputs, faster crop growth, and reduced input consumption.
- If XML tuning works, add focused presets such as output-only, crops-only, or process-only.
- Decide whether packaged app bundles should stay in git or move to GitHub Releases once size grows.
- Keep `.bak`, save files, game assemblies, local paths, and OS metadata out of commits.

## Project Roadmaps

- `LordsAndVilleins/ROADMAP.md`
- `Humanica/ROADMAP.md`
- `For The King/README.md`
- `spacehaven/README.md`
- `spacehaven/MODDING.md`
- `fightlife mods/`
