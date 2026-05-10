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

check_json_value "$mod/Buildings/Wood/LumberjackFlag/LumberjackFlag.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Wood/LumberjackFlag/LumberjackFlag.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Food/GathererFlag/GathererFlag.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Food/GathererFlag/GathererFlag.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Wood/TappersShack/TappersShack.Folktails.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"
check_json_value "$mod/Buildings/Wood/TappersShack/TappersShack.IronTeeth.blueprint.json" '.SimpleOutputInventorySpec.Capacity' "500"

check_json_value "$mod/Recipes/Recipe.SciencePoints.blueprint.json" '.RecipeSpec.ProducedSciencePoints' "10"
check_json_value "$mod/Recipes/Recipe.SciencePointsNumbercruncher.blueprint.json" '.RecipeSpec.ProducedSciencePoints' "100"
check_json_value "$mod/Recipes/Recipe.SciencePointsObservatory.blueprint.json" '.RecipeSpec.ProducedSciencePoints' "100"

echo "KKDoubleResources checks passed"
