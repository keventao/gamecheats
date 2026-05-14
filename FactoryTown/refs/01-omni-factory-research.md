# Omni Factory Research

Observed target:

- Steam game root: `/Users/anonymous/Library/Application Support/Steam/steamapps/common/Factory Town`
- Main assembly: `Factory Town.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll`
- Assembly SHA256: `412a07473f2bcfdd5253bc35bfc888daf524d1c724b2ef5bc6b8dd1560588e57`
- Unity player: `2022.3.62f2`

Relevant game data:

- `BuildingType.ItemGenerator` exists in the vanilla enum.
- `BuildingType.Workshop` exists and is a safe fallback if the hidden generator does not show in the build UI.
- `Data.defaultBuildingDefs` maps `BuildingType` to `BuildingDef`.
- `Data.defaultBuildingRecipes` and `Crafting.cachedBuildingRecipes` map buildings to recipe lists.
- `Data.defaultDisplayCategories` maps build categories to build-menu entity ids.
- `Crafting.recipeCache` stores runtime recipe definitions.
- `Crafting.physicalItemTypes`, `currencies`, `researchItemTypes`, and `workerItemTypes` expose curated item groups.

Chosen v0.1.2 route:

- Use vanilla `Workshop` for stable production and input behavior.
- Add generated recipe ids in the high range `50000 + (int)ItemType`.
- Use `Recipe.LoadBasic(ItemType.Wood, 1, output)` so the recipe picker selects exactly one output.
- Exclude `None`, `Invalid`, `Filter*`, `Utility*`, and `Wood`.

Rejected route:

- `BuildingType.ItemGenerator` was build-visible and recipe injection worked, but A/B smoke showed the plugin route caused left-clicks to require two clicks. Disabling the plugin restored one-click behavior. Treat hidden generator as unsafe until deeper build-menu/input research is done.

Deferred deeper route:

- Fully custom building means new type, build menu entry, localization, icon, prefab/render path, unlock behavior, save compatibility, and possibly UI cache patches.
