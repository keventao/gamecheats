using System;
using System.Collections.Generic;
using System.Linq;
using FactoryTownCheats.Core;
using HarmonyLib;

namespace FactoryTownCheats.Modules
{
    internal static class OmniFactoryRecipes
    {
        private const BuildingType TargetBuilding = BuildingType.ItemGenerator;
        private const BuildCategoryType TargetBuildCategory = BuildCategoryType.BuildingBasic;
        private const ItemType InputItem = ItemType.Wood;
        private const int InputCount = 1;
        private const float CraftingTimeSeconds = 1f;

        private static readonly object SyncRoot = new();

        public static bool IsInjected { get; private set; }
        public static int InjectedCount { get; private set; }
        public static string LastStatus { get; private set; } = "not injected";

        public static void Register(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(Crafting), nameof(Crafting.Init));
            var postfix = new HarmonyMethod(typeof(OmniFactoryRecipes), nameof(OnCraftingInitPostfix));
            harmony.Patch(original, postfix: postfix);
        }

        public static void InjectNow()
        {
            InjectRecipes("manual");
        }

        private static void OnCraftingInitPostfix()
        {
            InjectRecipes("Crafting.Init postfix");
        }

        private static void InjectRecipes(string source)
        {
            lock (SyncRoot)
            {
                if (IsInjected)
                {
                    LastStatus = $"already injected: {InjectedCount} recipes";
                    return;
                }

                try
                {
                    var outputs = CandidateOutputs();
                    if (outputs.Count == 0)
                    {
                        LastStatus = "no candidate outputs";
                        Plugin.Log.LogWarning("[OmniFactory] No candidate outputs were found.");
                        return;
                    }

                    ConfigureTargetBuilding();
                    var data = Data.Instance;
                    AddToBuildMenu(data);
                    var added = AddRecipes(data, outputs);

                    InjectedCount = added;
                    IsInjected = added > 0;
                    LastStatus = $"injected {added} ItemGenerator recipes from {source}";
                    Plugin.Log.LogInfo($"[OmniFactory] {LastStatus}.");
                }
                catch (Exception ex)
                {
                    LastStatus = $"inject failed: {ex.GetType().Name}";
                    Plugin.Log.LogError($"[OmniFactory] Injection failed: {ex}");
                }
            }
        }

        private static List<ItemType> CandidateOutputs()
        {
            var outputs = new HashSet<ItemType>();

            AddRange(outputs, Crafting.physicalItemTypes);
            AddRange(outputs, Crafting.currencies);
            AddRange(outputs, Crafting.researchItemTypes);
            AddRange(outputs, Crafting.workerItemTypes);

            if (outputs.Count == 0)
            {
                foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
                {
                    outputs.Add(item);
                }
            }

            return outputs
                .Where(item => item != InputItem)
                .Where(item => OmniItemFilterPolicy.IsCandidateOutput(item.ToString()))
                .OrderBy(item => item.ToString(), StringComparer.Ordinal)
                .ToList();
        }

        private static void AddRange(ISet<ItemType> target, IEnumerable<ItemType>? source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var item in source)
            {
                target.Add(item);
            }
        }

        private static void ConfigureTargetBuilding()
        {
            var def = Crafting.GetCachedBuildingDef(TargetBuilding);
            def.enabled = true;
            def.allowMultipleRecipes = true;
            def.allowAutoManageRecipes = false;
            def.forceAutoManageRecipes = false;
            def.assignDefaultOnCreate = false;
            def.hasPhysicalOutputItem = true;

            if (def.fixedInputItems != null && !def.fixedInputItems.Contains(InputItem))
            {
                def.fixedInputItems.Add(InputItem);
            }

            def.CalcDerivedData();
        }

        private static void AddToBuildMenu(Data data)
        {
            AddUnique(data.buildCategories, TargetBuildCategory);

            if (!data.defaultDisplayCategories.TryGetValue(TargetBuildCategory, out var entities))
            {
                entities = new List<EntityId>();
                data.defaultDisplayCategories[TargetBuildCategory] = entities;
            }

            AddUnique(entities, EntityId.FromBuilding(TargetBuilding));
        }

        private static int AddRecipes(Data data, IEnumerable<ItemType> outputs)
        {
            var defaultRecipes = data.defaultRecipeDefs;
            var defaultBuildingRecipes = GetOrCreateBuildingRecipeList(data.defaultBuildingRecipes, TargetBuilding);
            var cachedBuildingRecipes = GetOrCreateBuildingRecipeList(Crafting.cachedBuildingRecipes, TargetBuilding);
            var added = 0;

            foreach (var output in outputs)
            {
                var recipeType = (RecipeType)OmniItemFilterPolicy.RecipeIdForOutput((int)output);
                if (!Crafting.recipeCache.ContainsKey(recipeType))
                {
                    var recipe = CreateRecipe(recipeType, output);
                    Crafting.recipeCache[recipeType] = recipe;
                    defaultRecipes[recipeType] = recipe;
                    added++;
                }

                AddUnique(defaultBuildingRecipes, recipeType);
                AddUnique(cachedBuildingRecipes, recipeType);
                AddProductionSource(output, TargetBuilding);
                Crafting.allAvailableOutputs?.Add(output);
            }

            return added;
        }

        private static Recipe CreateRecipe(RecipeType recipeType, ItemType output)
        {
            var recipe = new Recipe(recipeType);
            recipe.LoadBasic(InputItem, InputCount, output);
            recipe.type = recipeType;
            recipe.category = RecipeCategory.DefaultItem;
            recipe.craftingTime = CraftingTimeSeconds;
            recipe.enabled = true;
            recipe.suppressNotification = true;
            recipe.FinalizeMetadata();
            return recipe;
        }

        private static List<RecipeType> GetOrCreateBuildingRecipeList(
            IDictionary<BuildingType, List<RecipeType>> map,
            BuildingType buildingType)
        {
            if (!map.TryGetValue(buildingType, out var list))
            {
                list = new List<RecipeType>();
                map[buildingType] = list;
            }

            return list;
        }

        private static void AddUnique<T>(ICollection<T> list, T value)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
            }
        }

        private static void AddProductionSource(ItemType output, BuildingType building)
        {
            if (!Crafting.derivedItemProductionSources.TryGetValue(output, out var sources))
            {
                sources = new List<BuildingType>();
                Crafting.derivedItemProductionSources[output] = sources;
            }

            AddUnique(sources, building);
        }
    }
}
