# Humanica Cheats Smoke Checklist

Run on a disposable save first. Use a backed-up real save only after the
disposable save passes.

## Startup

- [ ] MelonLoader loads `HumanicaCheats v0.1.1`.
- [ ] The installed DLL hash matches the expected build.
- [ ] No mod startup exception appears.
- [ ] Log contains `WarehouseCapacityPatch] Disabled`.
- [ ] Log contains `Save snapshot hook OK`.
- [ ] Log does not contain `warehouse resource snapshot saved (periodic)`.

## Panel

- [ ] Press F1 to open the panel.
- [ ] Press F1 again to close it.
- [ ] The panel can be dragged.
- [ ] Tabs are visible: Time, Resource, Village, Unlock.
- [ ] Buttons and toggles react to mouse clicks.

## Time

- [ ] x1 restores normal speed.
- [ ] x2 increases game speed.
- [ ] x5 increases game speed more than x2.
- [ ] x10 increases game speed more than x5.
- [ ] The displayed actual `Time.timeScale` matches the selected target.

## Resource Add Buttons

- [ ] Resource tab shows 5 configurable slots.
- [ ] +5 adds the selected resource.
- [ ] +50 adds the selected resource.
- [ ] Repeated +50 clicks do not freeze the game.
- [ ] Resource slot selection persists after restart.

## Resource Picker

- [ ] Clicking a resource slot opens the picker.
- [ ] English search filters resource enum names.
- [ ] Chinese search filters translated resource names when available.
- [ ] Backspace edits the search text.
- [ ] Escape clears/unfocuses search.
- [ ] Mouse wheel scrolls the result list.
- [ ] Clicking an item selects it and closes the picker.

## Warehouse Expansion

- [ ] Select x5 in the Resource tab.
- [ ] Click `Execute expansion`.
- [ ] A save backup is created under `UserData/HumanicaCheats/SaveBackups`.
- [ ] Log shows `manual warehouse expansion x5`.
- [ ] Log shows consistent baselines, for example `baseline=16,16,16,16,16,16`.
- [ ] Clicking the same multiplier again does not stack capacity.
- [ ] Switching to a lower multiplier does not shrink existing expanded warehouses.
- [ ] Save the game normally.
- [ ] Log shows `warehouse resource snapshot saved (game-save)` or
  `warehouse resource snapshot unchanged (game-save)`.
- [ ] Exit the game.
- [ ] Restart and reload the same save.
- [ ] Warehouses auto-expand back to the selected multiplier.
- [ ] Resources that existed at normal save time are restored.
- [ ] No `EndOfStreamException` appears.
- [ ] No repeated periodic snapshot lines appear.

## Village Cheats

- [ ] Build speed x10 visibly accelerates construction progress.
- [ ] Production speed x10 visibly accelerates workshop output.
- [ ] Self-planted crop growth x10 affects planted crops.
- [ ] Wild plants do not receive the crop multiplier.
- [ ] Non-crop resource deposits do not receive the crop multiplier.
- [ ] Own villager movement x2 visibly increases movement speed.
- [ ] Own villager movement x5 is faster than x2.
- [ ] Clicking the active movement multiplier again restores normal movement.
- [ ] Animals and non-villager creatures are not intentionally accelerated.

## Stability

- [ ] Play several minutes after warehouse expansion.
- [ ] Start or finish combat after warehouse expansion.
- [ ] No `coreclr.dll 0xc0000005` crash occurs.
- [ ] No long UI freeze or Windows "not responding" state occurs.

## Unlock

- [ ] Unlock tab opens.
- [ ] Research unlock action can be clicked.
- [ ] If unlock behavior fails, capture the MelonLoader log and update refs.

## Result

Game version:

MelonLoader version:

DLL hash:

Save tested:

Result:
