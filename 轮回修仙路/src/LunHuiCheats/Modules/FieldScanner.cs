using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime;

namespace LunHuiCheats.Modules
{
    public static class FieldScanner
    {
        private static readonly string[] TargetTypes = new[]
        {
            "UnitData",
            "CharacterData",
            "PlayerUnitData",
            "BaseData",
            "CharacterBaseAttributesData",
            "SpiritStoneData",
            "CoinData",
            "SkillData",
            "SkillStateMachine",
            "FakeInventoryData",
            "BackpackGoods",
            "BaseRewardData",
            "Cultivation",
            "Practice",
            "RoleUpgradeData",
            "ExperienceData",
            "LifeTime",
            "SpiritRoot",
            "Linggen",
            "DanYaoData",
            "PetData",
            "HeartAchievementMethod",
            "JindanData",
            "RefiningDanData",
        };

        public static void ScanAndDump(ManualLogSource log)
        {
            try
            {
                log.LogInfo("[FieldScanner] Starting field scan…");
                var path = Path.Combine(
                    Path.GetDirectoryName(typeof(FieldScanner).Assembly.Location) ?? ".",
                    "lunhui-fieldscan.txt");

                using (var sw = new StreamWriter(path, false))
                {
                    sw.WriteLine($"# Field scan at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sw.WriteLine();

                    foreach (var typeName in TargetTypes)
                    {
                        try
                        {
                            var t = AccessTools.TypeByName(typeName);
                            if (t == null)
                            {
                                sw.WriteLine($"## {typeName}: NOT FOUND");
                                sw.WriteLine();
                                log.LogWarning($"[FieldScanner] {typeName} not found.");
                                continue;
                            }

                            sw.WriteLine($"## {typeName} ({t.FullName})");
                            sw.WriteLine($"# Base: {t.BaseType?.Name}");
                            sw.WriteLine($"# Namespace: {t.Namespace}");

                            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .OrderBy(f => f.Name)
                                .ToArray();
                            if (fields.Length > 0)
                            {
                                sw.WriteLine("# Fields:");
                                foreach (var f in fields)
                                {
                                    var il2cppInfo = "";
                                    try
                                    {
                                        if (f.FieldType != null)
                                            il2cppInfo = f.FieldType.Name;
                                    }
                                    catch { }
                                    sw.WriteLine($"  {f.Name} : {il2cppInfo} [{f.Attributes}]");
                                }
                            }
                            else
                            {
                                sw.WriteLine("# Fields: (none found)");
                            }

                            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .OrderBy(p => p.Name)
                                .ToArray();
                            if (props.Length > 0)
                            {
                                sw.WriteLine("# Properties:");
                                foreach (var p in props)
                                {
                                    var getter = p.CanRead ? "get;" : "";
                                    var setter = p.CanWrite ? "set;" : "";
                                    sw.WriteLine($"  {p.Name} : {p.PropertyType?.Name} {{{getter}{setter}}}");
                                }
                            }

                            var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .Where(m => !m.IsSpecialName)
                                .OrderBy(m => m.Name)
                                .Take(30)
                                .ToArray();
                            if (methods.Length > 0)
                            {
                                sw.WriteLine("# Methods (top 30):");
                                foreach (var m in methods)
                                    sw.WriteLine($"  {m.Name}({string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType?.Name} {p.Name}"))})");
                            }

                            sw.WriteLine();
                            log.LogInfo($"[FieldScanner] {typeName} scanned: {fields.Length} fields, {props.Length} props.");
                        }
                        catch (Exception ex)
                        {
                            sw.WriteLine($"## {typeName}: ERROR - {ex.Message}");
                            sw.WriteLine();
                            log.LogError($"[FieldScanner] {typeName} scan error: {ex.Message}");
                        }
                    }
                }

                log.LogInfo($"[FieldScanner] Results written to {path}");
            }
            catch (Exception ex)
            {
                log.LogError($"[FieldScanner] Fatal: {ex}");
            }
        }
    }
}
