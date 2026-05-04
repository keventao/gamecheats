using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;

namespace HumanicaCheats.Modules
{
    internal static class WarehouseCapacityPatch
    {
        public static bool Register(HarmonyLib.Harmony harmony)
        {
            MelonLogger.Warning("[WarehouseCapacityPatch] Disabled: no always-on warehouse resize patches are installed.");
            return false;
        }

        public readonly struct ExpansionResult
        {
            public ExpansionResult(
                int attempted,
                int expanded,
                int shrunk,
                int skipped,
                IReadOnlyList<string> errors,
                string baselinePacksCsv)
            {
                Attempted = attempted;
                Expanded = expanded;
                Shrunk = shrunk;
                Skipped = skipped;
                Errors = errors;
                BaselinePacksCsv = baselinePacksCsv;
            }

            public int Attempted { get; }
            public int Expanded { get; }
            public int Shrunk { get; }
            public int Skipped { get; }
            public IReadOnlyList<string> Errors { get; }
            public string BaselinePacksCsv { get; }
            public bool HasErrors => Errors.Count > 0;
        }

        public static ExpansionResult ExpandWarehousesOnce(int multiplier, int previousMultiplier, string baselinePacksCsv)
        {
            var errors = new List<string>();
            var baselinePacks = ParseBaselinePacks(baselinePacksCsv);
            if (multiplier <= 1)
            {
                return new ExpansionResult(0, 0, 0, 0, errors, FormatBaselinePacks(baselinePacks));
            }

            object? villageData = Il2Cpp.S.VillageData;
            if (villageData == null)
            {
                errors.Add("VillageData is not ready.");
                return new ExpansionResult(0, 0, 0, 0, errors, FormatBaselinePacks(baselinePacks));
            }

            object? warehouses = ReadMember(villageData, "Warehouses");
            if (warehouses == null)
            {
                errors.Add("VillageData.Warehouses was not found.");
                return new ExpansionResult(0, 0, 0, 0, errors, FormatBaselinePacks(baselinePacks));
            }

            int attempted = 0;
            int expanded = 0;
            int shrunk = 0;
            int skipped = 0;
            int index = 0;
            foreach (object? inventory in EnumerateListItems(warehouses, errors))
            {
                if (inventory == null)
                {
                    skipped++;
                    index++;
                    continue;
                }

                attempted++;
                try
                {
                    int currentPacks = GetPackCount(inventory);
                    if (currentPacks <= 0)
                    {
                        skipped++;
                        errors.Add($"{InventoryLabel(inventory)} has no readable pack count.");
                        index++;
                        continue;
                    }

                    int baseline = GetOrInferBaseline(baselinePacks, index, currentPacks, previousMultiplier);
                    long desired = (long)baseline * multiplier;
                    int targetPacks = desired > int.MaxValue ? int.MaxValue : (int)desired;
                    SetBaseline(baselinePacks, index, baseline);

                    if (targetPacks == currentPacks)
                    {
                        skipped++;
                        MelonLogger.Msg($"[WarehouseCapacityPatch] {InventoryLabel(inventory)} already at x{multiplier}: packs={currentPacks}, baseline={baseline}");
                        index++;
                        continue;
                    }

                    MethodInfo? resize = FindResizeInventory(inventory.GetType());
                    if (resize == null)
                    {
                        skipped++;
                        errors.Add($"{InventoryLabel(inventory)} has no ResizeInventory(int).");
                        index++;
                        continue;
                    }

                    if (targetPacks < currentPacks)
                    {
                        skipped++;
                        errors.Add($"{InventoryLabel(inventory)} shrink blocked: current packs {currentPacks}, target packs {targetPacks}. Keeping larger warehouse to avoid item loss.");
                        index++;
                        continue;
                    }

                    resize.Invoke(inventory, new object[] { targetPacks });
                    if (targetPacks > currentPacks)
                    {
                        MelonLogger.Msg($"[WarehouseCapacityPatch] Expanded {InventoryLabel(inventory)} packs {currentPacks} -> {targetPacks} (baseline={baseline}, x{multiplier})");
                        expanded++;
                    }
                    else
                    {
                        MelonLogger.Msg($"[WarehouseCapacityPatch] Shrunk {InventoryLabel(inventory)} packs {currentPacks} -> {targetPacks} (baseline={baseline}, x{multiplier})");
                        shrunk++;
                    }
                }
                catch (Exception ex)
                {
                    skipped++;
                    errors.Add($"{InventoryLabel(inventory)} failed: {UnwrapMessage(ex)}");
                }

                index++;
            }

            string newBaselineCsv = FormatBaselinePacks(baselinePacks);
            MelonLogger.Msg($"[WarehouseCapacityPatch] Expansion x{multiplier}: attempted={attempted}, expanded={expanded}, shrunk={shrunk}, skipped={skipped}, errors={errors.Count}, baseline={newBaselineCsv}");
            return new ExpansionResult(attempted, expanded, shrunk, skipped, errors, newBaselineCsv);
        }

        private static List<int> ParseBaselinePacks(string value)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(value)) return result;

            string[] parts = value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                result.Add(int.TryParse(parts[i], out int parsed) && parsed > 0 ? parsed : -1);
            }
            return result;
        }

        private static string FormatBaselinePacks(List<int> baselinePacks)
        {
            return string.Join(",", baselinePacks);
        }

        private static int GetOrInferBaseline(List<int> baselinePacks, int index, int currentPacks, int previousMultiplier)
        {
            int savedBaseline = index < baselinePacks.Count ? baselinePacks[index] : -1;
            return WarehouseCapacityBaselinePolicy.InferBaseline(savedBaseline, currentPacks, previousMultiplier);
        }

        private static void SetBaseline(List<int> baselinePacks, int index, int baseline)
        {
            while (baselinePacks.Count <= index)
            {
                baselinePacks.Add(-1);
            }
            baselinePacks[index] = baseline;
        }

        private static IEnumerable<object?> EnumerateListItems(object list, List<string> errors)
        {
            int count = ReadIntMember(list, "Count");
            if (count < 0) count = ReadIntMember(list, "_size");
            MethodInfo? getItem = list.GetType().GetMethod("get_Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (count >= 0 && getItem != null)
            {
                for (int i = 0; i < count; i++)
                {
                    object? item = null;
                    try
                    {
                        item = getItem.Invoke(list, new object[] { i });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Warehouse list item {i} failed: {UnwrapMessage(ex)}");
                    }
                    yield return item;
                }
                yield break;
            }

            if (list is IEnumerable enumerable)
            {
                foreach (object? item in enumerable)
                {
                    yield return item;
                }
                yield break;
            }

            errors.Add($"Cannot enumerate warehouse list from {list.GetType().FullName}.");
        }

        private static int GetPackCount(object inventory)
        {
            string[] names =
            {
                "PacksAmount",
                "storedPacksAmount",
                "StoredPacksAmount",
                "StoredVal",
                "StoredRes",
                "Slots"
            };

            for (int i = 0; i < names.Length; i++)
            {
                int value = ReadIntMember(inventory, names[i]);
                if (value > 0) return value;

                object? member = ReadMember(inventory, names[i]);
                int count = ReadCollectionCount(member);
                if (count > 0) return count;
            }

            MethodInfo? getFreePacks = inventory.GetType().GetMethod("GetFreePacksAmount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (getFreePacks != null && getFreePacks.GetParameters().Length == 0)
            {
                object? raw = getFreePacks.Invoke(inventory, Array.Empty<object>());
                if (raw is int value && value > 0) return value;
            }

            return -1;
        }

        private static bool CanShrinkWithoutLosingStoredPacks(object inventory, int targetPacks, int currentPacks)
        {
            object? storedValues = ReadMember(inventory, "StoredVal");
            if (storedValues == null) storedValues = ReadMember(inventory, "storedVal");
            if (storedValues == null) storedValues = ReadMember(inventory, "StoredValues");
            if (storedValues == null) return false;

            int count = ReadCollectionCount(storedValues);
            if (count < currentPacks) return false;

            for (int i = targetPacks; i < currentPacks; i++)
            {
                object? raw = ReadCollectionItem(storedValues, i);
                try
                {
                    if (raw != null && Convert.ToInt32(raw) != 0) return false;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        private static object? ReadCollectionItem(object collection, int index)
        {
            if (collection is Array array) return array.GetValue(index);

            var type = collection.GetType();
            MethodInfo? getItem = type.GetMethod("get_Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (getItem != null)
            {
                return getItem.Invoke(collection, new object[] { index });
            }

            return null;
        }

        private static int ReadCollectionCount(object? value)
        {
            if (value == null) return -1;
            if (value is Array array) return array.Length;

            foreach (string name in new[] { "Length", "Count" })
            {
                int count = ReadIntMember(value, name);
                if (count >= 0) return count;
            }

            return -1;
        }

        private static int ReadIntMember(object instance, string name)
        {
            object? value = ReadMember(instance, name);
            if (value is int intValue) return intValue;
            try
            {
                return value == null ? -1 : Convert.ToInt32(value);
            }
            catch
            {
                return -1;
            }
        }

        private static object? ReadMember(object instance, string name)
        {
            var type = instance.GetType();
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(instance);
            }

            var method = type.GetMethod("get_" + name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null && method.GetParameters().Length == 0)
            {
                return method.Invoke(instance, Array.Empty<object>());
            }

            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(instance);
        }

        private static MethodInfo? FindResizeInventory(Type inventoryType)
        {
            var method = inventoryType.GetMethod("ResizeInventory", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) return null;

            var parameters = method.GetParameters();
            return parameters.Length == 1 && parameters[0].ParameterType == typeof(int) ? method : null;
        }

        private static string InventoryLabel(object inventory)
        {
            return inventory.GetType().FullName ?? inventory.GetType().Name;
        }

        private static string UnwrapMessage(Exception ex)
        {
            while (ex is TargetInvocationException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }
    }
}
