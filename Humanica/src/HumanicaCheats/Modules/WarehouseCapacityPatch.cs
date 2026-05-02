using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
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

        private static readonly string[] FreeCapacityMethodNames =
        {
            "get_FreeCapacity",
            "get_AvailableCapacity",
            "get_RemainingCapacity",
            "get_FreeSpace",
            "get_AvailableSpace",
            "GetFreeCapacity",
            "GetAvailableCapacity",
            "GetRemainingCapacity",
            "GetFreeSpace",
            "GetAvailableSpace",
            "CalculateFreeCapacity",
            "CalculateAvailableCapacity",
            "CalculateRemainingCapacity"
        };

        private static readonly string[] PackSizeMethodNames =
        {
            "get_PackSize",
            "GetPackSize",
            "CalculatePackSize"
        };

        private static readonly PropertyInfo? WarehouseCapacityMultiplierProperty =
            typeof(ResourceCheats).GetProperty(
                "WarehouseCapacityMultiplier",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly Dictionary<IntPtr, int> LastIntCapacityByInstance = new Dictionary<IntPtr, int>();
        private static readonly Dictionary<IntPtr, float> LastFloatCapacityByInstance = new Dictionary<IntPtr, float>();
        private static readonly Dictionary<IntPtr, int> OriginalStoredPackSizeByInstance = new Dictionary<IntPtr, int>();
        private static readonly HashSet<string> LoggedStoredPackSizeFailures = new HashSet<string>();

        public static bool Register(HarmonyLib.Harmony harmony)
        {
            try
            {
                var capacityCandidates = FindCandidates(IsCapacityMethodName)
                    .GroupBy(MethodKey)
                    .Select(g => g.First())
                    .ToList();
                var freeCapacityCandidates = FindCandidates(IsFreeCapacityMethodName)
                    .GroupBy(MethodKey)
                    .Select(g => g.First())
                    .ToList();
                var packSizeCandidates = FindCandidates(IsPackSizeMethodName)
                    .GroupBy(MethodKey)
                    .Select(g => g.First())
                    .ToList();

                if (capacityCandidates.Count == 0)
                {
                    MelonLogger.Warning("[WarehouseCapacityPatch] No capacity candidates found.");
                    return false;
                }

                int patched = 0;
                foreach (var candidate in capacityCandidates)
                {
                    try
                    {
                        var postfix = candidate.ReturnType == typeof(int)
                            ? new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(IntCapacityPostfix))
                            : new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(FloatCapacityPostfix));

                        harmony.Patch(candidate, postfix: postfix);
                        patched++;
                        MelonLogger.Msg($"[WarehouseCapacityPatch] Patched {candidate.DeclaringType?.FullName}.{candidate.Name} -> {candidate.ReturnType.Name}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WarehouseCapacityPatch] Skipped {candidate.DeclaringType?.FullName}.{candidate.Name}: {ex.Message}");
                    }
                }

                int freePatched = 0;
                foreach (var candidate in freeCapacityCandidates)
                {
                    try
                    {
                        var postfix = candidate.ReturnType == typeof(int)
                            ? new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(IntFreeCapacityPostfix))
                            : new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(FloatFreeCapacityPostfix));

                        harmony.Patch(candidate, postfix: postfix);
                        freePatched++;
                        MelonLogger.Msg($"[WarehouseCapacityPatch] Patched free {candidate.DeclaringType?.FullName}.{candidate.Name} -> {candidate.ReturnType.Name}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WarehouseCapacityPatch] Skipped free {candidate.DeclaringType?.FullName}.{candidate.Name}: {ex.Message}");
                    }
                }

                if (freePatched == 0)
                {
                    MelonLogger.Warning("[WarehouseCapacityPatch] No free-capacity candidates found. Used-capacity display may be wrong.");
                }

                int packPatched = 0;
                foreach (var candidate in packSizeCandidates)
                {
                    try
                    {
                        var postfix = candidate.ReturnType == typeof(int)
                            ? new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(IntPackSizePostfix))
                            : new HarmonyMethod(typeof(WarehouseCapacityPatch), nameof(FloatPackSizePostfix));

                        harmony.Patch(candidate, postfix: postfix);
                        packPatched++;
                        MelonLogger.Msg($"[WarehouseCapacityPatch] Patched pack-size {candidate.DeclaringType?.FullName}.{candidate.Name} -> {candidate.ReturnType.Name}");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"[WarehouseCapacityPatch] Skipped pack-size {candidate.DeclaringType?.FullName}.{candidate.Name}: {ex.Message}");
                    }
                }

                if (packPatched == 0)
                {
                    MelonLogger.Warning("[WarehouseCapacityPatch] No pack-size candidates found. Slots may still cap storage.");
                }

                MelonLogger.Msg($"[WarehouseCapacityPatch] Patched {patched} capacity method(s), {freePatched} free-capacity method(s), {packPatched} pack-size method(s).");
                return patched > 0;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[WarehouseCapacityPatch] Registration failed: {ex.Message}");
                return false;
            }
        }

        private static IEnumerable<MethodInfo> FindCandidates(Func<string, bool> methodNameFilter)
        {
            foreach (var type in GetAssemblyCSharpTypes())
            {
                string fullName = type.FullName ?? type.Name;
                if (!HasAnyHint(fullName, TypeNameHints)) continue;

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!IsCapacityCandidate(method, methodNameFilter)) continue;
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

        private static bool IsCapacityCandidate(MethodInfo method, Func<string, bool> methodNameFilter)
        {
            if (method.IsStatic) return false;
            if (method.IsAbstract) return false;
            if (method.ContainsGenericParameters) return false;
            if (method.GetParameters().Length != 0) return false;
            if (method.ReturnType != typeof(int) && method.ReturnType != typeof(float)) return false;

            string name = method.Name;
            if (name.StartsWith("set_", StringComparison.Ordinal)) return false;
            return methodNameFilter(name);
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

        private static bool IsFreeCapacityMethodName(string name)
        {
            for (int i = 0; i < FreeCapacityMethodNames.Length; i++)
            {
                if (string.Equals(name, FreeCapacityMethodNames[i], StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static bool IsPackSizeMethodName(string name)
        {
            for (int i = 0; i < PackSizeMethodNames.Length; i++)
            {
                if (string.Equals(name, PackSizeMethodNames[i], StringComparison.Ordinal))
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

        private static IntPtr InstancePointer(object? instance)
        {
            return instance is Il2CppObjectBase obj ? obj.Pointer : IntPtr.Zero;
        }

        private static void IntCapacityPostfix(object __instance, ref int __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (__result <= 0) return;

            ApplyStoredPackSizeMultiplier(__instance, multiplier);
            if (multiplier <= 1) return;

            var pointer = InstancePointer(__instance);
            if (pointer != IntPtr.Zero) LastIntCapacityByInstance[pointer] = __result;

            long scaled = (long)__result * multiplier;
            __result = scaled > int.MaxValue ? int.MaxValue : (int)scaled;
        }

        private static void FloatCapacityPostfix(object __instance, ref float __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (__result <= 0f) return;

            ApplyStoredPackSizeMultiplier(__instance, multiplier);
            if (multiplier <= 1) return;

            var pointer = InstancePointer(__instance);
            if (pointer != IntPtr.Zero) LastFloatCapacityByInstance[pointer] = __result;

            __result *= multiplier;
        }

        private static void IntFreeCapacityPostfix(object __instance, ref int __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (multiplier <= 1 || __result < 0) return;

            var pointer = InstancePointer(__instance);
            if (pointer == IntPtr.Zero || !LastIntCapacityByInstance.TryGetValue(pointer, out int capacity) || capacity <= 0)
                return;

            long scaledCapacity = (long)capacity * multiplier;
            if (__result > scaledCapacity)
                __result = scaledCapacity > int.MaxValue ? int.MaxValue : (int)scaledCapacity;
        }

        private static void FloatFreeCapacityPostfix(object __instance, ref float __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (multiplier <= 1 || __result < 0f) return;

            var pointer = InstancePointer(__instance);
            if (pointer == IntPtr.Zero || !LastFloatCapacityByInstance.TryGetValue(pointer, out float capacity) || capacity <= 0f)
                return;

            float scaledCapacity = capacity * multiplier;
            if (__result > scaledCapacity) __result = scaledCapacity;
        }

        private static void IntPackSizePostfix(ref int __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (multiplier <= 1 || __result <= 0) return;

            long scaled = (long)__result * multiplier;
            __result = scaled > int.MaxValue ? int.MaxValue : (int)scaled;
        }

        private static void FloatPackSizePostfix(ref float __result)
        {
            int multiplier = GetWarehouseCapacityMultiplier();
            if (multiplier <= 1 || __result <= 0f) return;

            __result *= multiplier;
        }

        private static void ApplyStoredPackSizeMultiplier(object instance, int multiplier)
        {
            try
            {
                var pointer = InstancePointer(instance);
                if (pointer == IntPtr.Zero) return;

                var type = instance.GetType();
                var property = type.GetProperty("storedPackSize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property == null || !property.CanRead || !property.CanWrite)
                {
                    LogStoredPackSizeFailure(type.FullName ?? type.Name, "storedPackSize property with getter/setter not found");
                    return;
                }

                object? raw = property.GetValue(instance);
                if (!(raw is int current) || current <= 0) return;

                if (!OriginalStoredPackSizeByInstance.TryGetValue(pointer, out int original))
                {
                    original = current;
                    OriginalStoredPackSizeByInstance[pointer] = original;
                }

                long scaled = multiplier <= 1 ? original : (long)original * multiplier;
                int target = scaled > int.MaxValue ? int.MaxValue : (int)scaled;
                if (current != target) property.SetValue(instance, target);
            }
            catch (Exception ex)
            {
                LogStoredPackSizeFailure(instance.GetType().FullName ?? instance.GetType().Name, ex.Message);
            }
        }

        private static void LogStoredPackSizeFailure(string typeName, string message)
        {
            string key = typeName + "|" + message;
            if (LoggedStoredPackSizeFailures.Add(key))
            {
                MelonLogger.Warning($"[WarehouseCapacityPatch] storedPackSize update skipped for {typeName}: {message}");
            }
        }
    }
}
