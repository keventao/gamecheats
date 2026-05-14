# Factory Town Roadmap

Last updated: 2026-05-14

## Status

`v0.1.1` prototype is implemented and build-verified.

- BepInEx plugin skeleton loads through `Plugin.Awake`.
- Harmony patch hooks `Crafting.Init`.
- Hidden `BuildingType.ItemGenerator` is enabled as the cheat building.
- Build menu injection targets `BuildCategoryType.BuildingBasic`.
- Runtime recipes use `1 Wood -> selected output`, one output per recipe.
- Debug IMGUI was removed after smoke found it could steal left-click focus.
- Pure tests cover output filtering and generated recipe id range.

## Verification

- PASS: `rtk dotnet test FactoryTown/src/FactoryTownCheats.Tests/FactoryTownCheats.Tests.csproj --logger "console;verbosity=detailed"`
- PASS: `rtk dotnet build FactoryTown/src/FactoryTownCheats/FactoryTownCheats.csproj -c Release -p:GameRoot="/Users/anonymous/Library/Application Support/Steam/steamapps/common/Factory Town" -p:BepInExPath="/private/tmp/factorytown-bepinex/BepInEx"`
- PASS: local BepInEx launch generated `BepInEx/LogOutput.log`, loaded `Factory Town Cheats 0.1.0`, and injected 330 `ItemGenerator` recipes.
- PENDING: in-game BepInEx load and disposable-save smoke.

## Next Work

1. Install BepInEx into the Factory Town game root.
2. Load a disposable save and confirm plugin log lines.
3. Confirm `ItemGenerator` appears in the build menu.
4. Build one `ItemGenerator`, select one injected recipe, and verify only that output is produced.
5. Decide whether v0.2 should keep the hidden `ItemGenerator` route or invest in a fully custom building definition, prefab, icon, and localization path.

## Risks

- Build menu visibility may depend on game mode or cached UI initialization.
- Some non-physical outputs may need special handling even if the recipe table accepts them.
- Dynamic recipe ids are stable in the mod range but not recognized by vanilla data files; keep smoke testing on disposable saves.
