# Factory Town Cheats

BepInEx + Harmony mod for *Factory Town* on macOS Unity Mono.

## Current Status

`v0.1.0` adds an Omni Factory prototype:

- Enables the game's built-in hidden `ItemGenerator` building as a cheat building.
- Adds it to the basic building category.
- Injects selectable recipes at runtime.
- Each recipe consumes `1 Wood` and produces exactly one selected output item.
- Filters out sentinel, filter, utility, and self-loop outputs.

This is intentionally recipe-based. It does not create one recipe that emits every item at once, so the output does not explode.

## Build

Build-only verification can use an extracted BepInEx 5 macOS package outside the game folder:

```bash
rtk dotnet build FactoryTown/src/FactoryTownCheats/FactoryTownCheats.csproj -c Release -p:GameRoot="/Users/anonymous/Library/Application Support/Steam/steamapps/common/Factory Town" -p:BepInExPath="/private/tmp/factorytown-bepinex/BepInEx"
```

Run pure policy tests:

```bash
rtk dotnet test FactoryTown/src/FactoryTownCheats.Tests/FactoryTownCheats.Tests.csproj
```

## Install Notes

Target game root:

```text
/Users/anonymous/Library/Application Support/Steam/steamapps/common/Factory Town
```

Install BepInEx 5 macOS universal into the game root, then place the built plugin DLL under:

```text
BepInEx/plugins/FactoryTownCheats/FactoryTownCheats.dll
```

Do first in-game checks on a disposable save.

## Controls

- `F1`: toggle the small cheat panel.
- `Inject Item Generator Omni Recipes`: retry recipe injection if the automatic `Crafting.Init` hook ran before game data was ready.

## Known Limits

- In-game smoke is still pending.
- The mod uses the hidden `ItemGenerator` instead of creating a brand-new enum, prefab, icon, and localization entry.
- If the hidden building is not visible in the build menu in a particular game mode, the next fallback is to patch an existing Workshop or add deeper build-menu/unlock hooks.
