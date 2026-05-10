# Timberborn Cheats

Local Timberborn mod workspace.

## Current Status

- Status: local official-mod package created.
- Game root provided by user:

```text
<TIMBERBORN_GAME_ROOT>
```

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
- Output inventory capacity:
  - Lumberjack Flag: 20 -> 500
  - Gatherer Flag: 20 -> 500
  - Tapper's Shack: 50 -> 500
- Science point recipes:
  - Inventor: 1 -> 10
  - Numbercruncher: 10 -> 100
  - Observatory: 10 -> 100

Natural resource blueprints are intentionally left at vanilla yield values. The
runtime patch marks selected yield collection in
`YielderRemover.CompleteReservation`, leaves the worker's carried amount at the
vanilla value, then makes `CarryRootBehavior.CompleteDelivery` give the boosted
amount to the destination inventory.

## Install

compatibility layer maps `C:\users\crossover\Documents` to:

```text
<USER_DOCUMENTS>
```

Install the mod folder here:

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
6. Run Inventor or other science building and confirm science points per cycle
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
      Scripts/
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
