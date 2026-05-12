#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mod="$root/mods/all-in-one-gen"

resource_goods=(
  Badwater Berries Dirt Log PineResin ScrapMetal Water
)

product_goods=(
  Explosives Extract Fireworks Gear MetalBlock Plank
)

check_json_value() {
  local file="$1"
  local query="$2"
  local expected="$3"
  local actual

  if [[ ! -f "$file" ]]; then
    echo "missing: ${file#$root/}"
    return 1
  fi

  actual="$(jq -r "$query" "$file")"
  if [[ "$actual" != "$expected" ]]; then
    echo "wrong: ${file#$root/} $query expected=$expected actual=$actual"
    return 1
  fi
}

check_recipe() {
  local prefix="$1"
  local good="$2"
  local file="$mod/Recipes/Recipe.$prefix.$good.blueprint.json"

  check_json_value "$file" '.RecipeSpec.Id' "$prefix.$good"
  check_json_value "$file" '.RecipeSpec.Ingredients[0].Id' "Log"
  check_json_value "$file" '.RecipeSpec.Ingredients[0].Amount' "1"
  check_json_value "$file" '.RecipeSpec.Products[0].Id' "$good"
  check_json_value "$file" '.RecipeSpec.Products[0].Amount' "10"
}

for recipe_file in "$mod"/Recipes/*.blueprint.json; do
  recipe_name="${recipe_file##*/}"
  case "$recipe_name" in
    Recipe.AllInOneResources.Badwater.blueprint.json|\
    Recipe.AllInOneResources.Berries.blueprint.json|\
    Recipe.AllInOneResources.Dirt.blueprint.json|\
    Recipe.AllInOneResources.Log.blueprint.json|\
    Recipe.AllInOneResources.PineResin.blueprint.json|\
    Recipe.AllInOneResources.ScrapMetal.blueprint.json|\
    Recipe.AllInOneResources.Water.blueprint.json|\
    Recipe.AllInOneProducts.Explosives.blueprint.json|\
    Recipe.AllInOneProducts.Extract.blueprint.json|\
    Recipe.AllInOneProducts.Fireworks.blueprint.json|\
    Recipe.AllInOneProducts.Gear.blueprint.json|\
    Recipe.AllInOneProducts.MetalBlock.blueprint.json|\
    Recipe.AllInOneProducts.Plank.blueprint.json)
      ;;
    *)
      echo "wrong: unexpected all-in-one recipe ${recipe_file#$root/}"
      exit 1
      ;;
  esac
done

check_json_value "$mod/manifest.json" '.Id' "public.timberborn.all-in-one-gen"
check_json_value "$mod/manifest.json" '.MinimumGameVersion' "1.0.0.0"
check_json_value "$mod/manifest.json" '.RequiredMods | length' "0"

tool_group_file="$mod/BlockObjectToolGroups/BlockObjectToolGroup.KKCheats.blueprint.json"
check_json_value "$tool_group_file" '.BlockObjectToolGroupSpec.Id' "KKCheats"
check_json_value "$tool_group_file" '.BlockObjectToolGroupSpec.NameLocKey' "ToolGroups.KKCheats"

if [[ -e "$mod/GoodCollections/GoodCollection.Common.blueprint.json" ]]; then
  echo "wrong: all-in-one-gen must not append goods to Common; recoverable-good tooltips can crash on faction needs"
  exit 1
fi

if [[ -f "$mod/TemplateCollections/TemplateCollection.Buildings.Common.blueprint.json" ]]; then
  echo "wrong: all-in-one-gen must not patch Buildings.Common; it can hide base Path"
  exit 1
fi

for faction in Folktails IronTeeth; do
  template_file="$mod/TemplateCollections/TemplateCollection.Buildings.$faction.blueprint.json"
  check_json_value "$template_file" '.TemplateCollectionSpec.CollectionId' "Buildings.$faction"
  check_json_value "$template_file" '.TemplateCollectionSpec["Blueprints#append"] | length' "2"
  check_json_value "$template_file" '.TemplateCollectionSpec["Blueprints#append"][0]' "Buildings/AllInOneGen/AllInOneResources/AllInOneResources.blueprint"
  check_json_value "$template_file" '.TemplateCollectionSpec["Blueprints#append"][1]' "Buildings/AllInOneGen/AllInOneProducts/AllInOneProducts.blueprint"
done

resources_building="$mod/Buildings/AllInOneGen/AllInOneResources/AllInOneResources.blueprint.json"
products_building="$mod/Buildings/AllInOneGen/AllInOneProducts/AllInOneProducts.blueprint.json"

check_json_value "$resources_building" '.TemplateSpec.TemplateName' "AllInOneResources"
check_json_value "$resources_building" '.BuildingSpec.BuildingCost[0].Id' "Log"
check_json_value "$resources_building" '.BuildingSpec.BuildingCost[0].Amount' "1"
check_json_value "$resources_building" '.ManufactorySpec.ProductionRecipeIds | length' "${#resource_goods[@]}"
check_json_value "$resources_building" '.LabeledEntitySpec.DisplayNameLocKey' "Building.AllInOneResources.DisplayName"
check_json_value "$resources_building" '.PlaceableBlockObjectSpec.ToolGroupId' "KKCheats"
check_json_value "$resources_building" '.PlaceableBlockObjectSpec.ToolOrder' "10"

check_json_value "$products_building" '.TemplateSpec.TemplateName' "AllInOneProducts"
check_json_value "$products_building" '.BuildingSpec.BuildingCost[0].Id' "Log"
check_json_value "$products_building" '.BuildingSpec.BuildingCost[0].Amount' "1"
check_json_value "$products_building" '.ManufactorySpec.ProductionRecipeIds | length' "${#product_goods[@]}"
check_json_value "$products_building" '.LabeledEntitySpec.DisplayNameLocKey' "Building.AllInOneProducts.DisplayName"
check_json_value "$products_building" '.PlaceableBlockObjectSpec.ToolGroupId' "KKCheats"
check_json_value "$products_building" '.PlaceableBlockObjectSpec.ToolOrder' "20"

for good in "${resource_goods[@]}"; do
  check_recipe "AllInOneResources" "$good"
  if ! jq -e --arg recipe "AllInOneResources.$good" \
    '.ManufactorySpec.ProductionRecipeIds | index($recipe) != null' "$resources_building" >/dev/null; then
    echo "wrong: AllInOneResources missing recipe $good"
    exit 1
  fi

done

for good in "${product_goods[@]}"; do
  check_recipe "AllInOneProducts" "$good"
  if ! jq -e --arg recipe "AllInOneProducts.$good" \
    '.ManufactorySpec.ProductionRecipeIds | index($recipe) != null' "$products_building" >/dev/null; then
    echo "wrong: AllInOneProducts missing recipe $good"
    exit 1
  fi

done

if ! grep -q 'Building.AllInOneResources.DisplayName' "$mod/Localizations/enUS.csv"; then
  echo "wrong: enUS localization missing AllInOneResources"
  exit 1
fi

if ! grep -q 'Building.AllInOneProducts.DisplayName' "$mod/Localizations/zhCN.csv"; then
  echo "wrong: zhCN localization missing AllInOneProducts"
  exit 1
fi

echo "All-in-One Gen checks passed"
