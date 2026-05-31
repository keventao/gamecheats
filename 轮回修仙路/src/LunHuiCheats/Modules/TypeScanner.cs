using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Runtime type scanner: prints Il2CppInterop type names matching keywords.
    /// Run once after game loads; results guide reverse-engineering.
    /// </summary>
    public static class TypeScanner
    {
        private static readonly string[] Keywords = new[]
        {
            "Player", "Role", "Hero", "Character", "Actor",
            "Attribute", "Property", "Stat", "Status", "Data",
            "Skill", "Spell", "Ability", "Magic", "Fight",
            "Bag", "Inventory", "Pack", "Item", "Goods", "Equip",
            "Money", "Coin", "Gold", "Silver", "SpiritStone", "LingShi", "灵石",
            "Exp", "Experience", "Cultivation", "Practice", "修炼", "修为",
            "Life", "Hp", "Health", "Vitality", "Vigor", "生机", "生命",
            "Mana", "Mp", "MagicPoint", "灵力", "真元",
            "Atk", "Attack", "Def", "Defense", "Power", "Strength",
            "Speed", "Move", "Jump", "Run",
            "Root", "SpiritRoot", "LingGen", "灵根", "金", "木", "水", "火", "土",
            "Quality", "ZiZhi", "体质", "根骨", "悟性", "Tizhi", "Gengu", "WuXing",
            "Dao", "Heart", "Mind", "Karma", "YeLi", "道心", "业力", "寿元", "ShouYuan",
            "Break", "Breakthrough", "突破", "Success", "Rate", "成功率",
            "Dan", "Pill", "Drug", "Medicine", "Alchemy", "炼丹", "Refine", "Smith", "炼器",
            "Auction", "Bid", "拍卖",
            "Beast", "Pet", "Mount", "LingShou", "灵兽", "血脉",
            "Task", "Mission", "Quest", "Event",
            "Time", "Scale", "Speed",
            "Scene", "Map", "World", "Level",
            "Save", "Load", "Archive",
        };

        public static void ScanAndDump(ManualLogSource log)
        {
            try
            {
                log.LogInfo("[TypeScanner] Starting scan…");
                var found = new Dictionary<string, List<string>>();
                foreach (var kw in Keywords) found[kw] = new List<string>();

                int total = 0;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    try
                    {
                        var types = asm.GetTypes();
                        foreach (var t in types)
                        {
                            total++;
                            var name = t.FullName ?? t.Name;
                            foreach (var kw in Keywords)
                            {
                                if (name.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    found[kw].Add(name);
                                }
                            }
                        }
                    }
                    catch { /* some assemblies may fail */ }
                }

                /* no-op */

                // Dump to log
                log.LogInfo($"[TypeScanner] Scanned {total} types.");
                foreach (var kv in found.OrderBy(x => x.Key))
                {
                    if (kv.Value.Count == 0) continue;
                    var unique = kv.Value.Distinct().OrderBy(x => x).ToList();
                    log.LogInfo($"[TypeScanner] Keyword '{kv.Key}' => {unique.Count} matches:");
                    foreach (var n in unique.Take(20))
                        log.LogInfo($"  {n}");
                    if (unique.Count > 20)
                        log.LogInfo($"  … and {unique.Count - 20} more.");
                }

                // Dump to file
                var path = Path.Combine(
                    Path.GetDirectoryName(typeof(TypeScanner).Assembly.Location) ?? ".",
                    "lunhui-typescan.txt");
                using (var sw = new StreamWriter(path, false))
                {
                    sw.WriteLine($"# Type scan at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sw.WriteLine($"# Total types scanned: {total}");
                    sw.WriteLine();
                    foreach (var kv in found.OrderBy(x => x.Key))
                    {
                        if (kv.Value.Count == 0) continue;
                        sw.WriteLine($"## Keyword: {kv.Key} ({kv.Value.Distinct().Count()} matches)");
                        foreach (var n in kv.Value.Distinct().OrderBy(x => x))
                            sw.WriteLine(n);
                        sw.WriteLine();
                    }
                }
                log.LogInfo($"[TypeScanner] Results written to {path}");
            }
            catch (Exception ex)
            {
                log.LogError($"[TypeScanner] Exception: {ex}");
            }
        }
    }
}
