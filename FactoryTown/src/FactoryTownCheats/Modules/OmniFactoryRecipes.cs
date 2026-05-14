using System;
using System.Collections.Generic;
using System.Linq;
using FactoryTownCheats.Core;
using HarmonyLib;
using UnityEngine;

namespace FactoryTownCheats.Modules
{
    internal static class OmniFactoryRecipes
    {
        private static readonly BuildingType TargetBuilding = (BuildingType)OmniFactoryDefinition.CustomBuildingId;

        private const BuildingType VisualSourceBuilding = BuildingType.Workshop;
        private const BuildCategoryType TargetCategory = BuildCategoryType.BuildingBasic;
        private const ItemType InputItem = ItemType.Wood;
        private const int InputCount = 1;
        private const float CraftingTimeSeconds = 1f;

        private static readonly object SyncRoot = new();

        public static bool IsInjected { get; private set; }
        public static int InjectedCount { get; private set; }
        public static string LastStatus { get; private set; } = "not injected";

        public static void Register(Harmony harmony)
        {
            PatchPostfix(harmony, typeof(Crafting), nameof(Crafting.Init), nameof(OnCraftingInitPostfix));
            PatchPrefix(harmony, typeof(Building), nameof(Building.PrefabForBuilding), nameof(PrefabForBuildingPrefix));
            PatchPrefix(harmony, typeof(Building), nameof(Building.PrefabForBuildingLevel), nameof(PrefabForBuildingLevelPrefix));
            PatchPrefix(harmony, typeof(IconManager), nameof(IconManager.SpriteForBuilding), nameof(SpriteForBuildingPrefix));
            PatchPrefix(harmony, typeof(TextDisplay), nameof(TextDisplay.LabelForBuilding), nameof(LabelForBuildingPrefix));
            PatchPrefix(harmony, typeof(TextDisplay), nameof(TextDisplay.DefaultLabelForBuilding), nameof(DefaultLabelForBuildingPrefix));
            PatchPrefix(harmony, typeof(TextDisplay), nameof(TextDisplay.DescriptionForBuilding), nameof(DescriptionForBuildingPrefix));
            PatchPrefix(harmony, typeof(Building), nameof(Building.Title), nameof(BuildingTitlePrefix));
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
                try
                {
                    var outputs = CandidateOutputs();
                    if (outputs.Count == 0)
                    {
                        LastStatus = "no candidate outputs";
                        Plugin.Log.LogWarning("[OmniFactory] No candidate outputs were found.");
                        return;
                    }

                    var data = Data.Instance;
                    ConfigureTargetBuilding(data);
                    RegisterDisplayCategory(data);
                    var recipeCount = AddRecipes(data, outputs);

                    InjectedCount = recipeCount;
                    IsInjected = recipeCount > 0;
                    LastStatus = $"ensured {recipeCount} {OmniFactoryDefinition.DisplayName} recipes from {source}";
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

        private static void ConfigureTargetBuilding(Data data)
        {
            var source = Crafting.GetCachedBuildingDef(VisualSourceBuilding);
            var def = CloneBuildingDef(source, TargetBuilding);
            def.enabled = true;
            def.allowMultipleRecipes = true;
            def.allowAutoManageRecipes = false;
            def.forceAutoManageRecipes = false;
            def.assignDefaultOnCreate = false;
            def.hasPhysicalOutputItem = true;
            def.fixedInputItems = new List<ItemType> { InputItem };
            def.requirements.Clear();
            def.cost.Clear();

            def.CalcDerivedData();

            data.defaultBuildingDefs[TargetBuilding] = def;
            Crafting.buildingCache[TargetBuilding] = def;
        }

        private static BuildingDef CloneBuildingDef(BuildingDef source, BuildingType type)
        {
            var def = new BuildingDef(type)
            {
                maxWorkerCount = source.maxWorkerCount,
                bonusPerWorker = source.bonusPerWorker,
                canSupportWorkers = source.canSupportWorkers,
                enabled = source.enabled,
                isStorage = source.isStorage,
                isMarket = source.isMarket,
                usesSharedInventory = source.usesSharedInventory,
                isAdjacentTransferAllowed = source.isAdjacentTransferAllowed,
                hasItemFilter = source.hasItemFilter,
                specialty = source.specialty,
                footprintOverride = source.footprintOverride,
                footprintHeightOverride = source.footprintHeightOverride,
                footprintBaseOverride = source.footprintBaseOverride,
                areaOfEffectRadius = source.areaOfEffectRadius,
                allowAutoManageRecipes = source.allowAutoManageRecipes,
                forceAutoManageRecipes = source.forceAutoManageRecipes,
                assignDefaultOnCreate = source.assignDefaultOnCreate,
                allowMultipleRecipes = source.allowMultipleRecipes,
                drawInventoryAsGrid = source.drawInventoryAsGrid,
                canLinkToWater = source.canLinkToWater,
                canAbsorbWater = source.canAbsorbWater,
                distributedGoodsFilter = source.distributedGoodsFilter,
                fixedInputItems = source.fixedInputItems == null ? null : new List<ItemType>(source.fixedInputItems),
                maxCountPerTechLevel = CloneArray(source.maxCountPerTechLevel),
                maxCountPerHappinessLevel = CloneArray(source.maxCountPerHappinessLevel),
                maxCountPerResearchLevel = CloneArray(source.maxCountPerResearchLevel),
                populationProvided = CloneArray(source.populationProvided),
                accessPointTemplates = source.accessPointTemplates == null
                    ? null
                    : (AccessPointTemplate[])source.accessPointTemplates.Clone(),
                fixedInventoryFilter = source.fixedInventoryFilter,
                hasPhysicalOutputItem = source.hasPhysicalOutputItem,
                blockHeight = source.blockHeight,
                airshipPickupHeight = source.airshipPickupHeight
            };

            return def;
        }

        private static int[]? CloneArray(int[]? source)
        {
            return source == null ? null : (int[])source.Clone();
        }

        private static void RegisterDisplayCategory(Data data)
        {
            AddToCategory(data.defaultDisplayCategories, TargetCategory);
            AddToCategory(Crafting.displayCategories, TargetCategory);
        }

        private static void AddToCategory(IDictionary<BuildCategoryType, List<EntityId>> categories, BuildCategoryType category)
        {
            if (!categories.TryGetValue(category, out var list))
            {
                list = new List<EntityId>();
                categories[category] = list;
            }

            var entityId = EntityId.FromBuilding(TargetBuilding);
            if (!list.Contains(entityId))
            {
                list.Insert(0, entityId);
            }
        }

        private static int AddRecipes(Data data, IEnumerable<ItemType> outputs)
        {
            var defaultRecipes = data.defaultRecipeDefs;
            var defaultBuildingRecipes = GetOrCreateBuildingRecipeList(data.defaultBuildingRecipes, TargetBuilding);
            var cachedBuildingRecipes = GetOrCreateBuildingRecipeList(Crafting.cachedBuildingRecipes, TargetBuilding);
            var ensured = 0;

            foreach (var output in outputs)
            {
                var recipeType = (RecipeType)OmniItemFilterPolicy.RecipeIdForOutput((int)output);
                if (!Crafting.recipeCache.TryGetValue(recipeType, out var recipe))
                {
                    recipe = CreateRecipe(recipeType, output);
                    Crafting.recipeCache[recipeType] = recipe;
                }

                defaultRecipes[recipeType] = recipe;
                AddUnique(defaultBuildingRecipes, recipeType);
                AddUnique(cachedBuildingRecipes, recipeType);
                AddProductionSource(output, TargetBuilding);
                Crafting.allAvailableOutputs?.Add(output);
                ensured++;
            }

            return ensured;
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

        private static void PatchPostfix(Harmony harmony, Type targetType, string targetMethodName, string patchMethodName)
        {
            var original = AccessTools.Method(targetType, targetMethodName);
            var postfix = new HarmonyMethod(typeof(OmniFactoryRecipes), patchMethodName);
            harmony.Patch(original, postfix: postfix);
        }

        private static void PatchPrefix(Harmony harmony, Type targetType, string targetMethodName, string patchMethodName)
        {
            var original = AccessTools.Method(targetType, targetMethodName);
            var prefix = new HarmonyMethod(typeof(OmniFactoryRecipes), patchMethodName);
            harmony.Patch(original, prefix: prefix);
        }

        private static bool PrefabForBuildingPrefix(BuildingType type, ref GameObject __result)
        {
            if (!IsTargetBuilding(type))
            {
                return true;
            }

            __result = Building.PrefabForBuilding(VisualSourceBuilding);
            return false;
        }

        private static bool PrefabForBuildingLevelPrefix(BuildingType type, int numUpgrades, ref GameObject __result)
        {
            if (!IsTargetBuilding(type))
            {
                return true;
            }

            __result = Building.PrefabForBuilding(VisualSourceBuilding);
            return false;
        }

        private static bool SpriteForBuildingPrefix(BuildingType type, ref Sprite __result)
        {
            if (!IsTargetBuilding(type))
            {
                return true;
            }

            __result = IconManager.SpriteForBuilding(VisualSourceBuilding);
            return false;
        }

        private static bool LabelForBuildingPrefix(BuildingType b, ref string __result)
        {
            if (!IsTargetBuilding(b))
            {
                return true;
            }

            __result = OmniFactoryDefinition.DisplayName;
            return false;
        }

        private static bool DefaultLabelForBuildingPrefix(BuildingType b, ref string __result)
        {
            return LabelForBuildingPrefix(b, ref __result);
        }

        private static bool DescriptionForBuildingPrefix(BuildingType b, ref string __result)
        {
            if (!IsTargetBuilding(b))
            {
                return true;
            }

            __result = OmniFactoryDefinition.Description;
            return false;
        }

        private static bool BuildingTitlePrefix(Building __instance, ref string __result)
        {
            if (!IsTargetBuilding(__instance.type))
            {
                return true;
            }

            __result = OmniFactoryDefinition.DisplayName;
            return false;
        }

        private static bool IsTargetBuilding(BuildingType type)
        {
            return (int)type == OmniFactoryDefinition.CustomBuildingId;
        }
    }
}
