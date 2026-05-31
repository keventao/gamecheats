# 轮回修仙路 Cheat-Panel Port — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a categorized / sortable / searchable IMGUI cheat panel in LunHuiCheats plus GodMode, PlayerStats, Inventory, and Cultivation modules, driven by the verified game types in `refs/01-discovered-types-summary.md`.

**Architecture:** Keep the existing `Plugin → ModuleRegistry → GuiManager → CheatsRunner` skeleton. Layer 0 adds shared plumbing (`ReflectAccessor`, cached `GameRefs`, per-frame `OnUpdate`). Layer 1 reworks the panel into category-sidebar + top-bar (search/sort) using new Rect-IMGUI widgets and pure filter/sort logic. Layer 2 adds four cheat modules that read/write game objects by name via reflection.

**Tech Stack:** C# (net6.0 plugin / net8.0 tests), BepInEx 6 IL2CPP, HarmonyX, Il2CppInterop, UnityEngine IMGUI (`GUI.*`, no GUILayout), xUnit.

---

## Environment & Verification

- **Compile** works on any machine (Mac/Win): game access is name-based reflection, so the unresolved `Assembly-CSharp` HintPath is a warning, not an error. Verify with `dotnet build -c Release` from `src/LunHuiCheats`.
- **Unit tests** (`dotnet test` from `src/LunHuiCheats.Tests`) run on any machine once Task 0 adds `<RollForward>LatestMajor</RollForward>` (lets the net8.0 testhost run on a newer installed runtime).
- **In-game behavior** (anything touching `GameRefs.CharacterData`/`UnitData`/`Inventory`) can only be verified on the **Windows machine** with the game installed: `dotnet build -c Release` → `powershell tools/install.ps1` → launch → press `P`. These are the smoke-checklist items, not unit tests.

Each GUI/module task below ends with **build-verify** (compiles) + **smoke-verify** (manual, Windows). Pure-logic tasks use full TDD.

---

## File Structure

**Create:**
- `src/LunHuiCheats/Core/ReflectAccessor.cs` — name-based get/set over any instance, property-then-field, cached. Pure BCL.
- `src/LunHuiCheats/Core/FilterSort.cs` — pure filter+sort helper. No Unity.
- `src/LunHuiCheats/Core/ItemBrowserModel.cs` — `ItemRow` + browser state model. No Unity.
- `src/LunHuiCheats/Core/Gui/GuiWidgets.cs` — Rect-IMGUI widget helpers + `LayoutCursor`.
- `src/LunHuiCheats/Core/Gui/ScrollList.cs` — scroll-view helper.
- `src/LunHuiCheats/Core/Gui/ItemBrowserView.cs` — renders an `ItemBrowserModel`.
- `src/LunHuiCheats/Modules/GodMode.cs`
- `src/LunHuiCheats/Modules/PlayerStats.cs`
- `src/LunHuiCheats/Modules/Cultivation.cs`
- `src/LunHuiCheats/Modules/Inventory.cs`
- `src/LunHuiCheats.Tests/FilterSortTests.cs`
- `src/LunHuiCheats.Tests/ReflectAccessorTests.cs`
- `src/LunHuiCheats.Tests/ItemBrowserModelTests.cs`

**Modify:**
- `src/LunHuiCheats.Tests/LunHuiCheats.Tests.csproj` — add `RollForward`.
- `src/LunHuiCheats/Core/ICheatModule.cs` — add `Category`, `OnUpdate`.
- `src/LunHuiCheats/Core/ModuleRegistry.cs` — add `OnUpdateAll`, `Categories`.
- `src/LunHuiCheats/Core/CheatsRunner.cs` — call `OnUpdateAll` each frame.
- `src/LunHuiCheats/Core/GameRefs.cs` — cached `CharacterData`/`UnitData`/`Inventory`.
- `src/LunHuiCheats/Core/GuiManager.cs` — category sidebar + top bar.
- `src/LunHuiCheats/Modules/TimeCheats.cs` — add `Category`, `OnUpdate`.
- `src/LunHuiCheats/Modules/DebugDiagnostics.cs` — add `Category`, `OnUpdate`.
- `src/LunHuiCheats.Tests/ModuleRegistryTests.cs` — `TestModule` implements new members.
- `src/LunHuiCheats/Plugin.cs` — register the four new modules.
- `docs/smoke-checklist.md`, `ROADMAP.md` — document.

---

## Phase 0 — Prep

### Task 0: Make tests runnable on any installed runtime

**Files:** Modify `src/LunHuiCheats.Tests/LunHuiCheats.Tests.csproj`

- [ ] **Step 1: Add RollForward to the PropertyGroup**

Change the `<PropertyGroup>` to include:

```xml
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>LunHuiCheats.Tests</AssemblyName>
    <RootNamespace>LunHuiCheats.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
    <RollForward>LatestMajor</RollForward>
  </PropertyGroup>
```

- [ ] **Step 2: Verify existing tests still run**

Run: `cd src/LunHuiCheats.Tests && dotnet test`
Expected: PASS (the 2 existing ModuleRegistryTests), no "must install .NET 8" error.

- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats.Tests/LunHuiCheats.Tests.csproj"
git commit -m "test: roll-forward testhost to installed runtime"
```

---

## Phase 1 — Plumbing & Pure Logic (full TDD)

### Task 1: Extend ICheatModule with Category + OnUpdate

**Files:**
- Modify: `src/LunHuiCheats/Core/ICheatModule.cs`
- Modify: `src/LunHuiCheats/Modules/TimeCheats.cs`, `src/LunHuiCheats/Modules/DebugDiagnostics.cs`
- Modify: `src/LunHuiCheats.Tests/ModuleRegistryTests.cs`

- [ ] **Step 1: Update the interface**

Replace `ICheatModule.cs` body with:

```csharp
using HarmonyLib;

namespace LunHuiCheats.Core
{
    public interface ICheatModule
    {
        string Id       { get; }
        string Name     { get; }
        string Category { get; }
        ModuleStatus Status { get; }

        void Register(ModConfig cfg, Harmony harmony);
        void OnGameReady();
        void OnUpdate();
        void DrawGui();
        void DisableAll();
    }
}
```

- [ ] **Step 2: Add the members to TimeCheats**

In `TimeCheats.cs`, add next to `Name`:

```csharp
        public string Category => "通用";
```

and add an empty per-frame hook (TimeCheats applies on GUI change, not per frame):

```csharp
        public void OnUpdate() { }
```

- [ ] **Step 3: Add the members to DebugDiagnostics**

In `DebugDiagnostics.cs`, add `public string Category => "调试";` next to its `Name`, and `public void OnUpdate() { }` next to its other methods.

- [ ] **Step 4: Update the test double**

In `ModuleRegistryTests.cs`, add to the private `TestModule`:

```csharp
            public string Category => "测试";
            public void OnUpdate() { }
```

- [ ] **Step 5: Build + test**

Run: `cd src/LunHuiCheats && dotnet build -c Release` → Expected: Build succeeded.
Run: `cd ../LunHuiCheats.Tests && dotnet test` → Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/ICheatModule.cs" "轮回修仙路/src/LunHuiCheats/Modules/TimeCheats.cs" "轮回修仙路/src/LunHuiCheats/Modules/DebugDiagnostics.cs" "轮回修仙路/src/LunHuiCheats.Tests/ModuleRegistryTests.cs"
git commit -m "feat: add Category and OnUpdate to ICheatModule"
```

### Task 2: ReflectAccessor (TDD)

**Files:**
- Create: `src/LunHuiCheats/Core/ReflectAccessor.cs`
- Test: `src/LunHuiCheats.Tests/ReflectAccessorTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using LunHuiCheats.Core;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class ReflectAccessorTests
    {
        private class Dummy
        {
            public long Hp;                       // field
            public int Level { get; set; }        // property
            public float Speed { get; set; }
            private long Secret = 7;
            public long GetSecret() => Secret;
        }

        [Fact]
        public void Get_Field_And_Property()
        {
            var d = new Dummy { Hp = 100, Level = 5 };
            Assert.True(ReflectAccessor.TryGet(d, "Hp", out var hp));
            Assert.Equal(100L, hp);
            Assert.True(ReflectAccessor.TryGet(d, "Level", out var lv));
            Assert.Equal(5, lv);
        }

        [Fact]
        public void Set_Coerces_Int64_To_Int32()
        {
            var d = new Dummy();
            ReflectAccessor.SetInt64(d, "Level", 42L);   // Level is int
            Assert.Equal(42, d.Level);
        }

        [Fact]
        public void Set_Coerces_To_Single()
        {
            var d = new Dummy();
            ReflectAccessor.SetSingle(d, "Speed", 3.5f);
            Assert.Equal(3.5f, d.Speed);
        }

        [Fact]
        public void Missing_Member_Returns_False_And_Fallback()
        {
            var d = new Dummy();
            Assert.False(ReflectAccessor.TryGet(d, "Nope", out _));
            Assert.Equal(-1L, ReflectAccessor.GetInt64(d, "Nope", -1));
        }

        [Fact]
        public void Reads_Private_Field()
        {
            var d = new Dummy();
            Assert.Equal(7L, ReflectAccessor.GetInt64(d, "Secret"));
        }
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `cd src/LunHuiCheats.Tests && dotnet test --filter ReflectAccessorTests`
Expected: FAIL (ReflectAccessor does not exist).

- [ ] **Step 3: Implement ReflectAccessor**

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LunHuiCheats.Core
{
    /// <summary>
    /// Name-based reflection get/set over arbitrary instances. IL2CPP wrapper
    /// objects expose il2cpp fields as managed PROPERTIES, so we try property
    /// first, then field. Member lookups are cached per (Type, name). Pure BCL.
    /// </summary>
    public static class ReflectAccessor
    {
        private const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly Dictionary<(Type, string), PropertyInfo?> _props = new();
        private static readonly Dictionary<(Type, string), FieldInfo?> _fields = new();

        public static bool TryGet(object? instance, string member, out object? value)
        {
            value = null;
            if (instance == null) return false;
            var t = instance.GetType();
            var p = GetProp(t, member);
            if (p != null && p.CanRead) { value = p.GetValue(instance); return true; }
            var f = GetField(t, member);
            if (f != null) { value = f.GetValue(instance); return true; }
            return false;
        }

        public static bool TrySet(object? instance, string member, object? value)
        {
            if (instance == null) return false;
            var t = instance.GetType();
            var p = GetProp(t, member);
            if (p != null && p.CanWrite) { p.SetValue(instance, Coerce(value, p.PropertyType)); return true; }
            var f = GetField(t, member);
            if (f != null) { f.SetValue(instance, Coerce(value, f.FieldType)); return true; }
            return false;
        }

        public static long GetInt64(object? instance, string member, long fallback = 0)
            => TryGet(instance, member, out var v) && v != null ? Convert.ToInt64(v) : fallback;

        public static void SetInt64(object? instance, string member, long value)
            => TrySet(instance, member, value);

        public static float GetSingle(object? instance, string member, float fallback = 0)
            => TryGet(instance, member, out var v) && v != null ? Convert.ToSingle(v) : fallback;

        public static void SetSingle(object? instance, string member, float value)
            => TrySet(instance, member, value);

        public static int GetInt32(object? instance, string member, int fallback = 0)
            => TryGet(instance, member, out var v) && v != null ? Convert.ToInt32(v) : fallback;

        public static void SetInt32(object? instance, string member, int value)
            => TrySet(instance, member, value);

        private static object? Coerce(object? value, Type target)
        {
            if (value == null) return null;
            if (target.IsInstanceOfType(value)) return value;
            try { if (value is IConvertible) return Convert.ChangeType(value, Nullable.GetUnderlyingType(target) ?? target); }
            catch { /* leave as-is; SetValue may still accept */ }
            return value;
        }

        private static PropertyInfo? GetProp(Type t, string name)
        {
            var key = (t, name);
            if (!_props.TryGetValue(key, out var p)) { p = t.GetProperty(name, Flags); _props[key] = p; }
            return p;
        }

        private static FieldInfo? GetField(Type t, string name)
        {
            var key = (t, name);
            if (!_fields.TryGetValue(key, out var f)) { f = t.GetField(name, Flags); _fields[key] = f; }
            return f;
        }
    }
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `cd src/LunHuiCheats.Tests && dotnet test --filter ReflectAccessorTests`
Expected: PASS (5 tests).

- [ ] **Step 5: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/ReflectAccessor.cs" "轮回修仙路/src/LunHuiCheats.Tests/ReflectAccessorTests.cs"
git commit -m "feat: add ReflectAccessor name-based reflection helper"
```

### Task 3: FilterSort (TDD)

**Files:**
- Create: `src/LunHuiCheats/Core/FilterSort.cs`
- Test: `src/LunHuiCheats.Tests/FilterSortTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Collections.Generic;
using System.Linq;
using LunHuiCheats.Core;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class FilterSortTests
    {
        private record Row(string Name, string Cat, int Recent);

        private static List<Row> Sample() => new()
        {
            new("丹药A", "丹药", 1),
            new("装备B", "装备", 3),
            new("丹药C", "丹药", 2),
        };

        private static List<Row> Run(string q, SortKey k) =>
            FilterSort.Apply(Sample(), q, k, r => r.Name, r => r.Cat, r => r.Recent);

        [Fact]
        public void Empty_Query_Returns_All_Sorted_By_Name()
        {
            var r = Run("", SortKey.Name);
            Assert.Equal(new[] { "丹药A", "丹药C", "装备B" }, r.Select(x => x.Name).ToArray());
        }

        [Fact]
        public void Query_Filters_By_Substring()
        {
            var r = Run("丹药", SortKey.Name);
            Assert.Equal(2, r.Count);
            Assert.All(r, x => Assert.Contains("丹药", x.Name));
        }

        [Fact]
        public void Sort_By_Recent_Descends()
        {
            var r = Run("", SortKey.Recent);
            Assert.Equal(new[] { "装备B", "丹药C", "丹药A" }, r.Select(x => x.Name).ToArray());
        }

        [Fact]
        public void Sort_By_Category_Then_Name()
        {
            var r = Run("", SortKey.Category);
            Assert.Equal(new[] { "丹药A", "丹药C", "装备B" }, r.Select(x => x.Name).ToArray());
        }

        [Fact]
        public void Null_Items_Yields_Empty()
        {
            var r = FilterSort.Apply<Row>(null!, "", SortKey.Name, x => x.Name, x => x.Cat, x => x.Recent);
            Assert.Empty(r);
        }
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `cd src/LunHuiCheats.Tests && dotnet test --filter FilterSortTests`
Expected: FAIL (FilterSort / SortKey not defined).

- [ ] **Step 3: Implement FilterSort**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunHuiCheats.Core
{
    public enum SortKey { Name, Category, Recent }

    public static class FilterSort
    {
        public static List<T> Apply<T>(
            IEnumerable<T> items, string query, SortKey key,
            Func<T, string> nameOf, Func<T, string> catOf, Func<T, int> recentOf)
        {
            IEnumerable<T> seq = items ?? Enumerable.Empty<T>();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim();
                seq = seq.Where(x => (nameOf(x) ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            seq = key switch
            {
                SortKey.Category => seq.OrderBy(catOf, StringComparer.Ordinal).ThenBy(nameOf, StringComparer.Ordinal),
                SortKey.Recent   => seq.OrderByDescending(recentOf).ThenBy(nameOf, StringComparer.Ordinal),
                _                => seq.OrderBy(nameOf, StringComparer.Ordinal),
            };

            return seq.ToList();
        }
    }
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `cd src/LunHuiCheats.Tests && dotnet test --filter FilterSortTests`
Expected: PASS (5 tests).

- [ ] **Step 5: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/FilterSort.cs" "轮回修仙路/src/LunHuiCheats.Tests/FilterSortTests.cs"
git commit -m "feat: add FilterSort pure filter/sort helper"
```

### Task 4: ItemBrowserModel (TDD)

**Files:**
- Create: `src/LunHuiCheats/Core/ItemBrowserModel.cs`
- Test: `src/LunHuiCheats.Tests/ItemBrowserModelTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Linq;
using LunHuiCheats.Core;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class ItemBrowserModelTests
    {
        private static ItemBrowserModel Built()
        {
            var m = new ItemBrowserModel();
            m.SetRows(new[]
            {
                new ItemRow("回血丹", "丹药", new object()),
                new ItemRow("铁剑",   "装备", new object()),
                new ItemRow("聚气丹", "丹药", new object()),
            });
            return m;
        }

        [Fact]
        public void Categories_Lead_With_All_Then_Distinct()
        {
            var m = Built();
            Assert.Equal(new[] { "全部", "丹药", "装备" }, m.Categories().ToArray());
        }

        [Fact]
        public void Visible_All_Category_Sorted_By_Name()
        {
            var m = Built();
            Assert.Equal(new[] { "回血丹", "聚气丹", "铁剑" }, m.Visible().Select(r => r.Name).ToArray());
        }

        [Fact]
        public void Visible_Filtered_By_Category()
        {
            var m = Built();
            m.SelectedCategory = "丹药";
            Assert.Equal(2, m.Visible().Count);
            Assert.All(m.Visible(), r => Assert.Equal("丹药", r.Category));
        }

        [Fact]
        public void Visible_Filtered_By_Query()
        {
            var m = Built();
            m.Query = "聚气";
            Assert.Single(m.Visible());
            Assert.Equal("聚气丹", m.Visible()[0].Name);
        }
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `cd src/LunHuiCheats.Tests && dotnet test --filter ItemBrowserModelTests`
Expected: FAIL (ItemRow / ItemBrowserModel not defined).

- [ ] **Step 3: Implement ItemBrowserModel**

```csharp
using System.Collections.Generic;
using System.Linq;

namespace LunHuiCheats.Core
{
    public sealed class ItemRow
    {
        public string Name { get; }
        public string Category { get; }
        public object Payload { get; }   // game BaseRewardData / CoinData reference

        public ItemRow(string name, string category, object payload)
        {
            Name = name;
            Category = category;
            Payload = payload;
        }
    }

    public sealed class ItemBrowserModel
    {
        public const string AllCategories = "全部";

        private readonly List<ItemRow> _all = new();

        public string Query = "";
        public string SelectedCategory = AllCategories;
        public SortKey Sort = SortKey.Name;

        public void SetRows(IEnumerable<ItemRow> rows)
        {
            _all.Clear();
            if (rows != null) _all.AddRange(rows);
        }

        public IReadOnlyList<string> Categories()
        {
            var cats = _all.Select(r => r.Category).Distinct().ToList();
            cats.Insert(0, AllCategories);
            return cats;
        }

        public IReadOnlyList<ItemRow> Visible()
        {
            IEnumerable<ItemRow> seq = _all;
            if (SelectedCategory != AllCategories)
                seq = seq.Where(r => r.Category == SelectedCategory);
            return FilterSort.Apply(seq, Query, Sort, r => r.Name, r => r.Category, _ => 0);
        }
    }
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `cd src/LunHuiCheats.Tests && dotnet test --filter ItemBrowserModelTests`
Expected: PASS (4 tests).

- [ ] **Step 5: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/ItemBrowserModel.cs" "轮回修仙路/src/LunHuiCheats.Tests/ItemBrowserModelTests.cs"
git commit -m "feat: add ItemBrowserModel"
```

### Task 5: ModuleRegistry.OnUpdateAll + Categories

**Files:**
- Modify: `src/LunHuiCheats/Core/ModuleRegistry.cs`
- Test: extend `src/LunHuiCheats.Tests/ModuleRegistryTests.cs`

- [ ] **Step 1: Write the failing test**

Add to `ModuleRegistryTests`:

```csharp
        [Fact]
        public void OnUpdateAll_Calls_Each_Module()
        {
            var registry = new Core.ModuleRegistry();
            var m = new CountingModule();
            registry.Add(m);
            registry.OnUpdateAll();
            registry.OnUpdateAll();
            Assert.Equal(2, m.Updates);
        }

        private class CountingModule : Core.ICheatModule
        {
            public int Updates;
            public string Id => "count";
            public string Name => "Count";
            public string Category => "测试";
            public Core.ModuleStatus Status => Core.ModuleStatus.Ok;
            public void Register(Core.ModConfig cfg, HarmonyLib.Harmony harmony) { }
            public void OnGameReady() { }
            public void OnUpdate() => Updates++;
            public void DrawGui() { }
            public void DisableAll() { }
        }
```

- [ ] **Step 2: Run to verify it fails**

Run: `cd src/LunHuiCheats.Tests && dotnet test --filter ModuleRegistryTests`
Expected: FAIL (OnUpdateAll not defined).

- [ ] **Step 3: Implement OnUpdateAll + Categories**

Add to `ModuleRegistry`:

```csharp
        public void OnUpdateAll()
        {
            foreach (var m in _modules)
            {
                try { m.OnUpdate(); }
                catch { /* a faulty module must not break the frame loop */ }
            }
        }

        public IReadOnlyList<string> Categories()
        {
            var seen = new List<string>();
            foreach (var m in _modules)
                if (!seen.Contains(m.Category)) seen.Add(m.Category);
            return seen;
        }
```

(Add `using System.Collections.Generic;` if not present — it already is.)

- [ ] **Step 4: Run to verify it passes**

Run: `cd src/LunHuiCheats.Tests && dotnet test`
Expected: PASS (all tests).

- [ ] **Step 5: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/ModuleRegistry.cs" "轮回修仙路/src/LunHuiCheats.Tests/ModuleRegistryTests.cs"
git commit -m "feat: add ModuleRegistry.OnUpdateAll and Categories"
```

### Task 6: GameRefs cached lookups + CheatsRunner per-frame dispatch

**Files:**
- Modify: `src/LunHuiCheats/Core/GameRefs.cs`
- Modify: `src/LunHuiCheats/Core/CheatsRunner.cs`

No unit test (touches Unity `FindObjectOfType` / `Time.frameCount`). Verify by build + in-game smoke.

- [ ] **Step 1: Add cached lookups to GameRefs**

Replace `GameRefs.cs` with:

```csharp
using System;
using HarmonyLib;
using UnityEngine;

namespace LunHuiCheats.Core
{
    /// <summary>
    /// Tracks whether the game world is loaded and exposes cached, name-based
    /// lookups of the player's data objects. All game types are resolved by name
    /// through AccessTools to avoid hard IL2CPP dependencies.
    /// </summary>
    public static class GameRefs
    {
        public static bool IsReady { get; private set; }

        private static UnityEngine.Object? _characterData;
        private static int _lastResolveFrame = -1000;

        public static void SetReady(bool ready)
        {
            IsReady = ready;
            if (!ready) _characterData = null;
            Plugin.LogSrc?.LogInfo($"[GameRefs] IsReady = {ready}");
            if (ready) Plugin.Registry?.NotifyGameReady();
        }

        /// <summary>The player's CharacterData component (cached; throttled re-resolve).</summary>
        public static object? CharacterData
        {
            get
            {
                if (_characterData != null) return _characterData;
                if (Time.frameCount - _lastResolveFrame < 30) return null; // throttle FindObjectOfType
                _lastResolveFrame = Time.frameCount;
                _characterData = FindByTypeObj("CharacterData");
                return _characterData;
            }
        }

        /// <summary>CharacterData.unitData (DataLib.UnitData) — battle/base stats.</summary>
        public static object? UnitData
        {
            get
            {
                var c = CharacterData;
                return c != null && ReflectAccessor.TryGet(c, "unitData", out var u) ? u : null;
            }
        }

        /// <summary>FakeInventoryData instance, if it is a UnityEngine.Object in the scene.</summary>
        public static object? Inventory => FindByTypeObj("FakeInventoryData");

        public static T? FindByType<T>(string typeName) where T : UnityEngine.Object
            => FindByTypeObj(typeName) as T;

        public static UnityEngine.Object? FindByTypeObj(string typeName)
        {
            try
            {
                var t = AccessTools.TypeByName(typeName);
                if (t == null) return null;
                return UnityEngine.Object.FindObjectOfType(t);
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] FindObjectOfType<{typeName}> failed: {ex.Message}");
                return null;
            }
        }
    }
}
```

- [ ] **Step 2: Dispatch OnUpdate each frame in CheatsRunner**

In `CheatsRunner.Update()`, replace the line `_gui?.HandleInput();` with:

```csharp
            _gui?.HandleInput();

            if (GameRefs.IsReady && Plugin.Cfg != null && !Plugin.Cfg.GlobalDisableAll.Value)
                Plugin.Registry?.OnUpdateAll();
```

- [ ] **Step 3: Build-verify**

Run: `cd src/LunHuiCheats && dotnet build -c Release`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/GameRefs.cs" "轮回修仙路/src/LunHuiCheats/Core/CheatsRunner.cs"
git commit -m "feat: cache player refs in GameRefs and dispatch OnUpdate per frame"
```

---

## Phase 2 — UI Shell Views (build-verify + smoke-verify; no unit tests)

### Task 7: GuiWidgets + LayoutCursor

**Files:** Create `src/LunHuiCheats/Core/Gui/GuiWidgets.cs`

- [ ] **Step 1: Implement**

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace LunHuiCheats.Core.Gui
{
    /// <summary>A top-down layout cursor for Rect-based IMGUI (no GUILayout).</summary>
    public struct LayoutCursor
    {
        public float X, Y, Width, LineHeight, Pad;

        public LayoutCursor(float x, float y, float width, float lineHeight = 24f, float pad = 4f)
        { X = x; Y = y; Width = width; LineHeight = lineHeight; Pad = pad; }

        public Rect Line(float? height = null)
        {
            var h = height ?? LineHeight;
            var r = new Rect(X, Y, Width, h);
            Y += h + Pad;
            return r;
        }

        public Rect Slice(ref Rect line, float w, float gap = 4f)
        {
            var r = new Rect(line.x, line.y, w, line.height);
            line.x += w + gap;
            line.width -= w + gap;
            return r;
        }
    }

    /// <summary>Rect-based widget helpers. Number fields keep per-control text buffers.</summary>
    public static class GuiWidgets
    {
        private static readonly Dictionary<string, string> _buffers = new();

        public static void Label(Rect r, string text) => GUI.Label(r, text);

        public static bool Button(Rect r, string text) => GUI.Button(r, text);

        public static bool Toggle(Rect r, bool value, string label)
            => GUI.Toggle(r, value, " " + label);

        public static float Slider(Rect r, float value, float min, float max)
            => GUI.HorizontalSlider(r, value, min, max);

        /// <summary>
        /// Editable Int64 field. `id` must be unique per logical field so the text
        /// buffer survives between frames while the user types.
        /// </summary>
        public static long Int64Field(Rect r, string id, long value)
        {
            if (!_buffers.TryGetValue(id, out var buf) || !GUI.GetNameOfFocusedControl().Equals(id))
                buf = value.ToString();

            GUI.SetNextControlName(id);
            var text = GUI.TextField(r, buf);
            _buffers[id] = text;

            return long.TryParse(text, out var parsed) ? parsed : value;
        }
    }
}
```

- [ ] **Step 2: Build-verify**

Run: `cd src/LunHuiCheats && dotnet build -c Release`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/Gui/GuiWidgets.cs"
git commit -m "feat: add Rect-IMGUI GuiWidgets + LayoutCursor"
```

### Task 8: ScrollList

**Files:** Create `src/LunHuiCheats/Core/Gui/ScrollList.cs`

- [ ] **Step 1: Implement**

```csharp
using System;
using UnityEngine;

namespace LunHuiCheats.Core.Gui
{
    /// <summary>
    /// Wraps GUI.BeginScrollView for a vertical list of fixed-height rows.
    /// Holds its own scroll position; one instance per scrolling region.
    /// </summary>
    public sealed class ScrollList
    {
        private Vector2 _scroll;

        /// <param name="viewport">on-screen rect</param>
        /// <param name="rowCount">number of rows</param>
        /// <param name="rowHeight">height per row</param>
        /// <param name="drawRow">callback (index, rowRect) drawn in content space</param>
        public void Draw(Rect viewport, int rowCount, float rowHeight, Action<int, Rect> drawRow)
        {
            var contentHeight = Mathf.Max(viewport.height, rowCount * rowHeight);
            var content = new Rect(0, 0, viewport.width - 16, contentHeight);
            _scroll = GUI.BeginScrollView(viewport, _scroll, content);
            for (int i = 0; i < rowCount; i++)
                drawRow(i, new Rect(0, i * rowHeight, content.width, rowHeight - 2));
            GUI.EndScrollView();
        }
    }
}
```

- [ ] **Step 2: Build-verify**

Run: `cd src/LunHuiCheats && dotnet build -c Release` → Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/Gui/ScrollList.cs"
git commit -m "feat: add ScrollList scroll-view helper"
```

### Task 9: ItemBrowserView

**Files:** Create `src/LunHuiCheats/Core/Gui/ItemBrowserView.cs`

- [ ] **Step 1: Implement**

```csharp
using System;
using UnityEngine;

namespace LunHuiCheats.Core.Gui
{
    /// <summary>
    /// Renders an ItemBrowserModel: category buttons, a search box, a sort toggle,
    /// and a scrollable list of rows each with a quantity field + Add button.
    /// </summary>
    public sealed class ItemBrowserView
    {
        private readonly ScrollList _list = new();
        private long _qty = 1;

        public void Draw(Rect area, ItemBrowserModel model, Action<ItemRow, long> onAdd)
        {
            var c = new LayoutCursor(area.x, area.y, area.width);

            // Category row
            var catLine = c.Line();
            foreach (var cat in model.Categories())
            {
                var btn = c.Slice(ref catLine, 72f);
                var prev = GUI.color;
                if (model.SelectedCategory == cat) GUI.color = new Color(0.3f, 0.6f, 1f);
                if (GUI.Button(btn, cat)) model.SelectedCategory = cat;
                GUI.color = prev;
                if (catLine.width < 72f) break;
            }

            // Search + sort + qty
            var ctl = c.Line();
            GUI.Label(c.Slice(ref ctl, 40f), "搜索");
            model.Query = GUI.TextField(c.Slice(ref ctl, 140f), model.Query ?? "");
            if (GUI.Button(c.Slice(ref ctl, 80f), $"排序:{model.Sort}"))
                model.Sort = (SortKey)(((int)model.Sort + 1) % 3);
            GUI.Label(c.Slice(ref ctl, 40f), "数量");
            _qty = GuiWidgets.Int64Field(c.Slice(ref ctl, 80f), "itembrowser.qty", _qty);

            // List
            var rows = model.Visible();
            var listTop = c.Line();
            var viewport = new Rect(area.x, listTop.y, area.width, area.yMax - listTop.y);
            _list.Draw(viewport, rows.Count, 26f, (i, rowRect) =>
            {
                var row = rows[i];
                var nameRect = new Rect(rowRect.x, rowRect.y, rowRect.width - 70, rowRect.height);
                var addRect  = new Rect(rowRect.xMax - 64, rowRect.y, 60, rowRect.height);
                GUI.Label(nameRect, $"{row.Name}  ({row.Category})");
                if (GUI.Button(addRect, "Add")) onAdd(row, _qty);
            });
        }
    }
}
```

- [ ] **Step 2: Build-verify**

Run: `cd src/LunHuiCheats && dotnet build -c Release` → Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/Gui/ItemBrowserView.cs"
git commit -m "feat: add ItemBrowserView"
```

### Task 10: Rework GuiManager — category sidebar + top bar

**Files:** Modify `src/LunHuiCheats/Core/GuiManager.cs`

Replace the tab logic in `DrawPanel()` (the block from `// Tabs` through `GUI.EndGroup();`) with a category sidebar + search/sort top bar. Keep everything above `// Tabs` (background, title, Disable-All, ready/empty checks) unchanged.

- [ ] **Step 1: Add fields** near the existing private fields (`_open`, `_activeTab`, ...):

```csharp
        private string _selectedCategory = "";
        private string _search = "";
        private SortKey _sort = SortKey.Name;
        private Vector2 _moduleScroll;
```

- [ ] **Step 2: Replace the tab+content block** (everything from the `// Tabs` comment down to and including `GUI.EndGroup();`) with:

```csharp
            // Top bar: search + sort
            var barRect = new Rect(x, y, w, 24);
            GUI.Label(new Rect(barRect.x, barRect.y, 40, 24), "搜索");
            _search = GUI.TextField(new Rect(barRect.x + 42, barRect.y, 160, 24), _search ?? "");
            if (GUI.Button(new Rect(barRect.x + 210, barRect.y, 90, 24), $"排序:{_sort}"))
                _sort = (SortKey)(((int)_sort + 1) % 3);
            y += 28;

            // Category sidebar (left) + module content (right)
            var categories = _registry.Categories();
            if (_selectedCategory == "" && categories.Count > 0) _selectedCategory = categories[0];

            float sidebarW = 96f;
            float contentX = x + sidebarW + 6;
            float contentW = w - sidebarW - 6;
            float areaH = _panelRect.height - (y - _panelRect.y) - 8;

            for (int i = 0; i < categories.Count; i++)
            {
                var cat = categories[i];
                var rc = new Rect(x, y + i * 26, sidebarW, 24);
                var prev = GUI.color;
                if (cat == _selectedCategory) GUI.color = new Color(0.3f, 0.6f, 1f);
                if (GUI.Button(rc, cat)) _selectedCategory = cat;
                GUI.color = prev;
            }

            // Modules in the selected category, filtered + sorted by name
            var inCat = new System.Collections.Generic.List<ICheatModule>();
            foreach (var m in _registry.Modules)
                if (m.Category == _selectedCategory) inCat.Add(m);
            var shown = FilterSort.Apply(inCat, _search, _sort, m => m.Name, m => m.Category, _ => 0);

            var contentRect = new Rect(contentX, y, contentW, areaH);
            GUI.BeginGroup(contentRect);
            float my = 0;
            foreach (var module in shown)
            {
                var marker = module.Status switch
                {
                    ModuleStatus.Broken   => " (!)",
                    ModuleStatus.Disabled => " (off)",
                    _ => "",
                };
                GUI.Label(new Rect(0, my, contentW, 20), $"▼ {module.Name}{marker}");
                my += 22;
                if (module.Status == ModuleStatus.Broken)
                {
                    GUI.Label(new Rect(8, my, contentW - 8, 20), "patches failed — see LogOutput.log");
                    my += 22;
                }
                else
                {
                    // Each module draws into its own sub-group starting at (8, my).
                    var sub = new Rect(8, my, contentW - 12, areaH - my);
                    if (sub.height < 40) break;
                    GUI.BeginGroup(sub);
                    module.DrawGui();
                    GUI.EndGroup();
                    my += EstimateModuleHeight(module);
                }
                my += 8;
            }
            GUI.EndGroup();
```

- [ ] **Step 3: Add a height estimator** as a private method on GuiManager (modules are simple; a fixed budget per module is enough for v1):

```csharp
        private static float EstimateModuleHeight(ICheatModule module) => 180f;
```

- [ ] **Step 4: Add `using LunHuiCheats.Core.Gui;`** is NOT needed (GuiManager is already in `Core`; FilterSort/SortKey are in `Core`). Ensure `using UnityEngine;` is present (it is).

- [ ] **Step 5: Build-verify**

Run: `cd src/LunHuiCheats && dotnet build -c Release`
Expected: Build succeeded.

- [ ] **Step 6: Smoke-verify (Windows)** — `tools/install.ps1`, launch, press `P`: panel shows a search box, sort button, category sidebar (通用/调试 at least), and TimeCheats under 通用. Drag works.

- [ ] **Step 7: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Core/GuiManager.cs"
git commit -m "feat: category sidebar + search/sort panel in GuiManager"
```

---

## Phase 3 — Cheat Modules (build-verify + smoke-verify)

> Pattern for every module: resolve key game type in `Register` (set `Status=Broken` if missing); cache nothing game-side beyond `GameRefs`; `OnUpdate` applies per-frame locks; `DisableAll` restores; `DrawGui` uses `GuiWidgets`/`ItemBrowserView`.

### Task 11: GodMode module

**Files:** Create `src/LunHuiCheats/Modules/GodMode.cs`

- [ ] **Step 1: Implement**

```csharp
using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>Locks the player's curHp to maxHp every frame. Target: DataLib.UnitData.</summary>
    public sealed class GodMode : ICheatModule
    {
        public string Id => "godmode";
        public string Name => "无敌 GodMode";
        public string Category => "战斗";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private bool _enabled;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("DataLib.UnitData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { }

        public void OnUpdate()
        {
            if (!_enabled) return;
            var unit = GameRefs.UnitData;
            if (unit == null) return;
            var max = ReflectAccessor.GetInt64(unit, "maxHp");
            if (max > 0) ReflectAccessor.SetInt64(unit, "curHp", max);
        }

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            _enabled = GuiWidgets.Toggle(c.Line(), _enabled, "锁定满血 (curHp = maxHp)");
            var unit = GameRefs.UnitData;
            GuiWidgets.Label(c.Line(), unit == null
                ? "未找到 UnitData（进入游戏世界后生效）"
                : $"curHp={ReflectAccessor.GetInt64(unit, "curHp")}  maxHp={ReflectAccessor.GetInt64(unit, "maxHp")}");
        }

        public void DisableAll() => _enabled = false;
    }
}
```

- [ ] **Step 2: Build-verify** — `cd src/LunHuiCheats && dotnet build -c Release` → Build succeeded.
- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Modules/GodMode.cs"
git commit -m "feat: add GodMode module"
```

### Task 12: PlayerStats module

**Files:** Create `src/LunHuiCheats/Modules/PlayerStats.cs`

- [ ] **Step 1: Implement**

```csharp
using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Read/write player battle stats on DataLib.UnitData. Each numeric stat has a
    /// "lock" toggle that re-applies the edited value every frame.
    /// </summary>
    public sealed class PlayerStats : ICheatModule
    {
        public string Id => "player";
        public string Name => "角色属性 PlayerStats";
        public string Category => "角色";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private long _phys, _spell;
        private float _moveSpeed;
        private int _flySpeed;
        private bool _lockPhys, _lockSpell, _lockMove, _lockFly;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("DataLib.UnitData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { }

        public void OnUpdate()
        {
            var unit = GameRefs.UnitData;
            if (unit == null) return;
            if (_lockPhys)  ReflectAccessor.SetInt64(unit, "curPhysicalAttacks", _phys);
            if (_lockSpell) ReflectAccessor.SetInt64(unit, "curSpellAttacks", _spell);
            if (_lockMove)  ReflectAccessor.SetSingle(unit, "MoveSpeed", _moveSpeed);
            if (_lockFly)   ReflectAccessor.SetInt32(unit, "bigWorldFlySpeed", _flySpeed);
        }

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            var unit = GameRefs.UnitData;
            if (unit == null) { GuiWidgets.Label(c.Line(), "未找到 UnitData（进入游戏世界后生效）"); return; }

            // physical attack
            var l1 = c.Line();
            GuiWidgets.Label(new Rect(l1.x, l1.y, 70, l1.height), "物攻");
            _phys = GuiWidgets.Int64Field(new Rect(l1.x + 72, l1.y, 120, l1.height), "player.phys", _phys == 0 ? ReflectAccessor.GetInt64(unit, "curPhysicalAttacks") : _phys);
            _lockPhys = GuiWidgets.Toggle(new Rect(l1.x + 200, l1.y, 90, l1.height), _lockPhys, "锁定");

            // spell attack
            var l2 = c.Line();
            GuiWidgets.Label(new Rect(l2.x, l2.y, 70, l2.height), "法攻");
            _spell = GuiWidgets.Int64Field(new Rect(l2.x + 72, l2.y, 120, l2.height), "player.spell", _spell == 0 ? ReflectAccessor.GetInt64(unit, "curSpellAttacks") : _spell);
            _lockSpell = GuiWidgets.Toggle(new Rect(l2.x + 200, l2.y, 90, l2.height), _lockSpell, "锁定");

            // move speed
            var l3 = c.Line();
            GuiWidgets.Label(new Rect(l3.x, l3.y, 70, l3.height), $"移速 {_moveSpeed:0.0}");
            _moveSpeed = GuiWidgets.Slider(new Rect(l3.x + 72, l3.y + 6, 120, l3.height), _moveSpeed <= 0 ? ReflectAccessor.GetSingle(unit, "MoveSpeed") : _moveSpeed, 0f, 50f);
            _lockMove = GuiWidgets.Toggle(new Rect(l3.x + 200, l3.y, 90, l3.height), _lockMove, "锁定");

            // fly speed
            var l4 = c.Line();
            GuiWidgets.Label(new Rect(l4.x, l4.y, 70, l4.height), "飞行速度");
            _flySpeed = (int)GuiWidgets.Int64Field(new Rect(l4.x + 72, l4.y, 120, l4.height), "player.fly", _flySpeed == 0 ? ReflectAccessor.GetInt32(unit, "bigWorldFlySpeed") : _flySpeed);
            _lockFly = GuiWidgets.Toggle(new Rect(l4.x + 200, l4.y, 90, l4.height), _lockFly, "锁定");

            if (GuiWidgets.Button(c.Line(new Rect(0,0,120,24).height), "立即写入一次"))
            {
                ReflectAccessor.SetInt64(unit, "curPhysicalAttacks", _phys);
                ReflectAccessor.SetInt64(unit, "curSpellAttacks", _spell);
                ReflectAccessor.SetSingle(unit, "MoveSpeed", _moveSpeed);
                ReflectAccessor.SetInt32(unit, "bigWorldFlySpeed", _flySpeed);
            }
        }

        public void DisableAll()
        {
            _lockPhys = _lockSpell = _lockMove = _lockFly = false;
        }
    }
}
```

- [ ] **Step 2: Build-verify** — `dotnet build -c Release` → Build succeeded.
- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Modules/PlayerStats.cs"
git commit -m "feat: add PlayerStats module"
```

### Task 13: Cultivation module

**Files:** Create `src/LunHuiCheats/Modules/Cultivation.cs`

- [ ] **Step 1: Implement**

```csharp
using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Edits CharacterData.currentExp / currentLevel / curDaoxin. (Spirit-root editing
    /// is left to a later iteration; this module only reads spirit-root info if present.)
    /// </summary>
    public sealed class Cultivation : ICheatModule
    {
        public string Id => "cultivation";
        public string Name => "修为 Cultivation";
        public string Category => "修为";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private long _exp;
        private int _level, _daoxin;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("CharacterData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { }
        public void OnUpdate() { }   // exp/level are write-on-demand, not locked

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            var cd = GameRefs.CharacterData;
            if (cd == null) { GuiWidgets.Label(c.Line(), "未找到 CharacterData（进入游戏世界后生效）"); return; }

            var l1 = c.Line();
            GuiWidgets.Label(new Rect(l1.x, l1.y, 70, l1.height), "经验");
            _exp = GuiWidgets.Int64Field(new Rect(l1.x + 72, l1.y, 140, l1.height), "cult.exp", _exp == 0 ? ReflectAccessor.GetInt64(cd, "currentExp") : _exp);

            var l2 = c.Line();
            GuiWidgets.Label(new Rect(l2.x, l2.y, 70, l2.height), "等级");
            _level = (int)GuiWidgets.Int64Field(new Rect(l2.x + 72, l2.y, 140, l2.height), "cult.level", _level == 0 ? ReflectAccessor.GetInt32(cd, "currentLevel") : _level);

            var l3 = c.Line();
            GuiWidgets.Label(new Rect(l3.x, l3.y, 70, l3.height), "道心");
            _daoxin = (int)GuiWidgets.Int64Field(new Rect(l3.x + 72, l3.y, 140, l3.height), "cult.daoxin", _daoxin == 0 ? ReflectAccessor.GetInt32(cd, "curDaoxin") : _daoxin);

            if (GuiWidgets.Button(c.Line(), "写入 经验/等级/道心"))
            {
                ReflectAccessor.SetInt64(cd, "currentExp", _exp);
                ReflectAccessor.SetInt32(cd, "currentLevel", _level);
                ReflectAccessor.SetInt32(cd, "curDaoxin", _daoxin);
            }

            // spirit-root read-only display, if reachable
            var unit = GameRefs.UnitData;
            if (unit != null && ReflectAccessor.TryGet(unit, "discipleSpiritData", out var dsd) && dsd != null)
                GuiWidgets.Label(c.Line(), "灵根数据已找到（编辑功能后续迭代）");
        }

        public void DisableAll() { }
    }
}
```

- [ ] **Step 2: Build-verify** — `dotnet build -c Release` → Build succeeded.
- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Modules/Cultivation.cs"
git commit -m "feat: add Cultivation module"
```

### Task 14: Inventory module — phase 1 (clone existing items)

**Files:** Create `src/LunHuiCheats/Modules/Inventory.cs`

Phase 1 only adds quantity to items the inventory ALREADY contains (via `All*` lists), so no `BaseRewardData` needs to be constructed.

- [ ] **Step 1: Implement**

```csharp
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Inventory cheat. Phase 1: browse items the FakeInventoryData already holds
    /// (its All* category lists) and add quantity via AddItem(existing, qty).
    /// </summary>
    public sealed class Inventory : ICheatModule
    {
        public string Id => "inventory";
        public string Name => "背包 Inventory";
        public string Category => "背包";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // Inventory list field name -> display category.
        private static readonly (string field, string cat)[] Lists =
        {
            ("AllDanYao", "丹药"), ("AllEquips", "装备"), ("AllMaterials", "材料"),
            ("AllUseItem", "消耗"), ("AllPets", "宠物"), ("AllFlyTalisman", "飞行符"),
            ("AllSeedMaterials", "种子"), ("AllCoins", "货币"),
        };

        private readonly ItemBrowserModel _model = new();
        private readonly ItemBrowserView _view = new();
        private int _lastBuildFrame = -1000;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("FakeInventoryData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { }
        public void OnUpdate() { }

        private void RebuildRows()
        {
            // Rebuild at most every ~60 frames (lists are large; reflection is costly).
            if (Time.frameCount - _lastBuildFrame < 60) return;
            _lastBuildFrame = Time.frameCount;

            var inv = GameRefs.Inventory;
            if (inv == null) { _model.SetRows(new List<ItemRow>()); return; }

            var rows = new List<ItemRow>();
            foreach (var (field, cat) in Lists)
            {
                if (!ReflectAccessor.TryGet(inv, field, out var listObj) || listObj is not IEnumerable en) continue;
                foreach (var item in en)
                {
                    if (item == null) continue;
                    var name = ReflectAccessor.TryGet(item, "name", out var n) && n != null ? n.ToString() : item.ToString();
                    rows.Add(new ItemRow(name ?? "?", cat, item));
                }
            }
            _model.SetRows(rows);
        }

        public void DrawGui()
        {
            var inv = GameRefs.Inventory;
            if (inv == null)
            {
                GUI.Label(new Rect(0, 0, 360, 20), "未找到 FakeInventoryData（进入游戏世界后生效）");
                return;
            }
            RebuildRows();
            _view.Draw(new Rect(0, 0, 380, 300), _model, OnAdd);
        }

        private void OnAdd(ItemRow row, long qty)
        {
            var inv = GameRefs.Inventory;
            if (inv == null) return;
            // AddCoin(CoinData,int) for coins, AddItem(BaseRewardData,int) otherwise.
            var method = row.Category == "货币" ? "AddCoin" : "AddItem";
            CallAdd(inv, method, row.Payload, (int)qty);
        }

        private static void CallAdd(object inv, string method, object payload, int qty)
        {
            try
            {
                var m = AccessTools.Method(inv.GetType(), method, new[] { payload.GetType(), typeof(int) });
                m?.Invoke(inv, new object[] { payload, qty });
            }
            catch (System.Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[Inventory] {method} failed: {ex.Message}");
            }
        }

        public void DisableAll() { }
    }
}
```

- [ ] **Step 2: Build-verify** — `dotnet build -c Release` → Build succeeded.
- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Modules/Inventory.cs"
git commit -m "feat: add Inventory module (phase 1: add qty to existing items)"
```

### Task 15: Inventory phase 2 — construct BaseRewardData by id (best-effort)

**Files:** Modify `src/LunHuiCheats/Modules/Inventory.cs`

Best-effort: let the user type an item id and try to construct a `BaseRewardData`. If construction fails, the panel says so and phase-1 behavior is unaffected.

- [ ] **Step 1: Add an id field + construct path** to the module. Add fields:

```csharp
        private long _newItemId = 1;
```

Add to the top of `DrawGui()` (after the `inv == null` guard, before `RebuildRows()`), a one-line "add by id" row:

```csharp
            var top = new Rect(0, 0, 380, 24);
            GUI.Label(new Rect(0, 0, 60, 24), "物品ID");
            _newItemId = GuiWidgets.Int64Field(new Rect(62, 0, 100, 24), "inv.newid", _newItemId);
            if (GUI.Button(new Rect(166, 0, 90, 24), "尝试添加"))
                TryAddById(_newItemId, 1);
```

and shift the browser area down by 28px: change `_view.Draw(new Rect(0, 0, 380, 300), ...)` to `_view.Draw(new Rect(0, 28, 380, 272), ...)`.

- [ ] **Step 2: Implement TryAddById**

```csharp
        private void TryAddById(long id, int qty)
        {
            var inv = GameRefs.Inventory;
            if (inv == null) return;
            var brt = AccessTools.TypeByName("BaseRewardData");
            if (brt == null) { Plugin.LogSrc?.LogWarning("[Inventory] BaseRewardData type not found."); return; }
            try
            {
                var reward = System.Activator.CreateInstance(brt);
                if (reward == null) return;
                // Common id field names on reward data; set whichever exists.
                ReflectAccessor.TrySet(reward, "id", id);
                ReflectAccessor.TrySet(reward, "itemId", id);
                CallAdd(inv, "AddItem", reward, qty);
                Plugin.LogSrc?.LogInfo($"[Inventory] TryAddById({id}) invoked AddItem.");
            }
            catch (System.Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[Inventory] construct BaseRewardData failed: {ex.Message}");
            }
        }
```

- [ ] **Step 3: Build-verify** — `dotnet build -c Release` → Build succeeded.
- [ ] **Step 4: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Modules/Inventory.cs"
git commit -m "feat: Inventory phase 2 — best-effort add item by id"
```

### Task 16: Register the new modules

**Files:** Modify `src/LunHuiCheats/Plugin.cs`

- [ ] **Step 1: Add registrations** right after the existing two `Registry.Add(...)` lines:

```csharp
                Registry.Add(new Modules.GodMode());
                Registry.Add(new Modules.PlayerStats());
                Registry.Add(new Modules.Cultivation());
                Registry.Add(new Modules.Inventory());
```

- [ ] **Step 2: Build-verify** — `cd src/LunHuiCheats && dotnet build -c Release` → Build succeeded.
- [ ] **Step 3: Run full test suite** — `cd ../LunHuiCheats.Tests && dotnet test` → PASS.
- [ ] **Step 4: Smoke-verify (Windows)** — install, launch, press `P`: sidebar shows 战斗 / 角色 / 修为 / 背包 / 通用 / 调试; each module renders; GodMode toggle holds HP; PlayerStats edits apply; Cultivation writes; Inventory lists items and Add increases count.
- [ ] **Step 5: Commit**

```bash
git add "轮回修仙路/src/LunHuiCheats/Plugin.cs"
git commit -m "feat: register GodMode/PlayerStats/Cultivation/Inventory modules"
```

---

## Phase 4 — Docs

### Task 17: smoke-checklist + ROADMAP

**Files:** Modify `docs/smoke-checklist.md`, `ROADMAP.md`

- [ ] **Step 1: Add smoke-checklist rows** for each module:

```markdown
- [ ] godmode: 开启锁血后受击 curHp 不降（curHp 持续 = maxHp）
- [ ] player: 改物攻/法攻/移速/飞行速度并锁定，数值在游戏中生效且保持
- [ ] cultivation: 写入经验/等级/道心后角色面板更新；先备份存档
- [ ] inventory: 列出已有物品；Add 后对应物品数量增加；货币用 AddCoin
- [ ] inventory(二期): 输入物品ID 尝试添加，成功则新物品入背包（失败有日志，不崩溃）
- [ ] panel: 分类侧栏切换 / 搜索过滤 / 排序切换 / Disable All 还原所有锁
```

- [ ] **Step 2: Update ROADMAP** — move Phase 1 runtime modules from `[ ]` to `[x]` for the ones implemented, bump version note to v0.0.3, and link this plan.

- [ ] **Step 3: Commit**

```bash
git add "轮回修仙路/docs/smoke-checklist.md" "轮回修仙路/ROADMAP.md"
git commit -m "docs: smoke-checklist + roadmap for cheat-panel port"
```

---

## Self-Review

**Spec coverage:** Layer 1 UI (sidebar/search/sort/scroll/item-browser) → Tasks 7–10; `ICheatModule.Category/OnUpdate` → Task 1; ReflectAccessor → Task 2; FilterSort → Task 3; ItemBrowserModel → Task 4; registry OnUpdateAll/Categories → Task 5; GameRefs cache + per-frame dispatch → Task 6; GodMode/PlayerStats/Cultivation/Inventory(1+2) → Tasks 11–15; registration → Task 16; tests → Tasks 2–5; smoke-checklist → Task 17. All §3–§7 spec items mapped.

**Placeholder scan:** No "TBD/TODO". Inventory phase-2 is explicitly best-effort with a defined fallback (spec §3 risk). `EstimateModuleHeight` is a deliberate fixed v1 budget, documented inline.

**Type consistency:** `ICheatModule` members (`Category`, `OnUpdate`) used identically in Tasks 1/5/11–14 and the test doubles. `ReflectAccessor.{TryGet,TrySet,GetInt64,SetInt64,GetInt32,SetInt32,GetSingle,SetSingle}` used consistently in modules. `FilterSort.Apply` signature identical across Tasks 3/4/10. `ItemRow(name,category,payload)` / `ItemBrowserModel.{SetRows,Categories,Visible,Query,SelectedCategory,Sort}` consistent across Tasks 4/9/14. `GameRefs.{CharacterData,UnitData,Inventory}` consistent across Tasks 6/11–14.

**Known limitation (not a gap):** GUI/module tasks (7–16) are verified by compile + manual smoke on Windows, not unit tests — game behavior cannot be unit-tested without the IL2CPP runtime. This matches spec §6.
