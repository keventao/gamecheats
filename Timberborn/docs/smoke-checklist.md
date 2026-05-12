# Timberborn Smoke Checklist

Use a disposable world or backed-up save for every test.

## Preflight

- Record Timberborn version/build.
- Record mod route: official Blueprint JSON mod plus Harmony runtime patch.
- Confirm no copied game assemblies, saves, logs, or loader binaries are staged.
- Back up the test save or create a throwaway world.

## Loader Smoke

- Game launches from Steam.
- `Harmony` appears in the Timberborn Mod Manager.
- `KK Double Resources` appears in the Timberborn Mod Manager.
- Mod Manager enables both mods without errors.
- Game reaches main menu.
- Test world loads successfully.
- Player.log contains `[KKDoubleResources] StartMod`.

## Feature Smoke

- Confirm Lumberjack Flag output inventory capacity is 500.
- Confirm Gatherer Flag output inventory capacity is 500.
- Confirm Farmhouse output inventory capacity is 500.
- Confirm Efficient Farmhouse output inventory capacity is 500.
- Confirm Aquatic Farmhouse output inventory capacity is 500.
- Confirm Scavenger Flag output inventory capacity is 500.
- Confirm Tapper's Shack output inventory capacity is 500.
- Confirm storage-category buildings use x5 capacity, for example Small
  Warehouse 150, Medium Warehouse 1000, Large Warehouse 6000, Small Tank 150,
  Medium Tank 1500, Large Tank 6000, Small Pile 100, Large Pile 900, and
  Underground Pile 5000.
- Cut a Birch and confirm the Lumberjack Flag receives 10 logs.
- Cut a Pine and confirm the Lumberjack Flag receives 20 logs.
- Confirm the worker still carries the vanilla amount before delivery.
- Player.log contains one `[KKDoubleResources] Marked boosted delivery ...` line
  after the first boosted collection.
- Player.log contains one `[KKDoubleResources] Boosted flag delivery ...` line
  after the first boosted delivery.
- Cut an Oak and confirm the Lumberjack Flag receives 80 logs.
- Gather a Blueberry Bush and confirm the Gatherer Flag receives 30 berries.
- Gather Dandelion and confirm the Gatherer Flag receives 10.
- Gather Coffee Bean and confirm the Gatherer Flag receives 10.
- Gather Chestnut and confirm the Gatherer Flag receives 30.
- Gather Mangrove Fruit and confirm the Gatherer Flag receives 40.
- Tap Pine Resin and confirm the Tapper's Shack receives 20.
- Tap Maple Syrup and confirm the Tapper's Shack receives 30.
- Harvest Carrot or another land crop and confirm the Farmhouse receives x10.
- Harvest Cattail or another aquatic crop and confirm the Aquatic Farmhouse
  receives x10.
- Run a Water Pump or Deep Water Pump and confirm each cycle produces 5 water.
- Run a Large Water Pump and confirm each cycle produces 25 water.
- Collect Scrap Metal from ruins and confirm the Scavenger Flag receives x10
  of the selected ruin's base yield.
- Confirm adult beavers move at roughly x2 speed without path stutter.
- Confirm bots move at roughly x2 speed if available.
- Confirm child beavers move faster if available.
- Run Inventor and confirm science output is 10 per cycle.
- Run Numbercruncher or Observatory if available and confirm science output is
  100 per cycle.
- Save/load still works after using the mod.
- Disable/uninstall path restores normal behavior.

## All-in-One Gen Smoke

- Enable `all-in-one-gen` in the Mod Manager.
- Confirm startup/load does not throw `Need with id ... not found`.
- Confirm a `KK` tool group appears in the bottom build bar.
- Confirm `all-in-one-resources` appears in the `KK` tool group.
- Confirm `all-in-one-products` appears in the `KK` tool group.
- Build each generator and confirm each costs 1 Log.
- Run one resource recipe, such as Log or Water, and confirm it consumes 1 Log
  and produces 10 selected goods.
- Run one product recipe, such as Plank or Gear, and confirm it consumes 1 Log
  and produces 10 selected goods.

## Exit Criteria

- Relevant log markers copied into the feature notes.
- Any crash, exception, or save issue is recorded before further changes.
