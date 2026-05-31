# 轮回修仙路 Cheats Roadmap

Last updated: 2026-05-31

## Current Status

Status: v0.0.1 framework ready.

Game/runtime:

- 轮回修仙路 (Steam AppID 1993150)
- Game path: `<STEAM>\steamapps\common\轮回修仙路`
- Engine: **Unity IL2CPP** (confirmed via `GameAssembly.dll`)
- Mod loader: BepInEx 6 IL2CPP (proven by existing English mod)
- Save format: JSON (`playerData.txt`, `packData.txt`, etc.)

## Completed

- [x] Project scaffold
- [x] BepInEx 6 IL2CPP .csproj with Il2CppInterop references
- [x] Core framework: Plugin, ICheatModule, ModuleRegistry, GuiManager, ModConfig, ModuleStatus, CheatsRunner, GameRefs
- [x] Utility: SaveBackup, HarmonyHelpers
- [x] Time module placeholder (Unity Time.timeScale)
- [x] xUnit test skeleton
- [x] PowerShell install/tail-log tools
- [x] BepInEx 6 IL2CPP installer (`tools/BepInEx-Unity.IL2CPP-win-x64.zip` + `install-bepinex.ps1`)
- [x] BepInEx 6 IL2CPP installed to game directory (via WSL)
- [x] Research checklist scaffold

## Next Work

### Reverse-Engineering (required before any runtime modules)

1. Install BepInEx 6 IL2CPP and verify game launches.
2. Dump `global-metadata.dat` with Il2CppDumper.
3. Identify key game classes:
   - Main game manager
   - Player / character stats
   - Inventory system
   - Cultivation / realm system
   - Time / game clock manager
   - Combat / damage system
4. Document findings in `refs/01-player-research.md`, `refs/02-inventory-research.md`, etc.

### Phase 1 — Runtime Modules

- [ ] Time scale module (placeholder → full implementation)
- [ ] Player stats module (HP, lifespan 寿元, spirit root 灵根, realm 境界)
- [ ] God mode / no damage module
- [ ] Cultivation speed multiplier module
- [ ] Inventory module (runtime add/remove)
- [ ] NPC relationship module

## Known Risks

- IL2CPP method names may differ from decompiled C# source; always verify via dump.
- Game classes may use Chinese names; preserve original names in research notes.
- Runtime injection of cultivation/experience systems may trigger anti-cheat or corrupt saves.
- Unity `Time.timeScale` affects UI animations; may need selective patching instead.

## Version History

### v0.0.1

- BepInEx 6 IL2CPP project scaffold created.
- Core framework modeled after LordsAndVilleins project.
- IL2CPP engine confirmed.

### v0.0.0

- Empty scaffold.
