# Factory Town Roadmap

Last updated: 2026-05-14

## Status

`v0.2.0` prototype is implemented and build-verified.

- BepInEx plugin skeleton loads through `Plugin.Awake`.
- Harmony patch hooks `Crafting.Init`.
- Runtime `BuildingType` id `90000` creates a separate `KK 万能工坊` cheat building.
- `KK 万能工坊` reuses the vanilla `Workshop` prefab/icon through Harmony prefix patches.
- Runtime recipes use `1 Wood -> selected output`, one output per recipe.
- Vanilla `Workshop` recipes are no longer modified.
- Debug IMGUI was removed after smoke found it could steal left-click focus.
- Hidden `ItemGenerator` was removed after A/B smoke showed the plugin caused double-click input behavior and disabling the plugin restored one-click behavior.
- Pure tests cover output filtering, generated recipe id range, custom building id, and display name.

## Verification

- PASS: `rtk dotnet test FactoryTown/src/FactoryTownCheats.Tests/FactoryTownCheats.Tests.csproj`
- PASS: `rtk dotnet build FactoryTown/src/FactoryTownCheats/FactoryTownCheats.csproj -c Release -p:GameRoot="<FACTORY_TOWN_GAME_ROOT>"`
- PASS: `v0.2.0` DLL copied to `BepInEx/plugins/FactoryTownCheats/FactoryTownCheats.dll`; installed hash matches Release build hash `60b1bc3c5739883f1a2c1be66c4161dbac1007cb6986512eeb83927405d44dc8`.
- PENDING: `v0.2.0` in-game smoke.

## Next Work

1. Restart Factory Town and confirm the log loads `Factory Town Cheats 0.2.0`.
2. Load a disposable save.
3. Confirm normal left-click actions still trigger once.
4. Confirm build menu shows `KK 万能工坊`.
5. Confirm vanilla `Workshop` recipes are not polluted by omni recipes.
6. Build one `KK 万能工坊`, select one injected recipe, and verify only that output is produced.
7. Save and reload the map without errors.

## Risks

- Build menu visibility may depend on game mode or cached UI initialization.
- Custom building id `90000` serializes as a runtime enum value; use disposable saves until reload smoke passes.
- Some non-physical outputs may need special handling even if the recipe table accepts them.
- Dynamic recipe ids are stable in the mod range but not recognized by vanilla data files; keep smoke testing on disposable saves.
