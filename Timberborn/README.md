# Timberborn Cheats

Local Timberborn mod workspace.

## Current Status

- Status: local official-mod package created.
- Observed game version: `1.0.13.1-b769e88-sw`
- Runtime: Unity Mono, based on local `Timberborn_Data/Managed` assemblies.
- Loader path: official Timberborn mod manager, Blueprint JSON, and Harmony for
  runtime yield collection patching.
- Current mod: `mods/KKDoubleResources/`

Do not guess game class, method, or field names. Inspect the target build first,
then write research notes under `refs/`.

## KK Double Resources

`KKDoubleResources` multiplies selected output by 10:

- Flag-collected natural outputs via runtime patch:
  - Birch: 1 -> 10
  - Pine: 2 -> 20
  - Oak: 8 -> 80
  - Maple: 6 -> 60
  - Chestnut tree: 4 -> 40
  - Mangrove: 2 -> 20
  - Blueberry bush berries: 3 -> 30
  - Dandelion: 1 -> 10
  - Coffee bean: 1 -> 10
  - Chestnut: 3 -> 30
  - Mangrove fruit: 4 -> 40
  - Pine resin: 2 -> 20
  - Maple syrup: 3 -> 30
  - Scrap metal: 15-120 -> 150-1200
- Output inventory capacity:
  - Lumberjack Flag: 20 -> 500
  - Gatherer Flag: 20 -> 500
  - Scavenger Flag: 20 -> 500
  - Tapper's Shack: 50 -> 500
- Science point recipes:
  - Inventor: 1 -> 10
  - Numbercruncher: 10 -> 100
  - Observatory: 10 -> 100
- Character movement speed:
  - Adult beavers and bots: 2.7 -> 5.4
  - Child beavers: 1.35 -> 2.7
  - Slowed movement speeds are also doubled.

Natural resource blueprints are intentionally left at vanilla yield values. The
runtime patch marks selected yield collection in
`YielderRemover.CompleteReservation`, leaves the worker's carried amount at the
vanilla value, then makes `CarryRootBehavior.CompleteDelivery` give the boosted
amount to the destination inventory.

## Install

Build the Harmony DLL locally before installing:

```bash
dotnet build Timberborn/src/KKDoubleResources/KKDoubleResources.csproj -c Release \
  -p:GameRoot="<TIMBERBORN_GAME_ROOT>" \
  -p:HarmonyRoot="<HARMONY_MOD_SCRIPTS_DIR>" \
  -p:ModPackageDir="Timberborn/mods/KKDoubleResources"
```

Install the mod folder under Timberborn's local user mod directory:

```text
<USER_DOCUMENTS>/Timberborn/Mods/KKDoubleResources/
```

Then launch Timberborn, enable both `Harmony` and `KK Double Resources` in the
Mod Manager, and restart the game if prompted.

## Verify

Static package check:

```bash
bash Timberborn/tools/check_double_resources.sh
```

Manual smoke test:

1. Use a disposable save.
2. Enable `KK Double Resources`.
3. Cut one known tree and confirm the flag receives x10 logs.
4. Gather one blueberry bush and confirm berry output is 30.
5. Gather or tap one covered natural resource, such as dandelion, chestnut,
   pine resin, or maple syrup, and confirm the flag/building receives x10 goods.
6. Collect Scrap Metal from ruins and confirm the Scavenger Flag receives x10
   of the selected ruin's base yield.
7. Confirm adult beavers and bots move at roughly x2 speed without path stutter.
8. Run Inventor or other science building and confirm science points per cycle
   are multiplied by 10.

## Project Layout

```text
Timberborn/
  README.md
  ROADMAP.md
  docs/
    smoke-checklist.md
  mods/
    KKDoubleResources/
  refs/
    00-research-checklist.md
    01-double-resources-research.md
  tools/
    check_double_resources.sh
  src/
    KKDoubleResources/
```

Planned areas after runtime research:

- `src/` - mod source if runtime injection is selected.
- `tools/` - install, launch, log, or validation helpers.
- `refs/decompiled/` - local decompile output, ignored by git.

## First Investigation Pass

1. Confirm Timberborn version/build.
2. Confirm runtime from the game directory layout and managed assemblies.
3. Identify supported loader path for this build.
4. Record game APIs relevant to the first cheat target.
5. Choose the smallest safe first feature.

## Safety

- Test only against disposable or backed-up saves.
- Do not commit game binaries, copied assemblies, loader binaries, saves, logs,
  or decompiled output.
- Keep patches narrow and reversible.
