# Research Checklist

## Engine Confirmation

- [x] GameAssembly.dll present → **Unity IL2CPP confirmed**
- [ ] Exact Unity version (check `轮回修仙路_Data/StreamingAssets/UnityBuildLog` or exe properties)
- [ ] BepInEx 6 IL2CPP build tested and game launches
- [ ] IL2CPP proxy assemblies generated under `BepInEx/Il2CppAssemblies/`

## Save Format

- [ ] `%AppData%/../LocalLow/烟水寒/轮回修仙路/GameDataSave_Steam/` exists
- [ ] `playerData.txt` — player stats, lifespan, spirit root, realm
- [ ] `packData.txt` — inventory, items, drug resistance
- [ ] Other save files identified
- [ ] Save format is plain JSON (not binary)

## Runtime Targets (dnSpy / ILSpy + Il2CppDumper)

- [ ] Dump `global-metadata.dat` with Il2CppDumper
- [ ] Identify main game manager class (e.g., `GameManager`, `GameCore`, `App`)
- [ ] Identify player controller / stats class
- [ ] Identify time manager / game clock
- [ ] Identify inventory / item system
- [ ] Identify cultivation / realm system
- [ ] Identify combat / damage system

## Harmony Patch Candidates

- [ ] Time scale: `Time.timeScale` direct override (safest, no patch needed)
- [ ] Player stats: identify setter methods for HP, lifespan, spirit root
- [ ] Inventory: identify add/remove item methods
- [ ] Cultivation: identify realm-up / exp-gain methods
- [ ] Combat: identify damage-take methods for god mode

## Risk Notes

- IL2CPP method names may differ from Mono builds; always verify via dump.
- Game uses Chinese class/field names in some cases; keep original names in refs/.
- Save editing is lower-risk than runtime patching for permanent stats.
