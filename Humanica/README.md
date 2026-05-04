# Humanica Cheats

MelonLoader + HarmonyX cheat panel for Humanica.

The panel is opened with F1. UI rendering is fully custom-painted IMGUI because
the generated IL2CPP Unity IMGUI bindings for this game do not reliably return
values from several standard GUI calls.

## Current Status

Tested with:

- Humanica 0.8.18
- Unity 2019.4.41f2
- MelonLoader 0.7.2 Open-Beta
- Game type: IL2CPP x64

Current stable features:

- Time scale: x1, x2, x5, x10
- Resource tab:
  - 5 configurable resource slots
  - resource picker with search
  - +5 and +50 resource buttons
  - resource lock floor
  - warehouse x1, x2, x5, x10 controls
- Village tab:
  - build speed x10
  - production speed x10
  - self-planted crop growth x10
  - own villager movement speed x2 or x5
- Unlock tab:
  - research unlock entry point

## Warehouse Expansion Policy

Warehouse expansion is intentionally conservative.

- No always-on warehouse getter patches are installed.
- Expansion runs only as a one-shot action:
  - when the player clicks `Execute expansion`
  - automatically once after loading a save if the selected multiplier is above x1
- Expansion uses the game's own `Inventory.ResizeInventory(int)` method.
- Warehouse baselines are normalized from the currently loaded warehouse if saved
  baseline data no longer matches the loaded warehouse list.
- Shrinking is blocked. A larger warehouse is kept instead of being reduced to a
  smaller multiplier, because shrinking can lose items or hit invalid pack indexes.
- Resource recovery uses a high-water snapshot.
- The snapshot is saved when:
  - manual expansion runs
  - auto expansion runs
  - the game save flow calls `SaveLoader.StartSave(string)`
  - restored resources raise the high-water mark
- There is no periodic snapshot loop.

Expected stable log markers:

```text
[ResourceCheats] Save snapshot hook OK (Il2CppHumanica.SaveLoading.SaveLoader, methods=1)
[ResourceCheats] warehouse resource snapshot saved (game-save): ...
```

The log should not contain:

```text
warehouse resource snapshot saved (periodic)
```

## Install

1. Install MelonLoader 0.7.x x64 into the game folder so `version.dll` sits next
   to `Humanica.exe`.
2. Launch the game once so MelonLoader generates IL2CPP proxy assemblies under
   `<game>/MelonLoader/Il2CppAssemblies/`.
3. Build the mod:

```bash
rtk dotnet build Humanica/src/HumanicaCheats/HumanicaCheats.sln -c Release
```

4. Copy or let the build target copy `HumanicaCheats.dll` into:

```text
<game>/Mods/
```

5. Restart the game and press F1.

## Development

Default local `GameRoot` is configured in `Humanica/Directory.Build.props`.
Override it when needed:

```bash
rtk dotnet build Humanica/src/HumanicaCheats/HumanicaCheats.sln -c Release /p:GameRoot="<HUMANICA_GAME_ROOT>"
```

Run lightweight policy tests:

```bash
rtk dotnet run --project Humanica/src/HumanicaCheats.Tests/HumanicaCheats.Tests.csproj
```

## Project Layout

- `src/HumanicaCheats/` - mod source
- `src/HumanicaCheats/Core/` - GUI, module registry, shared helpers
- `src/HumanicaCheats/Modules/` - cheat modules and pure policy helpers
- `src/HumanicaCheats.Tests/` - no-game lightweight policy tests
- `refs/` - IL2CPP research notes
- `docs/smoke-checklist.md` - manual in-game verification checklist
- `docs/superpowers/specs/` - design notes
- `docs/superpowers/plans/` - implementation plans
- `ROADMAP.md` - current status and version notes

## IL2CPP IMGUI Notes

Avoid these APIs in this game:

- `GUILayout.*`
- `GUI.Window`
- `GUI.Button` return value
- `GUI.Toggle` return value
- `GUI.TextField` return value

Use positional `GUI.Box` / `GUI.Label`, then handle input with `Event.current`
and `Rect.Contains`. The helper code lives in `Core/Layout.cs`.

Known binding issues:

- `GUILayout.*` can throw `MissingMethodException`.
- `GUI.Window` does not propagate the returned `Rect`.
- `GUI.Button` and `GUI.Toggle` can process clicks but return `false`.
- `GUI.TextField` string return is not trusted; custom text input is used instead.

## Research Notes

- `Il2Cpp.ResourceIndex` is a runtime enum with 143 members.
- Common service locator pattern: `Il2Cpp.S.<ManagerName>`.
- Some game types live under deep namespaces such as
  `Il2CppGameCore.Features.ResourceManagement`.
- When a flat `Il2Cpp.*` proxy is unavailable, use `AccessTools.TypeByName`.

## Safety

- Test experimental warehouse behavior on disposable saves first.
- Do not commit save files, game binaries, generated loader files, logs, or backups.
- Avoid reintroducing per-pack amount scaling; it corrupted saves during research.
