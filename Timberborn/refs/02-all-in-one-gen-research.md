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
- Goods: do not patch `GoodCollection.Common` in v0.1.2. Appending
  faction-specific goods there makes the recoverable-good tooltip describe
  faction needs that are not globally available. Runtime smoke first hit
  `Need with id Grease not found`, then `Need with id Coffee not found`.
- Scope: v0.1.2 only ships common-safe outputs already available to both
  factions. Broader output coverage needs separate Folktails and IronTeeth
  generator buildings with faction-specific recipe lists.
- Tool group: `KKCheats`, displayed as `KK` in the bottom bar. v0.1.3 moved the
  two buildings out of `Wood` after in-game smoke made them hard to find.
- Model reuse: existing Grill model and construction model paths are referenced;
  no copied game binaries or meshes are committed.
- Localization: `enUS.csv` and `zhCN.csv`.

## Resource Generator Goods

`all-in-one-resources` includes common-safe raw, fluid, and base material goods:

```text
Badwater, Berries, Dirt, Log, PineResin, ScrapMetal, Water
```

## Product Generator Goods

`all-in-one-products` includes common-safe processed products:

```text
Explosives, Extract, Fireworks, Gear, MetalBlock, Plank
```

## Verification

Static check:

```bash
bash Timberborn/tools/check_all_in_one_gen.sh
```
