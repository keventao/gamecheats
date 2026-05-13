# Timberborn All-in-One Gen Research

Date: 2026-05-12

Update: 2026-05-13

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
- Goods: do not patch `GoodCollection.Common` in v0.1.2. Appending
  faction-specific goods there makes the recoverable-good tooltip describe
  faction needs that are not globally available. Runtime smoke first hit
  `Need with id Grease not found`, then `Need with id Coffee not found`.
- Scope: v0.1.4 ships faction-specific recipe lists. Folktails gets common +
  Folktails goods; IronTeeth gets common + IronTeeth goods. This restores broad
  output coverage without adding foreign goods to `GoodCollection.Common`.
- Tool group: `KKCheats`, displayed as `KK` in the bottom bar. v0.1.3 moved the
  two buildings out of `Wood` after in-game smoke made them hard to find.
- Model reuse: Folktails buildings reference the existing Grill model and
  construction model paths. IronTeeth buildings reference the existing Oil Press
  model and construction model paths as of v0.1.5, because the Folktails Grill
  model can fail during IronTeeth preview creation with
  `Material BaseWood_Brown.Folktails not found in repository`.
  No copied game binaries or meshes are committed.
- Localization: `enUS.csv` and `zhCN.csv`.

## Resource Generator Goods

Folktails `all-in-one-resources` includes:

```text
Badwater, Berries, Carrot, CattailRoot, Chestnut, Dandelion, Dirt, Log,
MapleSyrup, PineResin, Potato, ScrapMetal, Spadderdock, SunflowerSeeds, Water,
Wheat
```

IronTeeth `all-in-one-resources` includes:

```text
Algae, Badwater, Berries, CanolaSeeds, Cassava, CoffeeBean, Corn, Dirt,
Eggplant, Kohlrabi, Log, MangroveFruit, Mushroom, PineResin, ScrapMetal,
Soybean, Water
```

## Product Generator Goods

Folktails `all-in-one-products` includes:

```text
Antidote, Biofuel, Book, BotChassis, BotHead, BotLimb, Bread, Catalyst,
CattailCracker, CattailFlour, Explosives, Extract, Fireworks, Gear,
GrilledChestnut, GrilledPotato, GrilledSpadderdock, MaplePastry, MetalBlock,
Paper, Plank, PunchCard, TreatedPlank, WheatFlour
```

IronTeeth `all-in-one-products` includes:

```text
AlgaeRation, BotChassis, BotHead, BotLimb, CanolaOil, Coffee, CornRation,
EggplantRation, Explosives, Extract, FermentedCassava, FermentedMushroom,
FermentedSoybean, Fireworks, Gear, Grease, MetalBlock, MetalPart, Plank,
TreatedPlank
```

## Verification

Static check:

```bash
bash Timberborn/tools/check_all_in_one_gen.sh
```

Runtime smoke, 2026-05-12: PASS. User confirmed both `KK` buildings appear,
can be built, and produce goods. Cross-faction goods are intentionally hidden.
