# Factory Town Roadmap

Last updated: 2026-05-14

## Status

`v0.1.2` prototype is implemented and build-verified.

- BepInEx plugin skeleton loads through `Plugin.Awake`.
- Harmony patch hooks `Crafting.Init`.
- Vanilla `BuildingType.Workshop` is used as the stable cheat production building.
- Runtime recipes use `1 Wood -> selected output`, one output per recipe.
- Debug IMGUI was removed after smoke found it could steal left-click focus.
- Hidden `ItemGenerator` was removed after A/B smoke showed the plugin caused double-click input behavior and disabling the plugin restored one-click behavior.
- Pure tests cover output filtering and generated recipe id range.

## Verification

- PASS: `rtk dotnet test FactoryTown/src/FactoryTownCheats.Tests/FactoryTownCheats.Tests.csproj --logger "console;verbosity=detailed"`
- PASS: `rtk dotnet build FactoryTown/src/FactoryTownCheats/FactoryTownCheats.csproj -c Release -p:GameRoot="/Users/anonymous/Library/Application Support/Steam/steamapps/common/Factory Town" -p:BepInExPath="/Users/anonymous/Library/Application Support/Steam/steamapps/common/Factory Town/BepInEx"`
- PASS: `v0.1.2` DLL copied to `BepInEx/plugins/FactoryTownCheats/FactoryTownCheats.dll` and matched the local Release build hash.
- PENDING: `v0.1.2` Workshop fallback in-game smoke.

## Next Work

1. Restart Factory Town and confirm the log loads `Factory Town Cheats 0.1.2`.
2. Load a disposable save.
3. Confirm Workshop opens without double-click input regression.
4. Build one Workshop, select one injected recipe, and verify only that output is produced.
5. Decide whether v0.2 should invest in a fully custom building definition, prefab, icon, and localization path.

## Risks

- Build menu visibility may depend on game mode or cached UI initialization.
- Some non-physical outputs may need special handling even if the recipe table accepts them.
- Dynamic recipe ids are stable in the mod range but not recognized by vanilla data files; keep smoke testing on disposable saves.
