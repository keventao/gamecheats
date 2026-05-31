# Smoke Checklist

## Pre-flight

- [ ] BepInEx 6 IL2CPP installed in game folder
- [ ] Game launches without errors (first launch will generate IL2CPP proxies)
- [ ] Plugin DLL copied to `BepInEx/plugins/LunHuiCheats/`
- [ ] BepInEx/LogOutput.log shows plugin loaded

## Core

- [ ] F1 (or configured toggle key) opens IMGUI panel
- [ ] Panel shows "轮回修仙路 Cheats" title
- [ ] Tab bar appears once modules are registered
- [ ] "Disable All" button works
- [ ] Panel draggable

## Modules

### Time

- [ ] Time scale buttons (x0.5, x1, x2, x5, x10) apply correctly
- [ ] Game speed visibly changes
- [ ] Disable All resets to x1

### Player (TODO after reverse-engineering)

- [ ] Lifespan (寿元) display and edit
- [ ] Spirit root (灵根) display and edit
- [ ] Inventory item add/remove
- [ ] God mode toggle

### Cultivation (TODO after reverse-engineering)

- [ ] Realm (境界) display
- [ ] Experience/cultivation speed multiplier

## Safety

- [ ] Save backup created on first plugin load
- [ ] Backup directory contains copies of *.txt save files
- [ ] Old backups pruned to max 5
