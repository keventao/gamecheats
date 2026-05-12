# Timberborn Cheats

Local Timberborn mod workspace.

## Current Status

- Status: local official-mod package created.
- Observed game version: `1.0.13.1-b769e88-sw`
- Runtime: Unity Mono, based on local `Timberborn_Data/Managed` assemblies.
- Loader path: official Timberborn mod manager, Blueprint JSON, and Harmony for
  runtime yield collection patching.
- Current mod: `mods/KKDoubleResources/`
- New mod package: `mods/all-in-one-gen/`

Do not guess game class, method, or field names. Inspect the target build first,
then write research notes under `refs/`.

## KK Double Resources

`KKDoubleResources` multiplies selected output and selected capacities:

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
  - Farmhouse crops:
    - Canola seeds: 3 -> 30
    - Carrot: 3 -> 30
    - Cassava: 1 -> 10
    - Corn: 2 -> 20
    - Eggplant: 3 -> 30
    - Kohlrabi: 2 -> 20
    - Potato: 1 -> 10
    - Soybean: 2 -> 20
    - Sunflower seeds: 2 -> 20
    - Wheat: 3 -> 30
  - Aquatic Farmhouse crops:
    - Cattail root: 3 -> 30
    - Spadderdock: 3 -> 30
- Output inventory capacity:
  - Lumberjack Flag: 20 -> 500
  - Gatherer Flag: 20 -> 500
  - Scavenger Flag: 20 -> 500
  - Tapper's Shack: 50 -> 500
  - Farmhouse: 50 -> 500
  - Efficient Farmhouse: 50 -> 500
  - Aquatic Farmhouse: 50 -> 500
- Storage building capacity:
  - Small Warehouse / Small Tank: 30 -> 150
  - Medium Warehouse: 200 -> 1000
  - Large Warehouse / Large Tank: 1200 -> 6000
  - Small Pile / Small Industrial Pile: 20 -> 100
  - Large Pile / Large Industrial Pile: 180 -> 900
  - Medium Tank: 300 -> 1500
  - Underground Pile: 1000 -> 5000
- Science point recipes:
  - Inventor: 1 -> 10
  - Numbercruncher: 10 -> 100
  - Observatory: 10 -> 100
- Water pump recipes:
  - Water Pump and Deep Water Pump: 1 -> 5 water
  - Large Water Pump: 5 -> 25 water
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
7. Harvest one land crop, such as Carrot, and confirm the Farmhouse receives x10.
8. Harvest one aquatic crop, such as Cattail, and confirm the Aquatic Farmhouse
   receives x10 if available.
9. Build or inspect storage-category buildings and confirm their capacity is x5.
10. Run a Water Pump and confirm each cycle produces 5 water.
11. Run a Large Water Pump if available and confirm each cycle produces 25 water.
12. Confirm adult beavers and bots move at roughly x2 speed without path stutter.
13. Run Inventor or other science building and confirm science points per cycle
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
    all-in-one-gen/
  refs/
    00-research-checklist.md
    01-double-resources-research.md
    02-all-in-one-gen-research.md
  tools/
    check_all_in_one_gen.sh
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

## All-in-One Gen

`all-in-one-gen` is a separate Blueprint-only mod package. It adds two common
buildings to the Wood tool group:

- `all-in-one-resources`: costs 1 Log to build. Every recipe consumes 1 Log and
  produces 10 selected raw resources.
- `all-in-one-products`: costs 1 Log to build. Every recipe consumes 1 Log and
  produces 10 selected processed products.

The buildings are appended to both `Buildings.Folktails` and
`Buildings.IronTeeth`. The mod does not patch `Buildings.Common`, because that
collection contains base templates such as `Path` used by other tool mods.
The mod appends faction-specific goods used by these recipes to
`GoodCollection.Common`, so both factions can resolve every selected output.
`Grease` is intentionally excluded in v0.1.1 because adding it to the common
recoverable-good list crashes the game's effect tooltip path with
`Need with id Grease not found`.

Static package check:

```bash
bash Timberborn/tools/check_all_in_one_gen.sh
```
