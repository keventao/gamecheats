#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mod="$root/mods/KKDoubleResources"

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

check_json_value "$mod/manifest.json" '.Id' "public.timberborn.double-resources"
check_json_value "$mod/manifest.json" '.MinimumGameVersion' "1.0.0.0"
check_json_value "$mod/manifest.json" '.RequiredMods[0].Id' "Harmony"

if [[ -d "$mod/NaturalResources" ]] && find "$mod/NaturalResources" -type f | grep -q .; then
  echo "unexpected: mods/KKDoubleResources/NaturalResources contains files; resource yield is runtime-patched"
  exit 1
fi

source_dir="$root/src/KKDoubleResources"
if [[ ! -f "$source_dir/MainModStarter.cs" ]] || [[ ! -f "$source_dir/YieldCarryMultiplierPatch.cs" ]]; then
  echo "missing: source files for runtime yield patch"
  exit 1
fi

if ! grep -q 'public.timberborn.double-resources' "$source_dir/MainModStarter.cs"; then
  echo "wrong: MainModStarter.cs Harmony id"
  exit 1
fi

if ! grep -q 'private const int Multiplier = 10;' "$source_dir/YieldCarryMultiplierPatch.cs"; then
  echo "wrong: YieldCarryMultiplierPatch.cs multiplier"
  exit 1
fi

if ! grep -q '"ScrapMetal"' "$source_dir/YieldCarryMultiplierPatch.cs"; then
  echo "wrong: YieldCarryMultiplierPatch.cs missing ScrapMetal"
  exit 1
fi

if ! grep -q '"Carrot"' "$source_dir/YieldCarryMultiplierPatch.cs"; then
  echo "wrong: YieldCarryMultiplierPatch.cs missing Carrot"
  exit 1
fi

for crop_good in CanolaSeeds Cassava CattailRoot Corn Eggplant Kohlrabi Potato Soybean Spadderdock SunflowerSeeds Wheat; do
  if ! grep -q "\"$crop_good\"" "$source_dir/YieldCarryMultiplierPatch.cs"; then
    echo "wrong: YieldCarryMultiplierPatch.cs missing $crop_good"
    exit 1
  fi
done

check_json_value "$mod/Buildings/Wood/LumberjackFlag/LumberjackFlag.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Wood/LumberjackFlag/LumberjackFlag.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Food/GathererFlag/GathererFlag.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Food/GathererFlag/GathererFlag.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Food/FarmHouse/FarmHouse.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Food/EfficientFarmHouse/EfficientFarmHouse.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Food/AquaticFarmhouse/AquaticFarmhouse.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Metal/ScavengerFlag/ScavengerFlag.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Metal/ScavengerFlag/ScavengerFlag.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Wood/TappersShack/TappersShack.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Wood/TappersShack/TappersShack.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"

storage_capacity_checks=(
  "Buildings/Storage/LargeIndustrialPile/LargeIndustrialPile.IronTeeth.blueprint.json 900"
  "Buildings/Storage/LargePile/LargePile.Folktails.blueprint.json 900"
  "Buildings/Storage/LargeTank/LargeTank.Folktails.blueprint.json 6000"
  "Buildings/Storage/LargeTank/LargeTank.IronTeeth.blueprint.json 6000"
  "Buildings/Storage/LargeWarehouse/LargeWarehouse.Folktails.blueprint.json 6000"
  "Buildings/Storage/LargeWarehouse/LargeWarehouse.IronTeeth.blueprint.json 6000"
  "Buildings/Storage/MediumTank/MediumTank.Folktails.blueprint.json 1500"
  "Buildings/Storage/MediumTank/MediumTank.IronTeeth.blueprint.json 1500"
  "Buildings/Storage/MediumWarehouse/MediumWarehouse.Folktails.blueprint.json 1000"
  "Buildings/Storage/MediumWarehouse/MediumWarehouse.IronTeeth.blueprint.json 1000"
  "Buildings/Storage/SmallIndustrialPile/SmallIndustrialPile.IronTeeth.blueprint.json 100"
  "Buildings/Storage/SmallPile/SmallPile.Folktails.blueprint.json 100"
  "Buildings/Storage/SmallTank/SmallTank.Folktails.blueprint.json 150"
  "Buildings/Storage/SmallTank/SmallTank.IronTeeth.blueprint.json 150"
  "Buildings/Storage/SmallWarehouse/SmallWarehouse.Folktails.blueprint.json 150"
  "Buildings/Storage/SmallWarehouse/SmallWarehouse.IronTeeth.blueprint.json 150"
  "Buildings/Storage/UndergroundPile/UndergroundPile.Folktails.blueprint.json 5000"
)

for storage_capacity_check in "${storage_capacity_checks[@]}"; do
  storage_file="${storage_capacity_check% *}"
  storage_capacity="${storage_capacity_check##* }"
  check_json_value "$mod/$storage_file" '.StockpileSpec.MaxCapacity' "$storage_capacity"
done

check_json_value "$mod/Characters/Beaver/BeaverAdult.blueprint.json" '.WalkerSpeedManagerSpec.BaseWalkingSpeed' "5.4"
check_json_value "$mod/Characters/Beaver/BeaverAdult.blueprint.json" '.WalkerSpeedManagerSpec.BaseSlowedSpeed' "2.7"
check_json_value "$mod/Characters/Beaver/BeaverChild.blueprint.json" '.WalkerSpeedManagerSpec.BaseWalkingSpeed' "2.7"
check_json_value "$mod/Characters/Beaver/BeaverChild.blueprint.json" '.WalkerSpeedManagerSpec.BaseSlowedSpeed' "1.3"
check_json_value "$mod/Characters/Bot/Bot.Folktails.blueprint.json" '.WalkerSpeedManagerSpec.BaseWalkingSpeed' "5.4"
check_json_value "$mod/Characters/Bot/Bot.Folktails.blueprint.json" '.WalkerSpeedManagerSpec.BaseSlowedSpeed' "2.7"
check_json_value "$mod/Characters/Bot/Bot.IronTeeth.blueprint.json" '.WalkerSpeedManagerSpec.BaseWalkingSpeed' "5.4"
check_json_value "$mod/Characters/Bot/Bot.IronTeeth.blueprint.json" '.WalkerSpeedManagerSpec.BaseSlowedSpeed' "2.7"

check_json_value "$mod/Recipes/Recipe.SciencePoints.blueprint.json" '.RecipeSpec.ProducedSciencePoints' "10"
check_json_value "$mod/Recipes/Recipe.SciencePointsNumbercruncher.blueprint.json" '.RecipeSpec.ProducedSciencePoints' "100"
check_json_value "$mod/Recipes/Recipe.SciencePointsObservatory.blueprint.json" '.RecipeSpec.ProducedSciencePoints' "100"
check_json_value "$mod/Recipes/Recipe.Water.blueprint.json" '.RecipeSpec.Products[0].Id' "Water"
check_json_value "$mod/Recipes/Recipe.Water.blueprint.json" '.RecipeSpec.Products[0].Amount' "5"
check_json_value "$mod/Recipes/Recipe.Water.Efficient.blueprint.json" '.RecipeSpec.Products[0].Id' "Water"
check_json_value "$mod/Recipes/Recipe.Water.Efficient.blueprint.json" '.RecipeSpec.Products[0].Amount' "25"

echo "KKDoubleResources checks passed"
