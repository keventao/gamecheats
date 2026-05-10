# Timberborn Double Resources Research

Date: 2026-05-10

## Local Game Evidence

- compatibility layer bottle: `Steam`
- Game root:
  `<USER_HOME>/<APP_SUPPORT>/compatibility layer/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/Timberborn`
- Windows path inside bottle:
  `<TIMBERBORN_GAME_ROOT>`
- Version file:
  `Timberborn_Data/StreamingAssets/Version.txt`
- Observed version: `1.0.13.1-b769e88-sw`
- Blueprint source:
  `Timberborn_Data/StreamingAssets/Modding/Blueprints.zip`

## Target Resources

User requested selected resource outputs raised to x10 after x2 felt too weak.
Tree and natural resource blueprints were later restored to vanilla values after
testing showed tree UI could display modded amounts while Lumberjack Flag
storage still received the vanilla amount.

- Wood: treated as `Good.Log`.
- Natural gathered/tapped goods:
  - `Good.Berries` from Blueberry Bush.
  - `Good.Dandelion` from Dandelion.
  - `Good.CoffeeBean` from Coffee Bush.
  - `Good.Chestnut` from Chestnut Tree.
  - `Good.MangroveFruit` from Mangrove.
  - `Good.PineResin` from Pine.
  - `Good.MapleSyrup` from Maple.
- Science points: `ProducedSciencePoints` from science recipes, multiplied by
  10 per user request.

## Blueprint Changes

Runtime-patched flag-collected yields:

| Blueprint | Base | Mod |
|---|---:|---:|
| Birch Log | 1 | 10 |
| Pine Log | 2 | 20 |
| Oak Log | 8 | 80 |
| Maple Log | 6 | 60 |
| Chestnut Tree Log | 4 | 40 |
| Mangrove Log | 2 | 20 |

Natural gathered/tapped yields:

| Blueprint | Base | Mod |
|---|---:|---:|
| Blueberry Bush Berries | 3 | 30 |
| Dandelion | 1 | 10 |
| Coffee Bush CoffeeBean | 1 | 10 |
| Chestnut Tree Chestnut | 3 | 30 |
| Mangrove MangroveFruit | 4 | 40 |
| Pine PineResin | 2 | 20 |
| Maple MapleSyrup | 3 | 30 |

Implementation notes:

- `YielderRemover.CompleteReservation` is patched with a postfix that records a
  pending boosted delivery for that worker's `GoodCarrier`.
- `CarryRootBehavior.CompleteDelivery` normally gives the original capacity
  reservation amount to the flag inventory. A prefix replaces that amount with
  the pending boosted amount only for marked yield deliveries.
- `GoodCarrier.EmptyHands` clears stale pending delivery markers.
- This avoids multiplying normal warehouse/hauling transfers.
- `NaturalResources/**` overrides are intentionally absent from the mod.
- `LumberjackFlag`, `GathererFlag`, and `TappersShack` output inventories are
  raised to 500 capacity to avoid overfilling with x10 yields.

Science point recipes:

| Blueprint | Base | Mod |
|---|---:|---:|
| `Recipes/Recipe.SciencePoints.blueprint.json` | 1 | 10 |
| `Recipes/Recipe.SciencePointsNumbercruncher.blueprint.json` | 10 | 100 |
| `Recipes/Recipe.SciencePointsObservatory.blueprint.json` | 10 | 100 |

## Local Mod Folder

Repository package:

```text
Timberborn/mods/KKDoubleResources/
```

compatibility layer install target:

```text
<USER_DOCUMENTS>/Timberborn/Mods/KKDoubleResources/
```

Windows view:

```text
C:\users\crossover\Documents\Timberborn\Mods\KKDoubleResources
```

## Verification

Static check:

```bash
bash Timberborn/tools/check_double_resources.sh
```

Manual game smoke is still required after enabling the mod in Timberborn's Mod
Manager.
