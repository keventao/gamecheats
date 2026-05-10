# Space Haven Tools

This folder currently contains an offline save editor for *Space Haven*. Runtime modloader or Steam Workshop mods can be added later under a separate `mods/` area.

## Structure

```text
spacehaven/
  tools/
    build_resource_yield_mod.py
    check_resource_tuning_generator.js
    check_spacehaven_jar.js
  generated/
    kk-resource-tuning-x2/       # generated locally, ignored by git
  mac/
    SpaceHavenEditor.app
    editor.py
    resource_names.json
    run.command
  windows/
    editor.py
    extract_names.py
    resource_names.json
    run.bat
    README.txt
  MODDING.md
  README.md
```

## Save Editor

The editor opens Space Haven save XML files and can edit:

- Bank: credits, science points, and build points.
- Crew: health, hunger, mood, attributes, and skills.
- Resources: stored player-ship items from the generated resource name table.

Every save write creates a timestamped backup next to the original `game` file.

## Windows

Requirements:

- Python 3.10 or newer.
- Tkinter, included with the normal Python installer.

Run:

1. Exit Space Haven before editing a save.
2. Double-click `spacehaven/windows/run.bat`.
3. Pick a save, click `Load`, edit values, then click `Save (with backup)`.

If the editor cannot find saves, set `SPACEHAVEN_SAVES` or browse to the save file manually.

## macOS

Double-click `mac/SpaceHavenEditor.app`, or run `mac/run.command`.

If Gatekeeper blocks the unsigned app, right-click the app and choose `Open`, or remove quarantine:

```bash
xattr -dr com.apple.quarantine "mac/SpaceHavenEditor.app"
```

The editor checks an explicit path argument, `SPACEHAVEN_SAVES`, and the bundled `savegames/` fallback.

## Updating Resource Names

After a game update, regenerate `resource_names.json` if item IDs or names changed.

Example:

```bash
python extract_names.py "<SPACEHAVEN_GAME_ROOT>/spacehaven.jar"
```

## Runtime Mods

See `MODDING.md` for Space Haven Mod Loader and Workshop notes.

Current mod work:

- `tools/build_resource_yield_mod.py` builds a local XML test mod for Crop and Process products.
- Generated mods are written to `spacehaven/generated/` and ignored by git.
- The default generated `KK Resource Tuning x2` mod can be installed under the game's `mods/` folder and enabled from Space Haven Mod Loader.
- Default test features:
  - Crop and Process output `howMuch` x2.
  - Crop stage `time` divided by 2.
  - Crop `<needs>` `consumeEvery` intervals x2, lowering ongoing crop input use.
  - Process `<needs>` `consumeEvery` intervals x2, lowering recipe input use.
- A disabled `Clear QuickLaunch file` button usually means no cache exists; enable the mod and launch normally.

Example build command after Python is installed:

```bash
python spacehaven/tools/build_resource_yield_mod.py --game-root "<SPACEHAVEN_GAME_ROOT>"
```

Optional focused test examples:

```bash
python spacehaven/tools/build_resource_yield_mod.py --multiplier 5
python spacehaven/tools/build_resource_yield_mod.py --no-crop-speed --no-crop-input-saver --no-process-input-saver
```

Local validation without Python:

```bash
node spacehaven/tools/check_resource_tuning_generator.js
node spacehaven/tools/check_spacehaven_jar.js
```

## Safety

- Exit the game before editing saves.
- Keep backups until the edited save has loaded successfully.
- Do not commit save files, backup files, `spacehaven.jar`, extracted game assets, or modloader binaries.
