# Humanica Cheats (MelonLoader mod)

In-game cheat panel for *Humanica*. Modular, fully self-painted IMGUI, F1 to toggle.

> Personal use first. Code is structured for future release; UI is currently Chinese only.

## Status

**v0.1.1 — partially in-game verified (2026-05-02)**

- ✅ Loader: MelonLoader 0.7.2 + HarmonyX, dll deploys to `<game>/Mods/`
- ✅ GUI: F1 toggle, draggable window, 4 tabs — all custom-painted (see "IL2CPP IMGUI gotchas" below)
- ✅ Time: `Time.timeScale` ×1/×2/×5/×10 + reset, panel shows target vs actual
- ✅ Resource: 5 user-configurable slots, picker with **Chinese + English search**,
  scroll wheel list (~120 resources), selection persisted via MelonPreferences,
  +5 / +50 amounts, lock-floor toggle
- ⏳ Village: SpawnRandomVillager + build/production speed ×10 — patch direction needs in-game verification
- ⏳ Unlock: InstantResearchAll for tech tree

See `ROADMAP.md` for the full version history, known limitations, and v0.2 plans.

## Install

1. Download MelonLoader 0.7.x x64 from https://github.com/LavaGang/MelonLoader/releases (zip variant) and extract into the game folder so `version.dll` sits next to `Humanica.exe`.
2. Launch the game once. MelonLoader will generate Il2CppInterop proxy assemblies into `<game>/MelonLoader/Il2CppAssemblies/` (first launch can take 3–10 minutes; black screen is normal).
3. Build this project: `dotnet build -c Release` (from `src/HumanicaCheats/`).
4. Or run `tools/install.ps1` — builds and verifies dll deploy.
5. Launch the game. Press **F1** to toggle the cheat panel.

## Layout

- `src/HumanicaCheats/` — the mod
  - `Core/` — shared services (ICheatModule, ModuleRegistry, GuiManager, GameRefs, **Layout**, **ResourceI18n**)
  - `Modules/` — feature modules (TimeCheats, ResourceCheats, VillageCheats, UnlockCheats)
  - `Plugin.cs` — MelonMod entry point
- `tools/install.ps1` — build + verify dll deploy to `<game>/Mods/`
- `refs/` — IL2CPP proxy reconnaissance notes (game-specific class/field/method names)
- `docs/smoke-checklist.md` — manual smoke list to run before release
- `docs/superpowers/specs/` — design spec
- `docs/superpowers/plans/` — implementation plan
- `ROADMAP.md` — current status, known limitations, next-version plans, version history

## Develop

```bash
dotnet build -c Release
powershell tools/install.ps1
```

`Directory.Build.props` defines the default `GameRoot=E:\Games\Humanica`. Override per-build:

```bash
dotnet build -c Release /p:GameRoot="<HUMANICA_GAME_ROOT>"
```

After a code change: `tools/install.ps1`, then restart game (MelonLoader does not hot-reload).

## IL2CPP IMGUI gotchas (this mod's hard-won lessons)

MelonLoader 0.7.2 + Il2CppInterop's generated UnityEngine.IMGUIModule stub for this Unity
build has multiple bugs. **Do not** use any of these — they fail silently or throw:

1. **`GUILayout.*` — throws `MissingMethodException`** on `BeginHorizontal`/`Window` etc.
   Internal `ExitGUIException..ctor(string)` and `LayoutedWindow..ctor(...)` ctors don't
   match the binding. Use **only `GUI.*` positional API** (no auto-layout).
2. **`GUI.Window` — `Rect` return value does not propagate.** Drag is captured internally
   (hot control flips) but `_windowRect = GUI.Window(...)` always assigns the input rect.
   Workaround: don't use `GUI.Window`; draw window background with `GUI.Box` and implement
   drag yourself with `Event.current.MouseDown / MouseDrag / MouseUp`.
3. **`GUI.Button` / `GUI.Toggle` — `bool` return value does not propagate.** IMGUI processes
   the click correctly (event becomes `Used`, hot resets) but the bool comes back `false`.
   Workaround: draw with `GUI.Box`, hit-test manually:
   ```csharp
   GUI.Box(rect, label);
   if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
   { Event.current.Use(); /* fire action */ }
   ```
4. **`GUI.TextField` — by extension `string` return assumed broken.** Implement custom text
   input by tracking string state and listening to `Event.current.character` on `KeyDown`.
   IME (Chinese pinyin) delivers the final character via `character` after composition,
   so this approach handles both Chinese and English input.

The `Core/Layout.cs` helper wraps these workarounds (`ImguiUtil.Button`, `ImguiUtil.Toggle`,
plus a vertical-cursor `Layout` for stacked rows). New modules should use it instead of
calling `GUI.*` directly.

## Other IL2CPP-specific notes

- All game types are wrapped under `Il2Cpp` namespace by Il2CppInterop. The service locator pattern is `Il2Cpp.S.<ManagerName>` (e.g. `Il2Cpp.S.VillageData`, `Il2Cpp.S.TechManager`).
- `Il2Cpp.ResourceIndex` is a runtime enum with 143 members. Cast int → enum directly:
  `(Il2Cpp.ResourceIndex)42`. The startup dump in `Plugin.OnInitializeMelon` writes
  the full member list to `MelonLoader/Latest.log` under `[ResourceIndex.dump]`.
- `GUI.Window` callback requires `DelegateSupport.ConvertDelegate<GUI.WindowFunction>(...)`
  to wrap managed delegates — but per (2) above, you shouldn't be using `GUI.Window` anyway.
- Some game types live in deep namespaces (e.g. `Il2CppGameCore.Features.Buffs.BuffController`); when the flat `Il2Cpp.*` proxy isn't available, fall back to `AccessTools.TypeByName(全限定名)`.

## Tested game version

See `ROADMAP.md`. Game updates may break Harmony patches and Il2CppInterop class paths — re-run MelonLoader's first-launch generation and check `MelonLoader/Latest.log`.

## Known limitations

- Village `×10` build / production patches not yet in-game verified (direction may need flip).
- `ResourceI18n` covers ~100 of 143 enum entries; the rest fall back to enum name.
- No save backup yet (LordsAndVilleins has one; Humanica v0.2 may add).

## License

Personal use, no warranty.
