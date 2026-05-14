# Factory Town Cheats

BepInEx + Harmony mod for *Factory Town* on macOS Unity Mono.

## Current Status

`v0.1.2` adds an Omni Factory prototype:

- Uses the vanilla `Workshop` as the stable cheat production building.
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

For Steam launch, set Factory Town launch options to:

```text
./run_bepinex.sh %command%
```

BepInEx writes its load log here:

```text
BepInEx/LogOutput.log
```

Do first in-game checks on a disposable save.

## Known Limits

- In-game smoke is still pending.
- The hidden `ItemGenerator` route was removed in `v0.1.2` because smoke testing showed it could make left-clicks require two clicks.
- There is no in-game debug panel in `v0.1.2`; check `BepInEx/LogOutput.log` for load and injection status.
- A fully custom building still needs a safer custom definition, prefab/icon/localization path, and input regression smoke.
