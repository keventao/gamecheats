# Humanica Cheats (MelonLoader mod)

In-game cheat panel for *Humanica*. Modular, IMGUI, F1 to toggle.

> Personal use first. Code is structured for future release; UI is currently Chinese only.

## Status

**v0.1.0 — code complete, awaiting in-game smoke (2026-05-01)**

- ✅ Loader: MelonLoader 0.6.x + HarmonyX, dll deploys to `<game>/Mods/`
- ✅ Time: `Time.timeScale` ×1/×2/×5/×10 + reset, panel shows target vs actual
- ⏳ Resource: 4 resources (LOG / STONE_BRICKS / BREAD / TECHNOLOGY_KNOWLEDGE) — values are placeholders, in-game verification needed
- ⏳ Village: SpawnRandomVillager + build/production speed ×10 — patch direction needs in-game verification
- ⏳ Unlock: InstantResearchAll for tech tree

See `ROADMAP.md` for the full known-limitations list, v0.2 plans, and version history.

## Install

1. Download MelonLoader 0.6.x x64 from https://github.com/LavaGang/MelonLoader/releases (zip variant) and extract into the game folder so `version.dll` sits next to `Humanica.exe`.
2. Launch the game once. MelonLoader will generate Il2CppInterop proxy assemblies into `<game>/MelonLoader/Il2CppAssemblies/` (first launch can take 3–10 minutes; black screen is normal).
3. Build this project: `dotnet build -c Release` (from `src/HumanicaCheats/`).
4. Or run `tools/install.ps1` — builds and verifies dll deploy.
5. Launch the game. Press **F1** to toggle the cheat panel.

## Layout

- `src/HumanicaCheats/` — the mod
  - `Core/` — shared services (ICheatModule, ModuleRegistry, GuiManager, GameRefs)
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

## IL2CPP-specific notes

- All game types are wrapped under `Il2Cpp` namespace by Il2CppInterop. The service locator pattern is `Il2Cpp.S.<ManagerName>` (e.g. `Il2Cpp.S.VillageData`, `Il2Cpp.S.TechManager`).
- Resource indices are static fields on `Il2Cpp.ResourceIndex` (e.g. `LOG`, `STONE_BRICKS`, `BREAD`).
- IMGUI `using` blocks (`new GUILayout.HorizontalScope()`) do **not** work — use `BeginHorizontal/EndHorizontal` explicitly. IL2CPP Unity types do not implement standard `IDisposable`.
- `GUI.Window` callback requires `DelegateSupport.ConvertDelegate<GUI.WindowFunction>(...)` to wrap managed delegates.
- Some game types live in deep namespaces (e.g. `Il2CppGameCore.Features.Buffs.BuffController`); when the flat `Il2Cpp.*` proxy isn't available, fall back to `AccessTools.TypeByName(全限定名)`.

## Tested game version

See `ROADMAP.md`. Game updates may break Harmony patches and Il2CppInterop class paths — re-run MelonLoader's first-launch generation and check `MelonLoader/Latest.log`.

## Known limitations

- ResourceIndex specific values (which int = wood, stone, etc.) are placeholders pending in-game verification.
- Build speed `×10` postfix assumes the patched method returns "progress per step"; if it returns "duration", the direction must flip.
- No save backup yet (LordsAndVilleins has one; Humanica v0.2 may add).

## License

Personal use, no warranty.
