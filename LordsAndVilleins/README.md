# Lords & Villeins Cheats (BepInEx mod)

In-game cheat panel for *Lords & Villeins*. Modular, IMGUI, F1 to toggle.

> Personal use first. Code is structured for future release; UI is currently English only.

## Status

**v0.1.0 — partially in-game verified (2026-04-25)**

- ✅ Loader: F1 panel toggles after entering a save (this game's Player Loop does not schedule BepInEx-owned GameObjects, so the panel attaches to `GameManager.gameObject` once a save is loaded; not available on the main menu)
- ✅ Economy: `+100000 Money` and `+1000 Food` buttons add resources reliably
- ✅ Time: speed override applies in real time
- ⏳ Pawn / Build: in-game smoke pending
- ❌ Economy / Wood / Stone: removed from v0.1 panel — player-personal `Inventory.allowedResources` rejects them. Will need a stockpile-aware path in v0.2.

See `ROADMAP.md` for the full known-limitations list, v0.2 plans, and version history.

## Install

1. Download BepInEx 5.4.21 (x64) from https://github.com/BepInEx/BepInEx/releases and extract into the game folder so `winhttp.dll` sits next to `Lords and Villeins.exe`.
2. Launch the game once (BepInEx will create its folders, then exit).
3. Build this project: `dotnet build -c Release`
4. Run `tools/install.ps1` — copies the plugin DLL to `BepInEx/plugins/LordsAndVilleinsCheats/`.
5. Launch the game. Press **F1** to toggle the cheat panel.

## Layout

- `src/LordsAndVilleinsCheats/` — the mod
- `src/LordsAndVilleinsCheats.Tests/` — xUnit tests for pure logic
- `tools/install.ps1` — copy build output to `<game>/BepInEx/plugins/`
- `tools/tail-log.ps1` — follow `BepInEx/LogOutput.log` in real time
- `tools/run-and-check.ps1` — launch game, wait, parse log, report PASS/FAIL/WARN
- `refs/` — dnSpy reconnaissance notes (game-specific class/field/method names)
- `docs/smoke-checklist.md` — manual smoke list to run before release
- `ROADMAP.md` — current status, known limitations, next-version plans, version history

## Develop

```bash
dotnet build -c Release
powershell tools/install.ps1 -GameRoot "<GAME_ROOT>"
powershell tools/tail-log.ps1 -GameRoot "<GAME_ROOT>"   # in a separate terminal
# then launch the game via Steam
```

After a code change: `dotnet build -c Release && powershell tools/install.ps1 -GameRoot "<GAME_ROOT>"`, then restart game.

For an automated build+install+launch+log-check loop:

```bash
powershell tools/run-and-check.ps1 -GameRoot "<GAME_ROOT>"
```

Exit codes: `0` PASS, `1` FAIL (no plugin load or errors), `2` WARN (broken patches present).

## Tests

```bash
dotnet test
```

## Tested game version

See `refs/06-version-research.md`. Currently: `1.6.15`. Game updates may break Harmony patches — check `BepInEx/LogOutput.log` for "Patch summary".

## Known limitations

- Mods that change save format are out of scope. This mod only writes runtime values.
- Steam achievements are not protected; this mod does not claim "achievement-safe" mode.
- IMGUI styling is intentionally plain (debug-console aesthetic).

## License

Personal use, no warranty.
