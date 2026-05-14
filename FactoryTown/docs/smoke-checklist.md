# Factory Town Smoke Checklist

Use a disposable save.

## Load

- [ ] BepInEx starts without preloader errors.
- [ ] `Factory Town Cheats 0.1.2` appears in the BepInEx log.
- [ ] Log includes `Waiting for Crafting.Init`.
- [ ] After save load, log includes `injected ... Workshop recipes`.

## UI

- [ ] No always-on debug panel appears.
- [ ] Normal left-click actions do not require double-clicking.
- [ ] Workshop opens normally and shows injected recipes.

## Gameplay

- [ ] Place one Workshop.
- [ ] Recipe picker shows many injected outputs.
- [ ] Select one normal item output.
- [ ] Provide Wood.
- [ ] Building consumes Wood and produces only the selected output.
- [ ] Switch to a different output recipe.
- [ ] Building produces the new selected output only.
- [ ] Save and reload the map without errors.

## Failure Signals

- [ ] BepInEx log has no Harmony patch errors.
- [ ] No missing recipe or missing item exceptions.
- [ ] No save/load exception after a dynamic recipe was selected.
