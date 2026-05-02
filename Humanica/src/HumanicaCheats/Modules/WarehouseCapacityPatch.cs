using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using HumanicaCheats.Core;

namespace HumanicaCheats.Modules
{
    internal static class WarehouseCapacityPatch
    {
        private static readonly string[] TypeNameHints =
        {
            "Warehouse",
            "Inventory",
            "Storage",
            "ResourceManagement"
        };

        private static readonly string[] CapacityMethodNames =
        {
            "get_Capacity",
            "get_MaxCapacity",
            "Capacity",
            "MaxCapacity",
            "GetCapacity",
            "GetMaxCapacity",
            "CalculateCapacity",
            "CalculateMaxCapacity"
        };

        private static readonly PropertyInfo? WarehouseCapacityMultiplierProperty =
            typeof(ResourceCheats).GetProperty(
                "WarehouseCapacityMultiplier",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        public static bool Register(HarmonyLib.Harmony harmony)
        {
            try
            {
                var candidates = FindCandidates()
                    .GroupBy(MethodKey)
                    .Select(g => g.First())
                    .ToList();
                if (candidates.Count == 0)
                {
                    MelonLogger.Warning("[WarehouseCapacityPatch] No capacity candidates found.");
                    return false;
                }

                int patched = 0;
                foreach (var candidate in candidates)
                {
                    try
                    {
                        var postfix = candidate.ReturnType == typeof(int)
                            ? new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(IntPostfix))
                            : new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(FloatPostfix));

                        harmony.Patch(candidate, postfix: postfix);
                        patched++;
                        MelonLogger.Msg($"[WarehouseCapacityPatch] Patched {candidate.DeclaringType?.FullName}.{candidate.Name} -> {candidate.ReturnType.Name}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WarehouseCapacityPatch] Skipped {candidate.DeclaringType?.FullName}.{candidate.Name}: {ex.Message}");
                    }
                }

                MelonLogger.Msg($"[WarehouseCapacityPatch] Patched {patched} capacity method(s).");
                return patched > 0;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WarehouseCapacityPatch] Registration failed: {ex.Message}");
                return false;
            }
        }

        private static IEnumerable<MethodInfo> FindCandidates()
        {
            foreach (var type in GetAssemblyCSharpTypes())
            {
                string fullName = type.FullName ?? type.Name;
                if (!HasAnyHint(fullName, TypeNameHints)) continue;

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!IsCapacityCandidate(method)) continue;
                    yield return method;
                }
            }
        }

        private static string MethodKey(MethodInfo method)
        {
            return $"{method.Module.ModuleVersionId}:{method.MetadataToken}";
        }

        private static IEnumerable<Type> GetAssemblyCSharpTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string? name = assembly.GetName().Name;
                if (!string.Equals(name, "Assembly-CSharp", StringComparison.OrdinalIgnoreCase)) continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }

                foreach (var type in types) yield return type;
            }
        }

        private static bool IsCapacityCandidate(MethodInfo method)
        {
            if (method.IsStatic) return false;
            if (method.IsAbstract) return false;
            if (method.ContainsGenericParameters) return false;
            if (method.GetParameters().Length != 0) return false;
            if (method.ReturnType != typeof(int) && method.ReturnType != typeof(float)) return false;

            string name = method.Name;
            if (name.StartsWith("set_", StringComparison.Ordinal)) return false;
            return IsCapacityMethodName(name);
        }

        private static bool IsCapacityMethodName(string name)
        {
            for (int i = 0; i < CapacityMethodNames.Length; i++)
            {
                if (string.Equals(name, CapacityMethodNames[i], StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static bool HasAnyHint(string value, string[] hints)
        {
            for (int i = 0; i < hints.Length; i++)
            {
                if (value.IndexOf(hints[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static int GetWarehouseCapacityMultiplier()
        {
            try
            {
                if (!GameRefs.IsReady) return 1;
                if (WarehouseCapacityMultiplierProperty == null) return 1;

                object? value = WarehouseCapacityMultiplierProperty.GetValue(null);
                return value is int multiplier ? multiplier : 1;
            }
            catch
            {
                return 1;
            }
        }

        private static void IntPostfix(ref int __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (multiplier <= 1 || __result <= 0) return;

            long scaled = (long)__result * multiplier;
            __result = scaled > int.MaxValue ? int.MaxValue : (int)scaled;
        }

        private static void FloatPostfix(ref float __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (multiplier <= 1 || __result <= 0f) return;

            __result *= multiplier;
        }
    }
}
