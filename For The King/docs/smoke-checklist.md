# For The King Cheats Smoke Checklist

## Build And Install

- `dotnet build "src/ForTheKingCheats/ForTheKingCheats.csproj" -c Release` succeeds.
- `winhttp.dll` exists next to `FTK.exe`, and `BepInEx/core/BepInEx.dll` exists.
- `tools/install.ps1` copies `ForTheKingCheats.dll` to `BepInEx/plugins/ForTheKingCheats/`.
- Copying the DLL is not enough if either BepInEx loader file is absent; the plugin will not load until BepInEx 5 x64 is installed and the game is launched once.

## In Game

- BepInEx log contains `For The King Cheats ready.`
- F1 opens and closes the cheat panel.
- The panel can be dragged.
- Time x2 visibly speeds up animations or world movement.
- Time x5 visibly speeds up gameplay without immediate log errors.
- Time x10 visibly speeds up gameplay without immediate log errors.
- Reset returns `Time.timeScale` to x1.

## Notes

- Test against disposable saves.
- Gold and Lore are not active v0.1 controls until their runtime storage is confirmed.
