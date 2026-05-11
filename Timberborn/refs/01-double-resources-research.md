# Timberborn Double Resources Research

Date: 2026-05-10

## Local Game Evidence

- Game root: local Steam install path, not committed.
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
  - `Good.ScrapMetal` from ruins via Scavenger Flag.
  - Planted land crops via Farmhouse or Efficient Farmhouse:
    `Good.CanolaSeeds`, `Good.Carrot`, `Good.Cassava`, `Good.Corn`,
    `Good.Eggplant`, `Good.Kohlrabi`, `Good.Potato`, `Good.Soybean`,
    `Good.SunflowerSeeds`, and `Good.Wheat`.
  - Planted aquatic crops via Aquatic Farmhouse: `Good.CattailRoot` and
    `Good.Spadderdock`.
- Science points: `ProducedSciencePoints` from science recipes, multiplied by
  10 per user request.
- Character movement speed: `WalkerSpeedManagerSpec` on character blueprints,
  doubled after user requested faster Timberborn movement.

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
| Ruin ScrapMetal | 15-120 | 150-1200 |
| Canola CanolaSeeds | 3 | 30 |
| Carrot Carrot | 3 | 30 |
| Cassava Cassava | 1 | 10 |
| Cattail CattailRoot | 3 | 30 |
| Corn Corn | 2 | 20 |
| Eggplant Eggplant | 3 | 30 |
| Kohlrabi Kohlrabi | 2 | 20 |
| Potato Potato | 1 | 10 |
| Soybean Soybean | 2 | 20 |
| Spadderdock Spadderdock | 3 | 30 |
| Sunflower SunflowerSeeds | 2 | 20 |
| Wheat Wheat | 3 | 30 |

Implementation notes:

- `YielderRemover.CompleteReservation` is patched with a postfix that records a
  pending boosted delivery for that worker's `GoodCarrier`.
- `CarryRootBehavior.CompleteDelivery` normally gives the original capacity
  reservation amount to the flag inventory. A prefix replaces that amount with
  the pending boosted amount only for marked yield deliveries.
- `GoodCarrier.EmptyHands` clears stale pending delivery markers.
- This avoids multiplying normal warehouse/hauling transfers.
- `NaturalResources/**` overrides are intentionally absent from the mod.
- `LumberjackFlag`, `GathererFlag`, `FarmHouse`, `EfficientFarmHouse`,
  `AquaticFarmhouse`, `ScavengerFlag`, and `TappersShack` output inventories
  are raised to 500 capacity to avoid overfilling with x10 yields.

Science point recipes:

| Blueprint | Base | Mod |
|---|---:|---:|
| `Recipes/Recipe.SciencePoints.blueprint.json` | 1 | 10 |
| `Recipes/Recipe.SciencePointsNumbercruncher.blueprint.json` | 10 | 100 |
| `Recipes/Recipe.SciencePointsObservatory.blueprint.json` | 10 | 100 |

Movement speed blueprints:

| Blueprint | Base walk | Mod walk | Base slowed | Mod slowed |
|---|---:|---:|---:|---:|
| `Characters/Beaver/BeaverAdult.blueprint.json` | 2.7 | 5.4 | 1.35 | 2.7 |
| `Characters/Beaver/BeaverChild.blueprint.json` | 1.35 | 2.7 | 0.65 | 1.3 |
| `Characters/Bot/Bot.Folktails.blueprint.json` | 2.7 | 5.4 | 1.35 | 2.7 |
| `Characters/Bot/Bot.IronTeeth.blueprint.json` | 2.7 | 5.4 | 1.35 | 2.7 |

## Local Mod Folder

Repository package:

```text
Timberborn/mods/KKDoubleResources/
```

Install target:

```text
<USER_DOCUMENTS>/Timberborn/Mods/KKDoubleResources/
```

Windows view:

```text
<CROSSOVER_DOCUMENTS>\Timberborn\Mods\KKDoubleResources
```

## Verification

Static check:

```bash
bash Timberborn/tools/check_double_resources.sh
```

Manual game smoke is still required after enabling the mod in Timberborn's Mod
Manager.
