# Timberborn Roadmap

Last updated: 2026-05-12

## Status

`v0.1.4` local official-mod package created.

Known facts:

- Target game: Timberborn.
- Observed installed version: `1.0.13.1-b769e88-sw`.
- Runtime: Unity Mono, based on local managed assemblies.
- Mod route: official Timberborn Blueprint JSON plus Harmony runtime patch.
- `KKDoubleResources` multiplies selected flag-collected goods, all farmhouse
  and aquatic farmhouse crop outputs, and science points by 10.
- Water pump recipes produce 5x water.
- Storage-category buildings under `Buildings/Storage` have 5x
  `StockpileSpec.MaxCapacity`.
- Character movement speed is doubled for adult beavers, child beavers, and
  bots via Blueprint JSON.
- Lumberjack Flag, Gatherer Flag, Farmhouse, Efficient Farmhouse, Aquatic
  Farmhouse, Scavenger Flag, and Tapper's Shack output inventories are set to
  500 capacity.
- Package can be copied to the user's Timberborn mod directory for local testing.
- `all-in-one-gen` Blueprint-only package adds two common generator buildings:
  one for raw resources and one for products, both using `Log x1 -> selected
  Good x10` recipes. The buildings are appended to both faction building
  collections and avoid patching `Buildings.Common`. v0.1.2 also avoids
  patching `GoodCollection.Common` after runtime smoke hit `Need with id Grease
  not found` and `Need with id Coffee not found` in the game's recoverable-good
  tooltip path. v0.1.3 moves the two buildings to a dedicated `KK` tool group
  for discoverability. v0.1.4 restores broader output coverage with
  faction-specific recipe lists.

## Acceptance Criteria For First Real Mod

- Timberborn version/build recorded.
- Runtime and loader choice backed by local evidence. Done.
- First target behavior documented in `refs/`. Done.
- Minimal mod package validates and builds without committing game binaries. Done.
- Manual smoke checklist updated with exact in-game verification steps. Done.
- User confirms in-game smoke on disposable save. Pending.

## Candidate First Features

- Add more resource yield targets to the runtime patch if requested.
- Add optional faster growth/production recipes if Blueprint-only changes are
  enough.

## Next Work

1. Enable `Harmony` and `KK Double Resources` in Timberborn's Mod Manager.
2. Run a disposable-save smoke test for x10 logs, natural gathered/tapped goods,
   x10 farm crops, x10 aquatic farm crops, x10 scrap metal, x5 water pumps, x2
   movement speed, x5 storage buildings, and science points.
3. Adjust target resources if user meant a different wood product than `Log`.
4. Smoke-test `all-in-one-gen`: enable the mod, confirm a `KK` bottom-bar tool
   group appears with both buildings, confirm the active faction sees its
   faction-specific goods, build each for 1 Log, confirm startup no longer
   throws the recoverable-good need exception, and run one resource/product
   recipe.

## Risks

- Game updates may rename Blueprint paths or fields.
- Save-affecting features can corrupt active worlds.
- Loader choice should not be assumed before checking the installed build.
