#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mod="$root/mods/all-in-one-gen"

common_resource_goods=(
  Badwater Berries Dirt Log PineResin ScrapMetal Water
)

folktails_resource_goods=(
  Carrot CattailRoot Chestnut Dandelion MapleSyrup Potato Spadderdock
  SunflowerSeeds Wheat
)

ironteeth_resource_goods=(
  Algae CanolaSeeds Cassava CoffeeBean Corn Eggplant Kohlrabi MangroveFruit
  Mushroom Soybean
)

common_product_goods=(
  Explosives Extract Fireworks Gear MetalBlock Plank
)

folktails_product_goods=(
  Antidote Biofuel Book BotChassis BotHead BotLimb Bread Catalyst
  CattailCracker CattailFlour GrilledChestnut GrilledPotato
  GrilledSpadderdock MaplePastry Paper PunchCard TreatedPlank WheatFlour
)

ironteeth_product_goods=(
  AlgaeRation BotChassis BotHead BotLimb CanolaOil Coffee CornRation
  EggplantRation FermentedCassava FermentedMushroom FermentedSoybean Grease
  MetalPart TreatedPlank
)

folktails_resources=("${common_resource_goods[@]}" "${folktails_resource_goods[@]}")
ironteeth_resources=("${common_resource_goods[@]}" "${ironteeth_resource_goods[@]}")
folktails_products=("${common_product_goods[@]}" "${folktails_product_goods[@]}")
ironteeth_products=("${common_product_goods[@]}" "${ironteeth_product_goods[@]}")

resource_recipe_goods=("${common_resource_goods[@]}" "${folktails_resource_goods[@]}" "${ironteeth_resource_goods[@]}")
product_recipe_goods=("${common_product_goods[@]}" "${folktails_product_goods[@]}" "${ironteeth_product_goods[@]}")

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
  expected="false"
  for good in "${resource_recipe_goods[@]}"; do
    if [[ "$recipe_name" == "Recipe.AllInOneResources.$good.blueprint.json" ]]; then
      expected="true"
    fi
  done
  for good in "${product_recipe_goods[@]}"; do
    if [[ "$recipe_name" == "Recipe.AllInOneProducts.$good.blueprint.json" ]]; then
      expected="true"
    fi
  done
  if [[ "$expected" != "true" ]]; then
    echo "wrong: unexpected all-in-one recipe ${recipe_file#$root/}"
    exit 1
  fi
done

check_json_value "$mod/manifest.json" '.Id' "public.timberborn.all-in-one-gen"
check_json_value "$mod/manifest.json" '.Version' "0.1.5"
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

folktails_template="$mod/TemplateCollections/TemplateCollection.Buildings.Folktails.blueprint.json"
ironteeth_template="$mod/TemplateCollections/TemplateCollection.Buildings.IronTeeth.blueprint.json"
check_json_value "$folktails_template" '.TemplateCollectionSpec.CollectionId' "Buildings.Folktails"
check_json_value "$folktails_template" '.TemplateCollectionSpec["Blueprints#append"] | length' "2"
check_json_value "$folktails_template" '.TemplateCollectionSpec["Blueprints#append"][0]' "Buildings/AllInOneGen/AllInOneResources/AllInOneResources.blueprint"
check_json_value "$folktails_template" '.TemplateCollectionSpec["Blueprints#append"][1]' "Buildings/AllInOneGen/AllInOneProducts/AllInOneProducts.blueprint"
check_json_value "$ironteeth_template" '.TemplateCollectionSpec.CollectionId' "Buildings.IronTeeth"
check_json_value "$ironteeth_template" '.TemplateCollectionSpec["Blueprints#append"] | length' "2"
check_json_value "$ironteeth_template" '.TemplateCollectionSpec["Blueprints#append"][0]' "Buildings/AllInOneGen/AllInOneResourcesIronTeeth/AllInOneResources.IronTeeth.blueprint"
check_json_value "$ironteeth_template" '.TemplateCollectionSpec["Blueprints#append"][1]' "Buildings/AllInOneGen/AllInOneProductsIronTeeth/AllInOneProducts.IronTeeth.blueprint"

check_building() {
  local file="$1"
  local template="$2"
  local prefix="$3"
  local order="$4"
  shift 4
  local goods=("$@")

  check_json_value "$file" '.TemplateSpec.TemplateName' "$template"
  check_json_value "$file" '.BuildingSpec.BuildingCost[0].Id' "Log"
  check_json_value "$file" '.BuildingSpec.BuildingCost[0].Amount' "1"
  check_json_value "$file" '.ManufactorySpec.ProductionRecipeIds | length' "${#goods[@]}"
  check_json_value "$file" '.PlaceableBlockObjectSpec.ToolGroupId' "KKCheats"
  check_json_value "$file" '.PlaceableBlockObjectSpec.ToolOrder' "$order"

  for good in "${goods[@]}"; do
    check_recipe "$prefix" "$good"
    if ! jq -e --arg recipe "$prefix.$good" \
      '.ManufactorySpec.ProductionRecipeIds | index($recipe) != null' "$file" >/dev/null; then
      echo "wrong: ${file#$root/} missing recipe $good"
      exit 1
    fi
  done
}

check_building "$mod/Buildings/AllInOneGen/AllInOneResources/AllInOneResources.blueprint.json" \
  "AllInOneResources.Folktails" "AllInOneResources" "10" "${folktails_resources[@]}"
check_building "$mod/Buildings/AllInOneGen/AllInOneProducts/AllInOneProducts.blueprint.json" \
  "AllInOneProducts.Folktails" "AllInOneProducts" "20" "${folktails_products[@]}"
check_building "$mod/Buildings/AllInOneGen/AllInOneResourcesIronTeeth/AllInOneResources.IronTeeth.blueprint.json" \
  "AllInOneResources.IronTeeth" "AllInOneResources" "10" "${ironteeth_resources[@]}"
check_building "$mod/Buildings/AllInOneGen/AllInOneProductsIronTeeth/AllInOneProducts.IronTeeth.blueprint.json" \
  "AllInOneProducts.IronTeeth" "AllInOneProducts" "20" "${ironteeth_products[@]}"

check_json_value "$mod/Buildings/AllInOneGen/AllInOneResources/AllInOneResources.blueprint.json" \
  '.Children["#Finished"].TimbermeshSpec.Model' "Buildings/Food/Grill/Grill.Folktails.Model"
check_json_value "$mod/Buildings/AllInOneGen/AllInOneResources/AllInOneResources.blueprint.json" \
  '.Children["#Unfinished"].Children.ConstructionStage0.TimbermeshSpec.Model' "Buildings/Food/Grill/Grill.Folktails.ConstructionStage0.Model"
check_json_value "$mod/Buildings/AllInOneGen/AllInOneProducts/AllInOneProducts.blueprint.json" \
  '.Children["#Finished"].TimbermeshSpec.Model' "Buildings/Food/Grill/Grill.Folktails.Model"
check_json_value "$mod/Buildings/AllInOneGen/AllInOneProducts/AllInOneProducts.blueprint.json" \
  '.Children["#Unfinished"].Children.ConstructionStage0.TimbermeshSpec.Model' "Buildings/Food/Grill/Grill.Folktails.ConstructionStage0.Model"
check_json_value "$mod/Buildings/AllInOneGen/AllInOneResourcesIronTeeth/AllInOneResources.IronTeeth.blueprint.json" \
  '.Children["#Finished"].TimbermeshSpec.Model' "Buildings/Food/OilPress/OilPress.IronTeeth.Model"
check_json_value "$mod/Buildings/AllInOneGen/AllInOneResourcesIronTeeth/AllInOneResources.IronTeeth.blueprint.json" \
  '.Children["#Unfinished"].Children.ConstructionStage0.TimbermeshSpec.Model' "Buildings/Food/OilPress/OilPress.IronTeeth.ConstructionStage0.Model"
check_json_value "$mod/Buildings/AllInOneGen/AllInOneProductsIronTeeth/AllInOneProducts.IronTeeth.blueprint.json" \
  '.Children["#Finished"].TimbermeshSpec.Model' "Buildings/Food/OilPress/OilPress.IronTeeth.Model"
check_json_value "$mod/Buildings/AllInOneGen/AllInOneProductsIronTeeth/AllInOneProducts.IronTeeth.blueprint.json" \
  '.Children["#Unfinished"].Children.ConstructionStage0.TimbermeshSpec.Model' "Buildings/Food/OilPress/OilPress.IronTeeth.ConstructionStage0.Model"

if ! grep -q 'Building.AllInOneResources.DisplayName' "$mod/Localizations/enUS.csv"; then
  echo "wrong: enUS localization missing AllInOneResources"
  exit 1
fi

if ! grep -q 'Building.AllInOneProducts.DisplayName' "$mod/Localizations/zhCN.csv"; then
  echo "wrong: zhCN localization missing AllInOneProducts"
  exit 1
fi

echo "All-in-One Gen checks passed"
