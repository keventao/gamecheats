# For The King Cheats

BepInEx 5 x64 + Harmony mod skeleton for For The King (Unity Mono).

## Requirements

- .NET SDK
- For The King installed at `<FTK_GAME_ROOT>`
- BepInEx 5 x64 installed in the game directory

## Build

From this directory:

```bash
dotnet build src/ForTheKingCheats/ForTheKingCheats.csproj -c Release
```

From the repository root:

```bash
dotnet build "For The King/src/ForTheKingCheats/ForTheKingCheats.csproj" -c Release
```

Override the game path when needed:

```bash
dotnet build "For The King/src/ForTheKingCheats/ForTheKingCheats.csproj" -c Release -p:GameRoot="<FTK_GAME_ROOT>"
```

## Install

Copy the built `ForTheKingCheats.dll` to:

```text
<FTK_GAME_ROOT>\BepInEx\plugins\ForTheKingCheats
```

## Development

This v0.1 project starts with BepInEx 5 x64, Harmony, and IMGUI. The first planned feature is a time scale control.
