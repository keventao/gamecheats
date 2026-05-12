#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mod="$root/mods/all-in-one-gen"

resource_goods=(
  Algae Badwater Berries CanolaSeeds Carrot Cassava CattailRoot Chestnut
  CoffeeBean Corn Dandelion Dirt Eggplant Kohlrabi Log MangroveFruit
  MapleSyrup Mushroom PineResin Potato ScrapMetal Soybean Spadderdock
  SunflowerSeeds Water Wheat
)

product_goods=(
  AlgaeRation Antidote Biofuel Book BotChassis BotHead BotLimb Bread CanolaOil
  Catalyst CattailCracker CattailFlour Coffee CornRation EggplantRation
  Explosives Extract FermentedCassava FermentedMushroom FermentedSoybean
  Fireworks Gear GrilledChestnut GrilledPotato GrilledSpadderdock
  MaplePastry MetalBlock MetalPart Paper Plank PunchCard TreatedPlank
  WheatFlour
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

check_json_value "$mod/manifest.json" '.Id' "public.timberborn.all-in-one-gen"
check_json_value "$mod/manifest.json" '.MinimumGameVersion' "1.0.0.0"
check_json_value "$mod/manifest.json" '.RequiredMods | length' "0"

good_collection_file="$mod/GoodCollections/GoodCollection.Common.blueprint.json"
check_json_value "$good_collection_file" '.GoodCollectionSpec.CollectionId' "Common"

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

check_json_value "$products_building" '.TemplateSpec.TemplateName' "AllInOneProducts"
check_json_value "$products_building" '.BuildingSpec.BuildingCost[0].Id' "Log"
check_json_value "$products_building" '.BuildingSpec.BuildingCost[0].Amount' "1"
check_json_value "$products_building" '.ManufactorySpec.ProductionRecipeIds | length' "${#product_goods[@]}"
check_json_value "$products_building" '.LabeledEntitySpec.DisplayNameLocKey' "Building.AllInOneProducts.DisplayName"

for good in "${resource_goods[@]}"; do
  check_recipe "AllInOneResources" "$good"
  if ! jq -e --arg recipe "AllInOneResources.$good" \
    '.ManufactorySpec.ProductionRecipeIds | index($recipe) != null' "$resources_building" >/dev/null; then
    echo "wrong: AllInOneResources missing recipe $good"
    exit 1
  fi

  if ! jq -e --arg good "$good" \
    '.GoodCollectionSpec["Goods#append"] | index($good) != null' "$good_collection_file" >/dev/null; then
    case "$good" in
      Badwater|Berries|Dirt|Log|PineResin|ScrapMetal|Water)
        ;;
      *)
        echo "wrong: GoodCollection.Common missing $good"
        exit 1
        ;;
    esac
  fi
done

for good in "${product_goods[@]}"; do
  check_recipe "AllInOneProducts" "$good"
  if ! jq -e --arg recipe "AllInOneProducts.$good" \
    '.ManufactorySpec.ProductionRecipeIds | index($recipe) != null' "$products_building" >/dev/null; then
    echo "wrong: AllInOneProducts missing recipe $good"
    exit 1
  fi

  if ! jq -e --arg good "$good" \
    '.GoodCollectionSpec["Goods#append"] | index($good) != null' "$good_collection_file" >/dev/null; then
    case "$good" in
      Explosives|Extract|Fireworks|Gear|MetalBlock|Plank)
        ;;
      *)
        echo "wrong: GoodCollection.Common missing $good"
        exit 1
        ;;
    esac
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
