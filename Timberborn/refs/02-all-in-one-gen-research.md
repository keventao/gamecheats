# Timberborn All-in-One Gen Research

Date: 2026-05-12

## Intent

Create a separate mod named `all-in-one-gen` with two buildings:

- `all-in-one-resources`
- `all-in-one-products`

Both buildings cost 1 `Log` to build. Every production recipe consumes 1 `Log`
and produces 10 of the selected output good.

## Implementation

- Mod route: Blueprint-only package, no Harmony DLL.
- Registration: append both buildings to `TemplateCollection.Buildings.Folktails`
  and `TemplateCollection.Buildings.IronTeeth`. Do not patch
  `TemplateCollection.Buildings.Common`; that collection contains base templates
  such as `Path` used by other tool mods, and patching it caused a crash with
  ConfigurableToolGroups/CustomTools.
- Goods: append all non-common recipe output goods to `GoodCollection.Common`.
  Without this, cross-faction outputs such as `Algae` crash Folktails with
  `GoodSpec with id Algae not found`.
- Excluded goods: `Grease` is not shipped in v0.1.1. Adding it to
  `GoodCollection.Common` makes the recoverable-good tooltip describe the
  `Grease` effect, then `FactionNeedService` throws
  `Need with id Grease not found`.
- Tool group: `Wood`.
- Model reuse: existing Grill model and construction model paths are referenced;
  no copied game binaries or meshes are committed.
- Localization: `enUS.csv` and `zhCN.csv`.

## Resource Generator Goods

`all-in-one-resources` includes raw, gathered, farmed, fluid, and base material
goods:

```text
Algae, Badwater, Berries, CanolaSeeds, Carrot, Cassava, CattailRoot, Chestnut,
CoffeeBean, Corn, Dandelion, Dirt, Eggplant, Kohlrabi, Log, MangroveFruit,
MapleSyrup, Mushroom, PineResin, Potato, ScrapMetal, Soybean, Spadderdock,
SunflowerSeeds, Water, Wheat
```

## Product Generator Goods

`all-in-one-products` includes processed food, materials, bot parts, medicine,
fuel, and other manufactured goods:

```text
AlgaeRation, Antidote, Biofuel, Book, BotChassis, BotHead, BotLimb, Bread,
CanolaOil, Catalyst, CattailCracker, CattailFlour, Coffee, CornRation,
EggplantRation, Explosives, Extract, FermentedCassava, FermentedMushroom,
FermentedSoybean, Fireworks, Gear, GrilledChestnut, GrilledPotato,
GrilledSpadderdock, MaplePastry, MetalBlock, MetalPart, Paper, Plank,
PunchCard, TreatedPlank, WheatFlour
```

## Verification

Static check:

```bash
bash Timberborn/tools/check_all_in_one_gen.sh
```
