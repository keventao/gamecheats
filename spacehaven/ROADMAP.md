# Space Haven Roadmap

Last updated: 2026-05-05

## Current Status

Status: save editor exists; first XML runtime mod generator implemented and locally installed for testing.

Game/runtime:

- Space Haven
- Save files: XML-like `game` files
- Runtime mod path: Space Haven Mod Loader XML mods
- Modloader version observed in logs: 0.12.2

Current local test mod:

- Name: `KK Resource Tuning x2`
- Generated folder: `spacehaven/generated/kk-resource-tuning-x2/`
- Installed local copy: `<SPACEHAVEN_GAME_ROOT>\mods\kk-resource-tuning-x2`
- Generated folders are ignored by git because they are derived from local game assets.

## Implemented Features

### Offline Save Editor

- Windows Python/Tk launcher under `spacehaven/windows/`.
- macOS packaged app and launcher under `spacehaven/mac/`.
- Opens Space Haven save files and edits:
  - Bank values: credits, science points, build points.
  - Crew values: health, hunger, mood, attributes, skills.
  - Stored player-ship resources from generated resource name tables.
- Save writes create timestamped backups next to the edited save file.
- Resource name tables are generated from `spacehaven.jar`.

### XML Resource Tuning Mod Generator

Generator: `tools/build_resource_yield_mod.py`.

Default generated mod features:

- Copies only `Product` definitions with type `Crop` or `Process` from `library/haven`.
- Crop and Process output `howMuch` x2.
- Crop stage `time` divided by 2.
- Crop `<needs>` `consumeEvery` intervals x2.
- Process `<needs>` `consumeEvery` intervals x2.
- Multiplier, crop speed, crop input saver, and process input saver can be tested independently with CLI flags.
- Mod name, folder, and modid vary by multiplier to avoid x2/x5/x10 collisions.

Generated x2 stats from local `spacehaven.jar`:

```text
products=88
outputs=145
crop_times=24
crop_needs=18
process_needs=102
multiplier=x2
```

## Verified

- Generator static checks pass:
  - `node spacehaven/tools/check_resource_tuning_generator.js`
- Local jar structure check passes:
  - `node spacehaven/tools/check_spacehaven_jar.js`
- Python 3.14 successfully generated `kk-resource-tuning-x2` from local `spacehaven.jar`.
- Generated `info.xml`, `README.txt`, and `library/haven` were verified after install.
- Sample generated crop shows expected edits:
  - `time="1300" -> time="650"`
  - output `howMuch="2" -> howMuch="4"`
  - need `consumeEvery="3" -> consumeEvery="6"`

## Needs Smoke Test

- Enable `KK Resource Tuning x2` in Space Haven Mod Loader.
- Launch the game from the modloader.
- Confirm log marker:

```text
Finished loading KK Resource Tuning x2
```

- Verify in-game:
  - Crop output is doubled.
  - Crop growth is faster.
  - Crop water/resource consumption is reduced.
  - Process recipes output doubled or consume inputs less often.
- Record modloader and game versions in a smoke note.

## Known Risks

- XML mods replace definitions by ID; other mods changing the same `Product` IDs may conflict.
- Tuning outputs and input intervals together can strongly affect balance.
- `spacehaven.jar` updates may change `library/haven` schema or product definitions.
- The generated mod is derived from local game assets and should not be committed.
- The macOS `.app` bundle is currently tracked in git; consider moving packaged binaries to GitHub Releases if repository size grows.

## Next Work

1. Smoke test `KK Resource Tuning x2` through the modloader.
2. Add a `docs/smoke-checklist.md` specific to Space Haven.
3. Add focused presets:
   - output-only
   - crops-only
   - process-only
   - gentle balance preset
4. Add sample-save regression checks for save editor parsing and resource edits.
5. Decide whether any future cheat needs AspectJ code injection or can stay XML/save-editor based.

## Project Links

- `README.md` - save editor and mod generator quickstart.
- `MODDING.md` - modloader workflow and XML/code-injection notes.
- `tools/build_resource_yield_mod.py` - XML mod generator.
- `tools/check_resource_tuning_generator.js` - static generator checks.
- `tools/check_spacehaven_jar.js` - local jar structure check.
