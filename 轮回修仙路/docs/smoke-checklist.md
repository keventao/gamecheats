# Smoke Checklist

## Pre-flight

- [ ] BepInEx 6 IL2CPP installed in game folder
- [ ] Game launches without errors (first launch will generate IL2CPP proxies)
- [ ] Plugin DLL copied to `BepInEx/plugins/LunHuiCheats/`
- [ ] BepInEx/LogOutput.log shows plugin loaded

## Core

- [ ] P (default ToggleKey) opens IMGUI panel
- [ ] Panel shows "轮回修仙路 Cheats" title
- [ ] Category sidebar appears (战斗 / 角色 / 背包 / 修为 / 通用 / 调试)
- [ ] Search box filters modules; sort button cycles Name/Category/Recent
- [ ] "Disable All" button works (restores all locks)
- [ ] Panel draggable
- [ ] A module whose type wasn't found shows "(!)" and a "patches failed" note (no crash)

## Modules

### Time (通用)

- [ ] Time scale buttons (x0.5, x1, x2, x5, x10) apply correctly
- [ ] Game speed visibly changes
- [ ] Disable All resets to x1

### GodMode (战斗)

- [ ] Toggle "锁定满血" on → taking damage, curHp stays = maxHp
- [ ] Status label shows live curHp / maxHp
- [ ] Disable All turns it off

### PlayerStats (角色)

- [ ] Fields pre-fill from live UnitData on entering world
- [ ] Edit 物攻/法攻 + lock → values apply and hold (incl. setting 0)
- [ ] 移速 slider + lock applies; 飞行速度 edit applies
- [ ] "立即写入一次" writes once without lock
- [ ] Disable All clears all locks

### Inventory (背包)

- [ ] Item browser **lists existing items** from All* lists (verifies the IL2CPP Count/Item reflection iteration actually works — if empty, iteration is wrong)
- [ ] Category buttons filter; search filters; Add increases an existing item's count
- [ ] 货币 category uses AddCoin
- [ ] Phase 2: 输入物品ID + "尝试添加" — logs success/failure; if it fails, LogOutput.log explains (no crash)

### Cultivation (修为)

- [ ] Fields pre-fill from live CharacterData on entering world
- [ ] Write button: 道心 should show ✓ and apply (curDaoxin is writable)
- [ ] 经验/等级 expected to show ✗ (currentExp/currentLevel are read-only properties) — if they show ✓ and apply, great; otherwise a real write path (e.g. MySkillLib.AddExp) needs further RE
- [ ] Spirit-root "已找到" note appears when discipleSpiritData present
- [ ] **Back up save before testing exp/level/daoxin edits**

## Safety

- [ ] Save backup created on first plugin load
- [ ] Backup directory contains copies of *.txt save files
- [ ] Old backups pruned to max 5
