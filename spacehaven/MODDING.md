# Space Haven Modding Notes

## Current Direction

`spacehaven/` currently contains an offline save editor. Runtime mods should be added as a separate modloader-oriented area inside this project, not mixed into the save editor scripts.

Recommended layout for future runtime mods:

```text
spacehaven/
  tools/
    build_resource_yield_mod.py
  generated/
    kk-resource-yield-x2/
  mods/
    kk-cheats/
      info.xml
      library/
        haven.xml
        animations.xml
        textures.xml
      textures/
```

Use XML mods first when the feature can be expressed by changing library definitions. Use code injection only when runtime behavior must change.

## Resource Yield Mod

The first minimal resource cheat is generated locally instead of committed as copied game XML:

```bash
rtk python spacehaven/tools/build_resource_yield_mod.py
```

If `python` is not on PATH, use Codex's bundled Python or any Python 3.10+ install.

Default behavior:

- Reads `spacehaven.jar` from `SPACEHAVEN_GAME_ROOT` or common Steam install paths.
- Writes `spacehaven/generated/kk-resource-yield-x2/`.
- Copies only `Product` definitions with type `Crop` or `Process`.
- Multiplies output `howMuch` values under `<products>` by `x2`.

Copy the generated folder into the game's `mods/` folder, clear QuickLaunch in the modloader, then launch from the modloader.

Example local install path:

```text
<SPACEHAVEN_GAME_ROOT>\mods\kk-resource-yield-x2
```

The modloader log should show:

```text
Finished loading KK Resource Yield x2
```

If `Clear QuickLaunch file` is disabled, there is no QuickLaunch cache to delete. This is OK. Enable the mod in the modloader list and launch from the modloader.

Useful log paths:

```text
<SPACEHAVEN_GAME_ROOT>\mods\logs.txt
<STEAM_WORKSHOP_ROOT>\content\979110\3703674043\logs.txt
```

## Modloader Workflow

The Space Haven Mod Loader supports two mod types:

- XML mods: merge library definitions by ID.
- Code injection mods: use AspectJ-style Java weaving to alter runtime code.

Basic flow:

1. Install or launch the latest Space Haven Mod Loader.
2. Use `Open Mods Folder` to find the game `mods/` directory.
3. Put each mod in its own folder with an `info.xml`.
4. For development, click `Clear QuickLaunch file` before launching after edits.
5. Launch Space Haven from the modloader.

Steam Workshop is also available on the current modding path. Workshop packaging uses `workshop.jar` from the Space Haven install folder.

## XML Mod Notes

After extracting game assets, useful files are:

- `library/haven.xml` or annotated `library/haven_annotated.xml`: most gameplay definitions.
- `library/texts.xml`: localized text IDs.
- `library/animations.xml`: animation references.
- `library/textures.xml`: texture and region definitions.

Existing definitions are replaced by copying the same numeric ID into the mod. New definitions need unique positive 32-bit IDs.

## Code Injection Notes

Code injection mods require Java knowledge and decompiling `spacehaven.jar`. The game code is reported as not obfuscated, so research should start from decompiled Java symbols and then use AspectJ-style hooks.

For cheat features, prefer this order:

1. XML definition edit.
2. Save editor change.
3. Code injection mod.

## Safety Rules

- Do not commit `spacehaven.jar`, extracted game assets, saves, backup files, modloader binaries, or Workshop upload output.
- Test against disposable saves.
- Keep save-editor changes and runtime-mod changes separated.
- Document game version and modloader version in smoke notes.

## Sources

- Space Haven Mod Loader: https://github.com/Spacehaven-modding-tools/spacehaven-modloader#getting-started
- Space Haven Workshop/modding announcement: https://steamdb.info/patchnotes/23028790/
