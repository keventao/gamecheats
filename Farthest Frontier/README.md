# Farthest Frontier Mods

Local Farthest Frontier Mono mod workspace.

## Current Status

- Build path is supplied through `FARTHEST_FRONTIER_GAME_ROOT` or
  `/p:GameRoot=<FARTHEST_FRONTIER_MONO_ROOT>`.
- Observed game version: `v1.1.1b (Mono)`
- Unity version: `2022.3.62f3`
- Loader: MelonLoader `0.7.0 Open-Beta`
- Runtime: `MonoBleedingEdge`, `x64`, MelonLoader runtime type `net35`
- Current mod: `src/FastVillagers/`

## KK Fast Villagers

`KKFastVillagersPlugin_FF.dll` adapts Krasipeace's FastVillagers idea for the
current Mono build. It installs as a MelonLoader plugin under `Plugins/`
because Steam Workshop sync rebuilds `Mods/`. It patches at runtime by type and
field name instead of directly compiling against `Character` or
`TransportWagon`, so missing fields log a warning instead of hard-failing during
compilation.

Current behavior:

- Villagers:
  - patch `Character.Awake`
  - patch `Character.get_movementSpeed`
  - patch `Character.get_turningSpeed`
  - apply only when the runtime type is `Villager`
  - multiply movement speed by `VillagerMoveSpeedMultiplier`
  - multiply turning speed by `VillagerTurningSpeedMultiplier`
- Transport wagons:
  - patch `TransportWagon.Awake`
  - set `_movementSpeed`
  - set `_turningSpeed`
  - set `carryCapacity`

Default config matches the reference mod's current values:

```text
[KKFastVillagers]
EnableVillagerSpeed = true
VillagerMoveSpeedMultiplier = 3.0
VillagerShoeBonusBase = 1.0
VillagerTurningSpeedMultiplier = 3.0
EnableWagonSpeed = true
WagonMoveSpeed = 8.0
WagonTurningSpeedMultiplier = 50.0
WagonCarryCapacity = 400.0
```

Config is written to:

```text
<Mono game root>/UserData/MelonPreferences.cfg
```

## Build

From WSL:

```bash
export FARTHEST_FRONTIER_GAME_ROOT="<FARTHEST_FRONTIER_MONO_ROOT>"
rtk dotnet build "Farthest Frontier/src/FastVillagers/FastVillagers.csproj" -c Release \
  /p:GameRoot="$FARTHEST_FRONTIER_GAME_ROOT"
```

Or use the project helper:

```bash
rtk bash "Farthest Frontier/tools/install_fast_villagers.sh" "<FARTHEST_FRONTIER_MONO_ROOT>"
```

Steam Workshop sync rebuilds `Mods`, so this project intentionally installs to
`Plugins`.

The build target copies the DLL into:

```text
<Mono game root>/Plugins/KKFastVillagersPlugin_FF.dll
```

If building directly on Windows, pass:

```powershell
dotnet build "Farthest Frontier/src/FastVillagers/FastVillagers.csproj" -c Release `
  /p:GameRoot="<FARTHEST_FRONTIER_MONO_ROOT>"
```

## Verify

Manual smoke test:

1. Use a disposable save.
2. Build and install `KKFastVillagersPlugin_FF.dll`.
3. Launch the Mono executable through MelonLoader.
4. Confirm the MelonLoader log contains:
   - `KK Fast Villagers v0.1.0` under `Loading Plugins`
   - `[KK Fast Villagers] patched Character.Awake`
   - `[KK Fast Villagers] patched Character.get_movementSpeed`
   - `[KK Fast Villagers] patched TransportWagon.Awake`
5. Load a settlement and confirm villagers move faster than vanilla.
6. Build or select a Wagon Shop and confirm transport wagons move faster.
7. Confirm the log has no `[KK Fast Villagers] WARN:` lines.

## Project Layout

```text
Farthest Frontier/
  README.md
  ROADMAP.md
  Directory.Build.props
  docs/
    smoke-checklist.md
  refs/
    01-fast-villagers-research.md
  src/
    FastVillagers/
```

## Safety

- Test only against disposable saves first.
- Do not commit copied game assemblies, loader binaries, logs, saves, or local
  config files.
- Keep patches narrow and reversible.
