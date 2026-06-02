using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LunHuiCheats.Core
{
    public static class GameRefs
    {
        public static bool IsReady { get; private set; }

        private static object? _characterData;
        private static object? _inventoryData;
        private static bool _deepDumpDone;

        private static readonly BindingFlags WalkFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void SetReady(bool ready, UnityEngine.Object? seed = null)
        {
            IsReady = ready;
            if (!ready)
            {
                _characterData = null;
                _inventoryData = null;
                _lastResolveAttempt = -999f;
                _deepDumpDone = false;
                Plugin.Registry?.ResetGameReady();
            }
            Plugin.LogSrc?.LogInfo($"[GameRefs] IsReady = {ready}");
            if (ready)
            {
                if (seed != null)
                {
                    Plugin.LogSrc?.LogInfo($"[GameRefs] Phase1: deep recursive scan from seed {seed.GetType().Name}...");
                    DeepScanRecursive(seed, 0);
                }
                Plugin.Registry?.NotifyGameReady();
            }
        }

        public static object? CharacterData
        {
            get
            {
                EnsureResolved();
                return _characterData;
            }
        }

        public static object? UnitData
        {
            get
            {
                var c = CharacterData;
                return c != null && ReflectAccessor.TryGet(c, "unitData", out var u) ? u : null;
            }
        }

        public static object? Inventory
        {
            get
            {
                EnsureResolved();
                return _inventoryData;
            }
        }

        // Re-run the full multi-phase scan whenever a target is still missing. The old code
        // gated on a single `_resolved` flag that PassInventory/PassCharacterData set to true
        // on the FIRST capture — so once the inventory hook fired, the CharacterData fallback
        // scan (Phase2-5) never ran and CharacterData/UnitData stayed null forever. Throttled
        // so DrawGui/OnUpdate calling this per frame doesn't re-scan every frame.
        private static float _lastResolveAttempt = -999f;
        private static void EnsureResolved()
        {
            if (!IsReady) return;
            if (_characterData != null && _inventoryData != null) return;
            float now = Time.realtimeSinceStartup;
            if (now - _lastResolveAttempt < 1.5f) return;
            _lastResolveAttempt = now;
            ResolveAll(null);
        }

        /// <summary>Called by BootstrapHooks to capture live instances via Harmony patch.</summary>
        public static void PassInventory(object instance)
        {
            // Only accept this instance if it has data OR we don't have anything.
            // LoadData fires repeatedly — the first call may have real data, later
            // calls may be on empty temp instances.
            if (_inventoryData != null)
            {
                int oldCount = 0, newCount = 0;
                try
                {
                    if (ReflectAccessor.TryGet(_inventoryData, "AllDanYao", out var ol) && ol != null)
                    { var op = ol.GetType().GetProperty("Count"); if (op != null) oldCount = System.Convert.ToInt32(op.GetValue(ol)); }
                    if (ReflectAccessor.TryGet(instance, "AllDanYao", out var nl) && nl != null)
                    { var np = nl.GetType().GetProperty("Count"); if (np != null) newCount = System.Convert.ToInt32(np.GetValue(nl)); }
                }
                catch { }
                if (oldCount > 0 && newCount == 0)
                {
                    Plugin.LogSrc?.LogInfo($"[GameRefs] Skipping empty inventory capture (kept instance with {oldCount} items).");
                    return;
                }
            }
            _inventoryData = instance;
            int cnt = 0;
            try
            {
                if (ReflectAccessor.TryGet(instance, "AllDanYao", out var l) && l != null)
                { var p = l.GetType().GetProperty("Count"); if (p != null) cnt = System.Convert.ToInt32(p.GetValue(l)); }
            }
            catch { }
            Plugin.LogSrc?.LogInfo($"[GameRefs] Captured FakeInventoryData via hook. AllDanYao.Count={cnt}");
            if (_characterData == null)
                DeepScanRecursive(instance, 0);
        }

        public static void PassCharacterData(object instance)
        {
            // Capture-once: hooked on hot property getters (get_currentLevel/Exp/...) that the
            // HUD calls every frame. Keep the FIRST non-null instance — on world-enter that is the
            // player (HUD draws player level/exp first); later monster getters during combat must
            // not overwrite it. Also avoids per-frame log spam.
            if (_characterData != null || instance == null) return;
            _characterData = instance;
            Plugin.LogSrc?.LogInfo($"[GameRefs] Captured CharacterData via hook. type={instance.GetType().FullName}");
        }

        private static void ResolveAll(UnityEngine.Object? seed)
        {
            List<GameObject>? dumpRoots = null;
            try
            {
                if (seed != null)
                {
                    Plugin.LogSrc?.LogInfo($"[GameRefs] Phase1: deep recursive scan from seed {seed.GetType().Name}...");
                    DeepScanRecursive(seed, 0);
                }

                if (_characterData == null || _inventoryData == null)
                {
                    dumpRoots = FindRootObjects(seed);
                    Plugin.LogSrc?.LogInfo($"[GameRefs] Phase2: walking {dumpRoots.Count} root GOs...");
                    foreach (var r in dumpRoots)
                    {
                        WalkGoRecursive(r);
                        if (_characterData != null && _inventoryData != null) break;
                    }
                }

                if (_characterData == null || _inventoryData == null)
                {
                    Plugin.LogSrc?.LogInfo("[GameRefs] Phase3: trying known GameObject names...");
                    TryKnownObjects();
                }

                if (_characterData == null || _inventoryData == null)
                {
                    Plugin.LogSrc?.LogInfo("[GameRefs] Phase4: scanning static members...");
                    ScanStaticMembers();
                }

                // Phase5 is the heavyweight pass (walks every MonoBehaviour, rewrites
                // lunhui-deepdump.txt). EnsureResolved re-runs ResolveAll on a throttle while
                // targets are missing, so cap the deep dump to once per world to avoid
                // re-scanning/rewriting the file every retry.
                if ((_characterData == null || _inventoryData == null) && !_deepDumpDone)
                {
                    _deepDumpDone = true;
                    Plugin.LogSrc?.LogInfo("[GameRefs] Phase5: deep-property dump of all MonoBehaviours...");
                    DeepDumpAllMbs(dumpRoots);
                }

                Plugin.LogSrc?.LogInfo($"[GameRefs] Done: CharacterData={_characterData != null}, FakeInventoryData={_inventoryData != null}");
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] ResolveAll failed: {ex.Message}");
            }
        }

        private static List<GameObject> FindRootObjects(UnityEngine.Object? seed)
        {
            var roots = new List<GameObject>();

            // Try 1: Scene.GetRootGameObjects() via reflection.
            try
            {
                var scene = SceneManager.GetActiveScene();
                var mi = typeof(Scene).GetMethod("GetRootGameObjects", Type.EmptyTypes);
                if (mi != null)
                {
                    var raw = mi.Invoke(scene, null);
                    if (raw != null)
                    {
                        // IL2CPP returns Il2CppReferenceArray<GameObject>, not managed GameObject[].
                        // Iterate via IEnumerable to avoid cast issues.
                        if (raw is System.Collections.IEnumerable items)
                        {
                            foreach (var item in items)
                            {
                                var obj = item as GameObject;
                                if (obj != null) roots.Add(obj);
                            }
                            Plugin.LogSrc?.LogInfo($"[GameRefs] Scene.GetRootGameObjects() => {roots.Count} roots.");
                            return roots;
                        }
                    }
                }
                Plugin.LogSrc?.LogInfo("[GameRefs] GetRootGameObjects not available or returned empty.");
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] GetRootGameObjects failed: {ex.Message}");
            }

            // Try 2: Resources.FindObjectsOfTypeAll(typeof(GameObject)) via reflection.
            try
            {
                var resType = AccessTools.TypeByName("UnityEngine.Resources");
                if (resType != null)
                {
                    var mi = resType.GetMethod("FindObjectsOfTypeAll", new[] { typeof(Type) });
                    if (mi != null)
                    {
                        var raw = mi.Invoke(null, new object[] { typeof(GameObject) });
                        if (raw is System.Collections.IEnumerable items)
                        {
                            int count = 0;
                            foreach (var item in items)
                            {
                                count++;
                                var obj = item as GameObject;
                                if (obj != null && obj.transform.parent == null)
                                    roots.Add(obj);
                            }
                            Plugin.LogSrc?.LogInfo($"[GameRefs] FindObjectsOfTypeAll => {count} total, {roots.Count} roots.");
                            return roots;
                        }
                    }
                }
                Plugin.LogSrc?.LogInfo("[GameRefs] FindObjectsOfTypeAll not available.");
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] FindObjectsOfTypeAll failed: {ex.Message}");
            }

            // Try 3: transform.root from seed.
            if (seed != null)
            {
                try
                {
                    var gp = seed.GetType().GetProperty("gameObject", WalkFlags);
                    if (gp != null && gp.GetValue(seed) is GameObject go)
                    {
                        roots.Add(go.transform.root.gameObject);
                        Plugin.LogSrc?.LogInfo($"[GameRefs] Using transform.root '{go.transform.root.name}'.");
                    }
                }
                catch { }
            }
            return roots;
        }

        private static void TryKnownObjects()
        {
            var names = new[] { "Player", "Player(Clone)", "Main Camera", "GameMain", "GameManager", "SceneController" };
            foreach (var name in names)
            {
                try
                {
                    var go = GameObject.Find(name);
                    if (go == null) continue;
                    WalkGoRecursive(go);
                    if (_characterData != null && _inventoryData != null) return;
                }
                catch { }
            }
        }

        private static void WalkGoRecursive(GameObject go)
        {
            WalkGo(go);
        }

        /// <summary>
        /// Scan all MonoBehaviours on a GameObject and its children.
        /// In IL2CPP, GetComponents(typeof(MonoBehaviour)) may not be exposed,
        /// so we try GetComponentsInChildren(Type, bool) via reflection first,
        /// then fall back to recursive walk with GetComponent singular.
        /// </summary>
        private static void WalkGo(GameObject go)
        {
            if (_characterData != null && _inventoryData != null) return;
            if (go == null) return;

            // Try GetComponentsInChildren(Type, bool) — returns ALL components in subtree.
            try
            {
                var mi = go.GetType().GetMethod("GetComponentsInChildren", BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(Type), typeof(bool) }, null);
                if (mi != null)
                {
                    var raw = mi.Invoke(go, new object[] { typeof(MonoBehaviour), true });
                    if (raw is System.Collections.IEnumerable items)
                    {
                        foreach (var item in items)
                        {
                            var mb = item as MonoBehaviour;
                            if (mb == null) continue;
                            CheckMb(mb);
                            if (_characterData != null && _inventoryData != null) return;
                        }
                        return; // GetComponentsInChildren already covered the subtree.
                    }
                }
            }
            catch { }

            // Fallback: try GetComponentsInChildren with 1 param.
            try
            {
                var mi = go.GetType().GetMethod("GetComponentsInChildren", BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(Type) }, null);
                if (mi != null)
                {
                    var raw = mi.Invoke(go, new object[] { typeof(MonoBehaviour) });
                    if (raw is System.Collections.IEnumerable items)
                    {
                        foreach (var item in items)
                        {
                            var mb = item as MonoBehaviour;
                            if (mb == null) continue;
                            CheckMb(mb);
                            if (_characterData != null && _inventoryData != null) return;
                        }
                    }
                }
            }
            catch { }

            // Walk children recursively.
            try
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    WalkGo(go.transform.GetChild(i).gameObject);
                    if (_characterData != null && _inventoryData != null) return;
                }
            }
            catch { }
        }

        private static void CheckMb(MonoBehaviour mb)
        {
            if (_characterData == null)
                _characterData = ExtractRef(mb, "CharacterData");
            if (_inventoryData == null)
                _inventoryData = ExtractRef(mb, "FakeInventoryData");
        }

        /// <summary>
        /// Walk all non-null instance properties from a root object, up to 4 levels deep,
        /// looking for any value whose runtime type is CharacterData or FakeInventoryData.
        /// Catches cases where the target is nested inside a manager object.
        /// </summary>
        private static void DeepScanRecursive(object root, int depth)
        {
            if (depth > 4 || root == null) return;
            if (_characterData != null && _inventoryData != null) return;
            var t = root.GetType();
            // Check if this IS the target.
            if (_characterData == null && (t.Name == "CharacterData" || t.FullName == "CharacterData"))
                _characterData = root;
            if (_inventoryData == null && (t.Name == "FakeInventoryData" || t.FullName == "FakeInventoryData"))
                _inventoryData = root;
            // Walk properties.
            var seen = new HashSet<object>();
            foreach (var prop in t.GetProperties(WalkFlags))
            {
                if (_characterData != null && _inventoryData != null) return;
                try
                {
                    if (!prop.CanRead) continue;
                    var val = prop.GetValue(root);
                    if (val == null) continue;
                    var vt = val.GetType();
                    if (vt.Name == "CharacterData" || (vt.FullName != null && vt.FullName == "CharacterData"))
                        { _characterData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** CharacterData via Seed.{prop.Name} (depth {depth}) ***"); }
                    if (vt.Name == "FakeInventoryData" || (vt.FullName != null && vt.FullName == "FakeInventoryData"))
                        { _inventoryData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** FakeInventoryData via Seed.{prop.Name} (depth {depth}) ***"); }
                    // Recurse into the value if it's not a primitive.
                    if (!vt.IsPrimitive && !(val is string) && !vt.IsEnum && seen.Add(val))
                        DeepScanRecursive(val, depth + 1);
                }
                catch { }
            }
            foreach (var field in t.GetFields(WalkFlags))
            {
                if (_characterData != null && _inventoryData != null) return;
                try
                {
                    var val = field.GetValue(root);
                    if (val == null) continue;
                    var vt = val.GetType();
                    if (vt.Name == "CharacterData" || (vt.FullName != null && vt.FullName == "CharacterData"))
                        { _characterData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** CharacterData via Seed.{field.Name} (depth {depth}) ***"); }
                    if (vt.Name == "FakeInventoryData" || (vt.FullName != null && vt.FullName == "FakeInventoryData"))
                        { _inventoryData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** FakeInventoryData via Seed.{field.Name} (depth {depth}) ***"); }
                    if (!vt.IsPrimitive && !(val is string) && !vt.IsEnum && seen.Add(val))
                        DeepScanRecursive(val, depth + 1);
                }
                catch { }
            }
        }

        /// <summary>
        /// Scan static properties/fields across all loaded types for CharacterData
        /// and FakeInventoryData instances. Also tries the "Instance" singleton pattern.
        /// </summary>
        private static void ScanStaticMembers()
        {
            try
            {
                var charDataType = AccessTools.TypeByName("CharacterData");
                var invDataType = AccessTools.TypeByName("FakeInventoryData");
                var sflags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

                int scanned = 0;
                var asmNames = new List<string>();
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var name = asm.GetName().Name ?? "";
                    // Only scan game-related assemblies — skip framework, Unity, BepInEx, etc.
                    if (name.StartsWith("System") || name.StartsWith("mscorlib") ||
                        name.StartsWith("Unity") || name.StartsWith("Harmony") ||
                        name.StartsWith("Il2Cpp") || name.StartsWith("BepInEx") ||
                        name.StartsWith("Microsoft") || name.StartsWith("Newtonsoft") ||
                        name.StartsWith("DG.") || name.StartsWith("AsmResolver") ||
                        name.StartsWith("Iced") || name.Contains("Interop"))
                        continue;
                    asmNames.Add(name);
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch { continue; }
                    foreach (var t in types)
                    {
                        if (t == null) continue;
                        scanned++;
                        foreach (var prop in t.GetProperties(sflags))
                        {
                            try
                            {
                                if (!prop.CanRead) continue;
                                var pt = prop.PropertyType;
                                if (pt == charDataType || (charDataType != null && (pt.Name == "CharacterData" || pt.FullName == "CharacterData")))
                                {
                                    var val = prop.GetValue(null);
                                    if (val != null) { _characterData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** CharacterData via static {t.FullName}.{prop.Name} ***"); }
                                }
                                if (pt == invDataType || (invDataType != null && (pt.Name == "FakeInventoryData" || pt.FullName == "FakeInventoryData")))
                                {
                                    var val = prop.GetValue(null);
                                    if (val != null) { _inventoryData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** FakeInventoryData via static {t.FullName}.{prop.Name} ***"); }
                                }
                            }
                            catch { }
                        }
                        foreach (var field in t.GetFields(sflags))
                        {
                            try
                            {
                                var ft = field.FieldType;
                                if (ft == charDataType || (charDataType != null && (ft.Name == "CharacterData" || ft.FullName == "CharacterData")))
                                {
                                    var val = field.GetValue(null);
                                    if (val != null) { _characterData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** CharacterData via static {t.FullName}.{field.Name} ***"); }
                                }
                                if (ft == invDataType || (invDataType != null && (ft.Name == "FakeInventoryData" || ft.FullName == "FakeInventoryData")))
                                {
                                    var val = field.GetValue(null);
                                    if (val != null) { _inventoryData = val; Plugin.LogSrc?.LogInfo($"[GameRefs] *** FakeInventoryData via static {t.FullName}.{field.Name} ***"); }
                                }
                            }
                            catch { }
                        }
                        if (_characterData != null && _inventoryData != null) goto done;
                    }
                }
                done:
                Plugin.LogSrc?.LogInfo($"[GameRefs] Static scan: {scanned} types in {asmNames.Count} game assemblies: [{string.Join(",", asmNames)}].");
                if (_characterData == null)
                    Plugin.LogSrc?.LogInfo($"[GameRefs] CharacterData NOT found in any static member.");
                if (_inventoryData == null)
                    Plugin.LogSrc?.LogInfo($"[GameRefs] FakeInventoryData NOT found in any static member.");
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] ScanStaticMembers failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Deep scan: walk all MonoBehaviours and check every property's runtime
        /// value type for CharacterData/FakeInventoryData, even when stored as
        /// object or inside collections.
        /// </summary>
        private static void DeepDumpAllMbs(List<GameObject>? roots)
        {
            if (roots == null) return;
            var dumpPath = Path.Combine(
                Path.GetDirectoryName(typeof(CheatsRunner).Assembly.Location) ?? ".",
                "lunhui-deepdump.txt");
            try
            {
                using (var w = new System.IO.StreamWriter(dumpPath, false, System.Text.Encoding.UTF8))
                {
                    foreach (var root in roots)
                        DumpGoTree(root, w, "");
                }
                Plugin.LogSrc?.LogInfo($"[GameRefs] Deep dump written to {dumpPath}");
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] Deep dump failed: {ex.Message}");
            }

            // Also check run-time result
            if (_characterData == null && _inventoryData == null)
                Plugin.LogSrc?.LogInfo("[GameRefs] Deep dump: still not found after deep scan.");
        }

        private static void DumpGoTree(GameObject go, System.IO.StreamWriter w, string indent)
        {
            if (go == null) return;
            // Try GetComponentsInChildren(Type, bool) like WalkGo does.
            try
            {
                var mi = go.GetType().GetMethod("GetComponentsInChildren", BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(Type), typeof(bool) }, null);
                if (mi != null)
                {
                    var raw = mi.Invoke(go, new object[] { typeof(MonoBehaviour), true });
                    if (raw is System.Collections.IEnumerable mbs)
                    {
                        foreach (var item in mbs)
                        {
                            var mb = item as MonoBehaviour;
                            if (mb == null) continue;
                            var t = mb.GetType();
                            w.WriteLine($"{indent}[{t.FullName}]");
                            foreach (var prop in t.GetProperties(WalkFlags))
                            {
                                try
                                {
                                    if (!prop.CanRead) continue;
                                    var pt = prop.PropertyType;
                                    var v = prop.GetValue(mb);
                                    if (v == null) continue;
                                    var vt = v.GetType();
                                    w.WriteLine($"{indent}  .{prop.Name}  decl={pt.Name}  real={vt.FullName ?? vt.Name}");
                                    if (vt.Name == "CharacterData" || (vt.FullName != null && vt.FullName == "CharacterData"))
                                        { _characterData = v; w.WriteLine($"{indent}  *** FOUND CharacterData via {t.Name}.{prop.Name} ***"); }
                                    if (vt.Name == "FakeInventoryData" || (vt.FullName != null && vt.FullName == "FakeInventoryData"))
                                        { _inventoryData = v; w.WriteLine($"{indent}  *** FOUND FakeInventoryData via {t.Name}.{prop.Name} ***"); }
                                }
                                catch { }
                            }
                        }
                        return; // GetComponentsInChildren already covered subtree
                    }
                }
            }
            catch { }
        }

        private static object? ExtractRef(UnityEngine.Object root, string typeName)
        {
            var t = root.GetType();
            foreach (var prop in t.GetProperties(WalkFlags))
            {
                try
                {
                    if (prop.PropertyType.Name == typeName || prop.PropertyType.FullName == typeName)
                    {
                        var val = prop.GetValue(root);
                        if (val != null) return val;
                    }
                }
                catch { }
            }
            foreach (var field in t.GetFields(WalkFlags))
            {
                try
                {
                    if (field.FieldType.Name == typeName || field.FieldType.FullName == typeName)
                    {
                        var val = field.GetValue(root);
                        if (val != null) return val;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
