# Lords & Villeins 修改器 — 实施计划

> **Status:** EXECUTED 2026-04-22 — all 22 tasks complete, `Patch summary: 4/4 ok`. See `../../../ROADMAP.md` for current state and next-version plans.
>
> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现一个 BepInEx 5 + HarmonyX 的 Lords & Villeins 游戏内修改器,F1 弹出 IMGUI 面板,提供资源/人物/时间/建造四类作弊。

**Architecture:** 模块化(Core 框架 + 4 个独立 CheatModule)。每个模块自注册 Harmony patches、自绘 IMGUI、自管理 ConfigEntry。Core 提供:模块注册与生命周期、`GameRefs`(游戏单例缓存)、`SaveBackup`(启动时存档备份)、`SafePatch`(包错的 Harmony 包装)、`GuiManager`(F1 总面板 + Tab)。

**Tech Stack:** C# / netstandard2.1 / BepInEx 5.4.21 / HarmonyX 2.10 / Unity IMGUI / xUnit (tests, net8.0).

**Spec:** `LordsAndVilleins/docs/superpowers/specs/2026-04-22-lords-villeins-modder-design.md`

---

## 路径与前置假设

下文统一记号:

| 记号 | 实际路径 |
|---|---|
| `<PROJ>` | `C:\Users\keven\Desktop\gamecheats\LordsAndVilleins` |
| `<GAME>` | `E:\SteamLibrary\steamapps\common\Lords & Villeins` |
| `<MANAGED>` | `<GAME>\Lords and Villeins_Data\Managed` |
| `<BEPINEX>` | `<GAME>\BepInEx` |
| `<PLUGIN_DIR>` | `<BEPINEX>\plugins\LordsAndVilleinsCheats` |

执行前确认:
- `<GAME>\Lords and Villeins.exe` 存在
- `<MANAGED>\Assembly-CSharp.dll` 存在
- `dotnet --list-sdks` 输出包含一个 .NET 8.x SDK(测试项目用)
- `dotnet --list-sdks` 也覆盖编译 netstandard2.1 的 SDK(任何 ≥ 6.0 的 SDK 都行)
- dnSpy / dnSpyEx 可用(reverse-engineering)
- 已有 git 仓库:`C:\Users\keven\Desktop\gamecheats\` (`main` 分支)

工作分支建议:在仓库根 `git checkout -b feat/initial-build` 后再执行本计划。每次 `git commit` 都在该分支上。

---

## 文件结构(实施完成态)

```
<PROJ>/
├── README.md
├── LordsAndVilleins.sln
├── Directory.Build.props                  # MSBuild 公共属性($(GameRoot) 等)
├── refs/                                  # dnSpy 反编译笔记;.gitignore 内
│   ├── 00-research-checklist.md
│   ├── 01-economy-research.md
│   ├── 02-pawn-research.md
│   ├── 03-time-research.md
│   ├── 04-build-research.md
│   ├── 05-bootstrap-research.md
│   └── 06-version-research.md
├── src/
│   ├── LordsAndVilleinsCheats/
│   │   ├── LordsAndVilleinsCheats.csproj
│   │   ├── Plugin.cs                       # BepInEx 入口
│   │   ├── Core/
│   │   │   ├── ModuleStatus.cs             # enum
│   │   │   ├── ICheatModule.cs             # 接口
│   │   │   ├── GameRefs.cs                 # 单例引用缓存(static)
│   │   │   ├── ModConfig.cs                # BepInEx ConfigFile 包装
│   │   │   ├── ModuleRegistry.cs           # 模块注册/启停
│   │   │   └── GuiManager.cs               # F1 总面板 + Tab + Disable All
│   │   ├── Modules/
│   │   │   ├── EconomyCheats.cs
│   │   │   ├── PawnCheats.cs
│   │   │   ├── TimeCheats.cs
│   │   │   └── BuildCheats.cs
│   │   └── Util/
│   │       ├── HarmonyHelpers.cs           # SafePatch
│   │       └── SaveBackup.cs               # 启动时备份 + 轮转
│   └── LordsAndVilleinsCheats.Tests/
│       ├── LordsAndVilleinsCheats.Tests.csproj
│       ├── SaveBackupTests.cs
│       ├── ModConfigTests.cs
│       ├── ModuleRegistryTests.cs
│       └── HarmonyHelpersTests.cs
├── tools/
│   ├── install.ps1                         # 推送构建到游戏 BepInEx
│   ├── tail-log.ps1                        # 跟看 BepInEx LogOutput.log
│   └── run-and-check.ps1                   # 启动游戏并解析 log 校验加载
└── docs/
    ├── smoke-checklist.md                  # 手动冒烟清单
    └── superpowers/
        ├── specs/
        │   └── 2026-04-22-lords-villeins-modder-design.md
        └── plans/
            └── 2026-04-22-lords-villeins-modder-implementation.md   # 本文件
```

---

## TDD / 提交节奏约定

- Core 组件:**测试先行** — 写失败测试 → 跑失败 → 写实现 → 跑通过 → 提交
- 模块(Modules/*.cs):skeleton + GUI 先做,patch 在最后(依赖 dnSpy 笔记)
- 提交粒度:每 task 末尾 1 commit;研究任务每个 .md 1 commit
- 每次 commit 前必跑 `dotnet build` 与(已有时)`dotnet test`
- Commit message 格式:`<scope>: <action>`,例如 `core: implement SaveBackup rotation`、`research: document economy class layout`

---

## 阶段地图

| 阶段 | 任务范围 | 任务数 |
|---|---|---|
| Phase 1 | dnSpy 调研(6 项) | 6 |
| Phase 2 | 项目脚手架 + BepInEx 注入 | 1 |
| Phase 3 | Core 框架(8 组件) | 8 |
| Phase 4 | Plugin.Awake 完整接线 + 空面板冒烟 | 1 |
| Phase 5–8 | 4 个 CheatModule | 4 |
| Phase 9 | 工具脚本完善 | 1 |
| Phase 10 | README + 冒烟清单 + 终验 | 1 |
| **合计** | — | **22** |

---

# Phase 1 — dnSpy 调研

**目的**:固化 6 个未知项,所有 module 任务直接消费这些笔记。

**所有研究任务的通用 dnSpy 操作**:
1. 启动 dnSpy(或 dnSpyEx)
2. File → Open → `<MANAGED>\Assembly-CSharp.dll`(同时也加载 `<MANAGED>\Assembly-CSharp-firstpass.dll`)
3. 等待解析(可能 30–60 秒)
4. 用 `Ctrl+Shift+K` 跨 assembly 搜索关键词
5. 命中类右键 → Analyze 看 Used By/Uses
6. 把结论按下方模板填进 `refs/0X-...md`

**研究笔记模板(每个研究任务的产物文件都按这个写):**
```markdown
# 0X — <Topic> 调研

> 调研于 2026-04-XX
> 游戏版本:`<从 Application.version 或文件属性读>`
> Assembly-CSharp.dll SHA256:`<powershell: Get-FileHash 命令的结果>`

## 关键类型

- **类型 A**:`Namespace.ClassA`
  - 关键字段:`fieldName : Type`(用途说明)
  - 关键方法:`MethodName(args) : ReturnType`(用途说明)
- **类型 B**:...

## 修改钩子建议

| 想做的事 | 钩子方法 | Patch 类型 | 备注 |
|---|---|---|---|
| 锁金币 | `ClassA.UpdateEconomy` | Postfix | 每秒 N 次 |

## 已排除的备选

- `OtherClass.OtherMethod` — 排除原因:...

## 反编译片段(关键代码,直接粘贴)

```csharp
// ClassA.UpdateEconomy
public void UpdateEconomy() { ... }
```
```

---

### Task 1.1: Economy 调研

**Files:**
- Create: `<PROJ>/refs/00-research-checklist.md`(只在本 task 创建,后续 task 勾选项)
- Create: `<PROJ>/refs/01-economy-research.md`

- [ ] **Step 1: 创建总清单文件** `<PROJ>/refs/00-research-checklist.md`,内容:

```markdown
# Phase 1 — dnSpy 调研清单

- [ ] 01 — Economy(金币、食物、木材、石材的存储与更新)
- [ ] 02 — Pawn(村民/领主对象、属性、技能、状态字段)
- [ ] 03 — Time(游戏时间、速度乘数、季节)
- [ ] 04 — Build(蓝图、材料消耗、建造完成回调)
- [ ] 05 — Bootstrap(获取上述单例的 Awake/Start 入口)
- [ ] 06 — Version(`Application.version` 当前值,作为兼容白名单初值)

每完成一项把 `[ ]` 改为 `[x]`。
```

- [ ] **Step 2: 在 dnSpy 中按通用步骤打开 Assembly-CSharp.dll**

- [ ] **Step 3: 搜索经济相关类**

搜词:`gold`, `money`, `coin`, `economy`, `treasury`, `resource`, `food`, `wood`, `stone`, `Settlement`(Lords & Villeins 是殖民地经营,聚合体很可能叫 Settlement / Manor / Estate)。

每命中一个类 → Analyze → 看哪个方法在每帧/每 tick 被调用。

- [ ] **Step 4: 鉴别"持有金币的字段"**

候选:`int gold`、`int _gold`、`Resource[] resources`、字典 `Dictionary<ResourceType, int>`。把真实声明写下,**包括是 public/private/internal、是否 readonly、所在类的完整命名空间**。

- [ ] **Step 5: 找"经济每帧/每 tick 更新方法"**

理想钩子是一个无参数 instance 方法,Postfix 进去后 `__instance.gold = X` 直接改。如果改字段需要绕属性 getter/setter,记下来,我们后面用 reflection 写。

- [ ] **Step 6: 用研究模板填充** `<PROJ>/refs/01-economy-research.md`,模板见 Phase 1 顶部

至少要回答:
- 持有金币、食物、木材、石材的类全限定名
- 各资源字段的精确名字与类型
- 一个适合 Postfix 的"周期性方法"全限定签名
- 字段是 public 还是需要 reflection 访问

- [ ] **Step 7: 把 00-research-checklist.md 第一项改为 `[x]`**

- [ ] **Step 8: Commit**

```bash
git add LordsAndVilleins/refs/00-research-checklist.md LordsAndVilleins/refs/01-economy-research.md
git commit -m "research: document economy class layout and update hooks"
```

---

### Task 1.2: Pawn 调研

**Files:**
- Create: `<PROJ>/refs/02-pawn-research.md`

- [ ] **Step 1: 在 dnSpy 中搜索村民相关类**

搜词:`pawn`, `villager`, `villein`, `lord`, `peasant`, `character`, `npc`, `inhabitant`, `resident`, `worker`。

- [ ] **Step 2: 找出"村民集合在哪个类的哪个字段里"**

通常是 `Settlement.pawns` 或 `World.allPawns` 或类似。要能从 GameRefs.Settlement 一步拿到 `IEnumerable<Pawn>`。

- [ ] **Step 3: 鉴别属性/技能字段**

候选字段名:`stats`, `skills`, `attributes`, `traits`, `levels`。可能是字典 `Dictionary<SkillType, int>` 或数组 `int[] skills`。把准确数据结构写下。

- [ ] **Step 4: 鉴别状态字段**

饥饿:`hunger`, `food`, `nutrition`。疾病:`disease`, `sickness`, `health`, `condition`。心情:`mood`, `happiness`, `morale`。

记下:字段类型(float 0–1?int 0–100?),"健康"是大值好还是小值好。

- [ ] **Step 5: 找"是否有按村民 ID 查询的方法"**

避免遍历整个集合时性能差。如果只能遍历就遍历,记录最大集合规模预期。

- [ ] **Step 6: 填充** `<PROJ>/refs/02-pawn-research.md`(用 Phase 1 顶部模板)

- [ ] **Step 7: 把清单 02 改为 `[x]`**

- [ ] **Step 8: Commit**

```bash
git add LordsAndVilleins/refs/00-research-checklist.md LordsAndVilleins/refs/02-pawn-research.md
git commit -m "research: document pawn data layout and access path"
```

---

### Task 1.3: Time 调研

**Files:**
- Create: `<PROJ>/refs/03-time-research.md`

- [ ] **Step 1: 搜索时间系统**

搜词:`Time`, `Clock`, `Calendar`, `Season`, `Day`, `Tick`, `Speed`, `GameTime`, `TimeManager`, `WorldTime`。

- [ ] **Step 2: 找速度乘数字段**

通常是 `float gameSpeed` / `float timeScale` / `int speedLevel`。Lords & Villeins 默认有暂停/正常/快进按钮,所以应该有"档位 enum + 实际乘数"两层。

- [ ] **Step 3: 找季节/日期字段**

候选:`Season currentSeason`、`int day`、`int year`、`DateTime gameDate`。

- [ ] **Step 4: 找事件系统(可选,这阶段只需定位)**

搜词:`Event`, `Quest`, `Trigger`, `Random Event`。如果这次找不到也无所谓,Time 模块里事件控制可以等以后再做。

- [ ] **Step 5: 填充** `<PROJ>/refs/03-time-research.md`

- [ ] **Step 6: 把清单 03 改为 `[x]`**

- [ ] **Step 7: Commit**

```bash
git add LordsAndVilleins/refs/00-research-checklist.md LordsAndVilleins/refs/03-time-research.md
git commit -m "research: document time/season/speed fields"
```

---

### Task 1.4: Build 调研

**Files:**
- Create: `<PROJ>/refs/04-build-research.md`

- [ ] **Step 1: 搜索建造系统**

搜词:`Build`, `Blueprint`, `Construction`, `Building`, `Structure`, `Recipe`, `Craft`。

- [ ] **Step 2: 找"材料消耗"调用**

理想钩子是 `Construction.ConsumeMaterials()` 或类似一个 bool 返回值的 `bool TryConsume(...)`。Prefix 直接 `__result = true; return false;` 跳过原方法。

- [ ] **Step 3: 找"建造完成"回调**

候选:`OnBuildingComplete`, `FinishConstruction`, `Build.Complete`。

- [ ] **Step 4: 找"全建筑解锁"开关**

候选:`Building.unlocked`、`TechTree.unlocks`、`ResearchData.knownBuildings`。如果是 per-saved-game,可能要每次加载都重写一遍。

- [ ] **Step 5: 填充** `<PROJ>/refs/04-build-research.md`

- [ ] **Step 6: 把清单 04 改为 `[x]`**

- [ ] **Step 7: Commit**

```bash
git add LordsAndVilleins/refs/00-research-checklist.md LordsAndVilleins/refs/04-build-research.md
git commit -m "research: document build/material/unlock hooks"
```

---

### Task 1.5: Bootstrap 调研(取游戏单例的入口)

**Files:**
- Create: `<PROJ>/refs/05-bootstrap-research.md`

- [ ] **Step 1: 找游戏顶层 MonoBehaviour**

搜词:`GameManager`, `World`, `Main`, `Bootstrap`, `Game`, `Application`, `App`。在 Unity 里这种类通常是 MonoBehaviour 且 `Awake/Start` 中创建/获取主单例。

- [ ] **Step 2: 找 Settlement(或 Phase 1.1 确定的经济聚合体类)的"出生地"**

在 dnSpy 中右键该类 → Analyze → "Used By" → 找到所有把它赋值给静态字段或事件的地方。

- [ ] **Step 3: 鉴别"游戏世界已加载"信号**

候选:某 MonoBehaviour 的 `Start()` 完成后、某 event 比如 `World.OnLoaded`、某静态属性变成非 null。我们要在这个信号触发后才让 GameRefs.IsReady = true。

- [ ] **Step 4: 鉴别"主菜单"vs"游戏中"区分**

主菜单不应启用作弊。可能是:活动场景名 ≠ "MainMenu",或某 GameState enum ≠ MainMenu。

- [ ] **Step 5: 填充** `<PROJ>/refs/05-bootstrap-research.md`,**重点是要有这两段可直接抄进代码的伪 patch**:

```csharp
// 用于让 GameRefs.IsReady = true 的钩子点
[HarmonyPatch(typeof(<TYPE_FROM_RESEARCH>), "<METHOD_FROM_RESEARCH>")]
static void Postfix(<TYPE_FROM_RESEARCH> __instance) {
    GameRefs.Settlement = __instance.<PATH_TO_SETTLEMENT>;
    GameRefs.IsReady = true;
}

// 用于让 GameRefs.IsReady = false 的钩子点(返主菜单/退出游戏)
[HarmonyPatch(typeof(<TYPE_FROM_RESEARCH>), "<METHOD_FROM_RESEARCH>")]
static void Postfix() {
    GameRefs.Reset();
}
```

- [ ] **Step 6: 把清单 05 改为 `[x]`**

- [ ] **Step 7: Commit**

```bash
git add LordsAndVilleins/refs/00-research-checklist.md LordsAndVilleins/refs/05-bootstrap-research.md
git commit -m "research: document bootstrap hooks for GameRefs lifecycle"
```

---

### Task 1.6: Version 调研

**Files:**
- Create: `<PROJ>/refs/06-version-research.md`

- [ ] **Step 1: 在 PowerShell 里读 Assembly-CSharp.dll 的 hash 和文件版本**

```powershell
Get-FileHash "E:\SteamLibrary\steamapps\common\Lords & Villeins\Lords and Villeins_Data\Managed\Assembly-CSharp.dll" -Algorithm SHA256
(Get-Item "E:\SteamLibrary\steamapps\common\Lords & Villeins\Lords and Villeins.exe").VersionInfo | Format-List
```

- [ ] **Step 2: 启动游戏一次,在主菜单截图记下版本号**

(主菜单角落或设置页通常显示 `vX.Y.Z`。也可以在 dnSpy 里搜 `Application.version` 看 PlayerSettings 的字面值。)

- [ ] **Step 3: 填充** `<PROJ>/refs/06-version-research.md`

```markdown
# 06 — Version 调研

调研于 2026-04-XX。

## 当前安装版本

- 游戏内显示版本:`X.Y.Z`
- `Lords and Villeins.exe` FileVersion:`...`
- `Assembly-CSharp.dll` SHA256:`...`

## 兼容白名单(初始)

```csharp
// LordsAndVilleinsCheats/Plugin.cs 里 KnownCompatibleVersions
private static readonly HashSet<string> KnownCompatibleVersions = new() {
    "X.Y.Z",
};
```

## 不在白名单时的行为

仅 `Logger.LogWarning(...)`,继续加载。Mod 仍尝试 patch,失败的 patch 会自动降级到 Broken 状态(见 §7 错误处理)。
```

- [ ] **Step 4: 把清单 06 改为 `[x]`**

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/refs/00-research-checklist.md LordsAndVilleins/refs/06-version-research.md
git commit -m "research: pin current game version for compat whitelist"
```

---

# Phase 2 — 项目脚手架

### Task 2.1: 全部脚手架 + BepInEx 注入 + 空 Plugin 上线

**Files:**
- Create: `<PROJ>/LordsAndVilleins.sln`
- Create: `<PROJ>/Directory.Build.props`
- Create: `<PROJ>/src/LordsAndVilleinsCheats/LordsAndVilleinsCheats.csproj`
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`
- Create: `<PROJ>/tools/install.ps1`(本任务先放最小可用版,Phase 9 再扩)
- Modify: `C:\Users\keven\Desktop\gamecheats\.gitignore`(追加 `**/Directory.Build.props.user` 等)

#### 步骤组 A:创建 sln 与 Directory.Build.props

- [ ] **Step 1: 在仓库根新建 git 工作分支**

```bash
cd "C:/Users/keven/Desktop/gamecheats"
git checkout -b feat/initial-build
```

- [ ] **Step 2: 创建空 sln**

```bash
cd "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins"
dotnet new sln -n LordsAndVilleins
```

- [ ] **Step 3: 创建 `<PROJ>/Directory.Build.props`**,内容:

```xml
<Project>
  <PropertyGroup>
    <!-- 可被环境变量 GameRoot 覆盖 -->
    <GameRoot Condition="'$(GameRoot)' == ''">E:\SteamLibrary\steamapps\common\Lords &amp; Villeins</GameRoot>
    <GameManaged>$(GameRoot)\Lords and Villeins_Data\Managed</GameManaged>
    <BepInExPath>$(GameRoot)\BepInEx</BepInExPath>
    <PluginInstallDir>$(BepInExPath)\plugins\LordsAndVilleinsCheats</PluginInstallDir>
    <LangVersion>10</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

#### 步骤组 B:创建 mod 主项目

- [ ] **Step 4: 创建主项目 csproj 与 Plugin.cs**

创建 `<PROJ>/src/LordsAndVilleinsCheats/LordsAndVilleinsCheats.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>LordsAndVilleinsCheats</AssemblyName>
    <RootNamespace>LordsAndVilleinsCheats</RootNamespace>
    <Version>0.1.0</Version>
    <Configurations>Debug;Release</Configurations>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BepInEx.Core" Version="5.4.21" />
    <PackageReference Include="HarmonyX" Version="2.10.2" />
    <PackageReference Include="UnityEngine.Modules" Version="2022.3.21" IncludeAssets="compile" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>$(GameManaged)\Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="Assembly-CSharp-firstpass">
      <HintPath>$(GameManaged)\Assembly-CSharp-firstpass.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

> **关于 UnityEngine.Modules**:NuGet 包 `UnityEngine.Modules` 提供 stub references,版本要选与游戏 Unity 版本最接近的。先用 `2022.3.21`;如果游戏实际是别的版本(看 `<MANAGED>\UnityEngine.CoreModule.dll` 的属性 → 详细信息 → 产品版本),改成对应数字。如果 NuGet 版本找不到,fallback 改成 HintPath 直接引用 `<GameManaged>\UnityEngine.CoreModule.dll` + `<GameManaged>\UnityEngine.IMGUIModule.dll`。

创建 `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`:

```csharp
using BepInEx;
using BepInEx.Logging;

namespace LordsAndVilleinsCheats
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginId      = "com.kk.lav-cheats";
        public const string PluginName    = "Lords & Villeins Cheats";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log = null!;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }
    }
}
```

- [ ] **Step 5: 把项目加进 sln**

```bash
cd "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins"
dotnet sln add src/LordsAndVilleinsCheats/LordsAndVilleinsCheats.csproj
```

- [ ] **Step 6: 跑 build,验证编译干净**

```bash
cd "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins"
dotnet build src/LordsAndVilleinsCheats -c Release
```

期望:`Build succeeded. 0 Error(s)`,产物在 `src/LordsAndVilleinsCheats/bin/Release/netstandard2.1/LordsAndVilleinsCheats.dll`。

如果 UnityEngine.Modules 包解析失败,降级方案:删掉 `<PackageReference Include="UnityEngine.Modules" .../>`,在 `<ItemGroup>` 加入:

```xml
<Reference Include="UnityEngine.CoreModule">
  <HintPath>$(GameManaged)\UnityEngine.CoreModule.dll</HintPath>
  <Private>false</Private>
</Reference>
<Reference Include="UnityEngine.IMGUIModule">
  <HintPath>$(GameManaged)\UnityEngine.IMGUIModule.dll</HintPath>
  <Private>false</Private>
</Reference>
<Reference Include="UnityEngine.InputLegacyModule">
  <HintPath>$(GameManaged)\UnityEngine.InputLegacyModule.dll</HintPath>
  <Private>false</Private>
</Reference>
```

#### 步骤组 C:安装 BepInEx 到游戏

- [ ] **Step 7: 下载 BepInEx 5 x64 Pack 并解压到 `<GAME>`**

```powershell
$ProgressPreference = 'SilentlyContinue'
$url = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip"
$zip = "$env:TEMP\BepInEx_5.4.21.zip"
Invoke-WebRequest -Uri $url -OutFile $zip
Expand-Archive -Path $zip -DestinationPath "E:\SteamLibrary\steamapps\common\Lords & Villeins" -Force
Remove-Item $zip
```

- [ ] **Step 8: 启动游戏一次,让 BepInEx 自展开目录**

打开 Steam → 启动 Lords & Villeins → 进入主菜单后退出。BepInEx 会自动创建 `<BEPINEX>` 目录与 `LogOutput.log`。

验证:

```bash
ls "E:/SteamLibrary/steamapps/common/Lords & Villeins/BepInEx/"
```

期望看到:`config/`、`core/`、`plugins/`、`patchers/`、`LogOutput.log`。

#### 步骤组 D:创建最小 install.ps1 + 验证空 Plugin 加载

- [ ] **Step 9: 创建 `<PROJ>/tools/install.ps1`**(最小版,Phase 9 完善)

```powershell
param(
    [string]$Configuration = "Release",
    [string]$GameRoot = $env:LAV_GAME_ROOT
)
if (-not $GameRoot) { $GameRoot = "E:\SteamLibrary\steamapps\common\Lords & Villeins" }

$ErrorActionPreference = "Stop"
$repoRoot   = Split-Path -Parent $PSScriptRoot
$srcDll     = Join-Path $repoRoot "src/LordsAndVilleinsCheats/bin/$Configuration/netstandard2.1/LordsAndVilleinsCheats.dll"
$pluginDir  = Join-Path $GameRoot "BepInEx/plugins/LordsAndVilleinsCheats"

if (-not (Test-Path $srcDll)) {
    throw "Build artifact not found: $srcDll. Run 'dotnet build -c $Configuration' first."
}
New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
Copy-Item -Force $srcDll $pluginDir
Write-Host "Installed $srcDll -> $pluginDir"
```

- [ ] **Step 10: 运行 install.ps1**

```bash
powershell -ExecutionPolicy Bypass -File "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/tools/install.ps1"
```

期望输出:`Installed ... -> ...\BepInEx\plugins\LordsAndVilleinsCheats`。

验证:

```bash
ls "E:/SteamLibrary/steamapps/common/Lords & Villeins/BepInEx/plugins/LordsAndVilleinsCheats/"
```

期望看到 `LordsAndVilleinsCheats.dll`。

- [ ] **Step 11: 启动游戏,跟看日志**

新开一个终端:

```bash
tail -f "E:/SteamLibrary/steamapps/common/Lords & Villeins/BepInEx/LogOutput.log"
```

启动游戏。期望日志中出现:

```
[Info   :   BepInEx] Loading [Lords & Villeins Cheats 0.1.0]
[Info   :Lords & Villeins Cheats] Lords & Villeins Cheats v0.1.0 loaded.
```

如果看不到,常见原因:
- BepInEx 没装好(没 `winhttp.dll` 在游戏根目录)→ 重跑 Step 7
- DLL 没被加载(Plugin 类没继承 `BaseUnityPlugin` 或 `[BepInPlugin]` 属性写错)
- TargetFramework 不是 `netstandard2.1`

修好后重跑 Step 6 + Step 10 + Step 11。

#### 步骤组 E:提交脚手架

- [ ] **Step 12: 把游戏 DLL 路径相关提示加到根 .gitignore(若已有则跳过)**

确认 `C:/Users/keven/Desktop/gamecheats/.gitignore` 包含:`bin/`、`obj/`、`**/refs/`、`**/*.dll`。已确认存在(Phase 0 init)→ 跳过。

- [ ] **Step 13: Commit 脚手架**

```bash
cd "C:/Users/keven/Desktop/gamecheats"
git add LordsAndVilleins/LordsAndVilleins.sln LordsAndVilleins/Directory.Build.props \
        LordsAndVilleins/src/LordsAndVilleinsCheats/LordsAndVilleinsCheats.csproj \
        LordsAndVilleins/src/LordsAndVilleinsCheats/Plugin.cs \
        LordsAndVilleins/tools/install.ps1
git commit -m "scaffold: empty BepInEx plugin loads in game"
```

---

# Phase 3 — Core 框架

> 本阶段所有任务都在 git 工作分支 `feat/initial-build` 上。

### Task 3.0: xUnit 测试项目骨架

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats.Tests/LordsAndVilleinsCheats.Tests.csproj`
- Create: `<PROJ>/src/LordsAndVilleinsCheats.Tests/SmokeTest.cs`

- [ ] **Step 1: 创建测试项目 csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="BepInEx.Core" Version="5.4.21" />
    <PackageReference Include="HarmonyX" Version="2.10.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\LordsAndVilleinsCheats\LordsAndVilleinsCheats.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: 创建一个 smoke 测试,确认 runner 和项目引用都跑得起来**

`<PROJ>/src/LordsAndVilleinsCheats.Tests/SmokeTest.cs`:

```csharp
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class SmokeTest
    {
        [Fact]
        public void TestInfrastructure_Works()
        {
            Assert.Equal(2, 1 + 1);
        }
    }
}
```

- [ ] **Step 3: 把测试项目加入 sln**

```bash
cd "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins"
dotnet sln add src/LordsAndVilleinsCheats.Tests/LordsAndVilleinsCheats.Tests.csproj
```

- [ ] **Step 4: 跑测试**

```bash
cd "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins"
dotnet test src/LordsAndVilleinsCheats.Tests
```

期望:`Passed: 1, Failed: 0`。

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats.Tests/
git commit -m "test: scaffold xUnit project with smoke test"
```

---

### Task 3.1: ModuleStatus + ICheatModule

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Core/ModuleStatus.cs`
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Core/ICheatModule.cs`

(无单元测试 — 纯 enum + interface,无可测行为。)

- [ ] **Step 1: 创建 ModuleStatus.cs**

```csharp
namespace LordsAndVilleinsCheats.Core
{
    public enum ModuleStatus
    {
        Pending,    // 还没 register
        Ok,         // 全部 patch 成功
        Broken,     // 至少一个 patch 失败
        Disabled,   // 用户主动关闭
    }
}
```

- [ ] **Step 2: 创建 ICheatModule.cs**

```csharp
using HarmonyLib;

namespace LordsAndVilleinsCheats.Core
{
    public interface ICheatModule
    {
        string Id   { get; }            // 配置键前缀,稳定字符串
        string Name { get; }            // Tab 显示名

        ModuleStatus Status { get; }    // 由 Register/SafePatch 自更新

        void Register(ModConfig cfg, Harmony harmony);
        void OnGameReady();             // GameRefs.IsReady 转为 true 后调用一次
        void DrawGui();                 // 当 Tab 被选中时每帧调用
        void DisableAll();              // "Disable All" 按钮调用
    }
}
```

- [ ] **Step 3: build 验证**

```bash
dotnet build "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats" -c Release
```

期望:成功。`ModConfig` 还没创建,所以 ICheatModule 引用它会编译失败。**遇到该错误是预期的,下一个任务创建 ModConfig 后会修复**。

- [ ] **Step 4: Commit**(允许暂时编译失败,后续 task 修复)

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Core/ModuleStatus.cs LordsAndVilleins/src/LordsAndVilleinsCheats/Core/ICheatModule.cs
git commit -m "core: add ModuleStatus enum and ICheatModule interface"
```

---

### Task 3.2: GameRefs

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Core/GameRefs.cs`
- Test: `<PROJ>/src/LordsAndVilleinsCheats.Tests/GameRefsTests.cs`

- [ ] **Step 1: 写失败测试**

`<PROJ>/src/LordsAndVilleinsCheats.Tests/GameRefsTests.cs`:

```csharp
using LordsAndVilleinsCheats.Core;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class GameRefsTests
    {
        [Fact]
        public void Reset_ClearsIsReady()
        {
            GameRefs.IsReady = true;
            GameRefs.Reset();
            Assert.False(GameRefs.IsReady);
        }
    }
}
```

- [ ] **Step 2: 跑测试,确认失败(GameRefs 不存在)**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

期望:编译失败,提示 `GameRefs` 不存在。

- [ ] **Step 3: 实现 `<PROJ>/src/LordsAndVilleinsCheats/Core/GameRefs.cs`**

```csharp
namespace LordsAndVilleinsCheats.Core
{
    /// <summary>
    /// Static cache of game-side singleton references.
    /// Populated by Harmony patches during game bootstrap; reset on returning to main menu.
    /// </summary>
    public static class GameRefs
    {
        public static bool IsReady;

        // 这些字段类型在 Phase 5+ 模块实现时,需要把 object 改为 refs/05-bootstrap-research.md 里
        // 确认的真实 Settlement 类。先用 object 占位,Phase 4 完成时类型会被替换。
        public static object? Settlement;

        public static void Reset()
        {
            IsReady = false;
            Settlement = null;
        }
    }
}
```

- [ ] **Step 4: 跑测试,确认通过**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

期望:`Passed: 2, Failed: 0`。

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Core/GameRefs.cs LordsAndVilleins/src/LordsAndVilleinsCheats.Tests/GameRefsTests.cs
git commit -m "core: add GameRefs with Reset behavior"
```

---

### Task 3.3: HarmonyHelpers.SafePatch

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Util/HarmonyHelpers.cs`
- Test: `<PROJ>/src/LordsAndVilleinsCheats.Tests/HarmonyHelpersTests.cs`

- [ ] **Step 1: 写失败测试**

```csharp
using System;
using LordsAndVilleinsCheats.Util;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class HarmonyHelpersTests
    {
        [Fact]
        public void SafeRun_ReturnsTrue_WhenActionSucceeds()
        {
            var result = HarmonyHelpers.SafeRun("test-op", () => { });
            Assert.True(result);
        }

        [Fact]
        public void SafeRun_ReturnsFalse_AndSwallows_WhenActionThrows()
        {
            var result = HarmonyHelpers.SafeRun("test-op", () => throw new InvalidOperationException("boom"));
            Assert.False(result);
        }
    }
}
```

- [ ] **Step 2: 跑测试,确认失败**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

- [ ] **Step 3: 实现 HarmonyHelpers.cs**

```csharp
using System;

namespace LordsAndVilleinsCheats.Util
{
    /// <summary>
    /// Wraps Harmony patch registration / patch body invocations so any failure
    /// is logged and absorbed instead of breaking the plugin or the game.
    /// </summary>
    public static class HarmonyHelpers
    {
        // 测试无 BepInEx logger,这里把 logger 做成可注入,生产代码 Plugin.Awake 注入。
        public static Action<string>? OnFailure { get; set; }

        public static bool SafeRun(string opName, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                OnFailure?.Invoke($"[SafeRun] {opName} failed: {ex}");
                return false;
            }
        }
    }
}
```

- [ ] **Step 4: 跑测试,通过**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

期望:4 passed。

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Util/HarmonyHelpers.cs LordsAndVilleins/src/LordsAndVilleinsCheats.Tests/HarmonyHelpersTests.cs
git commit -m "util: add HarmonyHelpers.SafeRun with logged-and-swallowed failures"
```

---

### Task 3.4: SaveBackup

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Util/SaveBackup.cs`
- Test: `<PROJ>/src/LordsAndVilleinsCheats.Tests/SaveBackupTests.cs`

- [ ] **Step 1: 写失败测试 — 三个场景**

```csharp
using System;
using System.IO;
using System.Linq;
using LordsAndVilleinsCheats.Util;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class SaveBackupTests : IDisposable
    {
        private readonly string _root;
        public SaveBackupTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "lav-cheats-tests-" + Guid.NewGuid());
            Directory.CreateDirectory(_root);
        }
        public void Dispose() => Directory.Delete(_root, true);

        private string MakeSave(string name)
        {
            var path = Path.Combine(_root, name);
            File.WriteAllText(path, "fake save");
            return path;
        }

        [Fact]
        public void Run_CopiesAllSgzIntoTimestampedSubdir()
        {
            MakeSave("a.sgz");
            MakeSave("b.sgz");
            MakeSave("notes.txt"); // 非 .sgz 不应被复制

            SaveBackup.Run(_root, maxKeep: 5);

            var backupDir = Directory.GetDirectories(Path.Combine(_root, "_modbackup")).Single();
            var files = Directory.GetFiles(backupDir).Select(Path.GetFileName).OrderBy(x => x).ToArray();
            Assert.Equal(new[] { "a.sgz", "b.sgz" }, files);
        }

        [Fact]
        public void Run_RotatesOldestOut_WhenExceedingMaxKeep()
        {
            MakeSave("a.sgz");

            // 模拟 7 次启动备份
            for (int i = 0; i < 7; i++)
            {
                SaveBackup.Run(_root, maxKeep: 5);
                System.Threading.Thread.Sleep(20); // 保证目录名时间戳不重复
            }

            var backups = Directory.GetDirectories(Path.Combine(_root, "_modbackup"));
            Assert.Equal(5, backups.Length);
        }

        [Fact]
        public void Run_NoSaves_DoesNotCreateBackupDir()
        {
            SaveBackup.Run(_root, maxKeep: 5);
            Assert.False(Directory.Exists(Path.Combine(_root, "_modbackup")));
        }
    }
}
```

- [ ] **Step 2: 跑测试,确认全部失败(类不存在)**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

- [ ] **Step 3: 实现 SaveBackup.cs**

```csharp
using System;
using System.IO;
using System.Linq;

namespace LordsAndVilleinsCheats.Util
{
    /// <summary>
    /// On plugin start, copy all *.sgz from the save directory to a timestamped
    /// subdir under _modbackup/. Rotate so at most <paramref name="maxKeep"/> backups
    /// remain (oldest deleted).
    /// </summary>
    public static class SaveBackup
    {
        private const string BackupRoot = "_modbackup";

        public static void Run(string saveDir, int maxKeep)
        {
            if (!Directory.Exists(saveDir)) return;

            var saves = Directory.GetFiles(saveDir, "*.sgz", SearchOption.TopDirectoryOnly);
            if (saves.Length == 0) return;

            var backupParent = Path.Combine(saveDir, BackupRoot);
            Directory.CreateDirectory(backupParent);

            var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
            var thisRun = Path.Combine(backupParent, stamp);
            Directory.CreateDirectory(thisRun);

            foreach (var src in saves)
            {
                var dest = Path.Combine(thisRun, Path.GetFileName(src));
                File.Copy(src, dest, overwrite: true);
            }

            // 轮转
            var existing = Directory.GetDirectories(backupParent)
                                    .OrderByDescending(d => d)  // 时间戳字典序 = 时间序
                                    .ToList();
            foreach (var oldDir in existing.Skip(maxKeep))
            {
                try { Directory.Delete(oldDir, recursive: true); }
                catch { /* 任何失败保持静默,不阻塞 mod 加载 */ }
            }
        }
    }
}
```

- [ ] **Step 4: 跑测试,确认全部通过**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

期望:7 passed。

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Util/SaveBackup.cs LordsAndVilleins/src/LordsAndVilleinsCheats.Tests/SaveBackupTests.cs
git commit -m "util: add SaveBackup with rotation"
```

---

### Task 3.5: ModConfig

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Core/ModConfig.cs`
- Test: `<PROJ>/src/LordsAndVilleinsCheats.Tests/ModConfigTests.cs`

`ModConfig` 是 BepInEx `ConfigFile` 的薄包装 + 一组共享的 ConfigEntry。模块自己的 ConfigEntry 在各模块内部声明,这里只放跨模块共用的(主热键、面板宽高、Disable All 状态等)。

- [ ] **Step 1: 写失败测试**

```csharp
using System.IO;
using BepInEx.Configuration;
using LordsAndVilleinsCheats.Core;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class ModConfigTests
    {
        [Fact]
        public void Constructor_BindsDefaults_WhenFileMissing()
        {
            var path = Path.Combine(Path.GetTempPath(), $"lav-cfg-{System.Guid.NewGuid()}.cfg");
            try
            {
                var file = new ConfigFile(path, saveOnInit: true);
                var cfg = new ModConfig(file);

                Assert.Equal(UnityEngine.KeyCode.F1, cfg.ToggleKey.Value);
                Assert.False(cfg.GlobalDisableAll.Value);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
```

> **注意**:此测试依赖 `UnityEngine.KeyCode`。`UnityEngine.CoreModule.dll` 在 net8.0 测试主机能加载(不需要 Unity runtime,只需要类型定义)。如果 NuGet 拉不到 UnityEngine.Modules,改测试为 `Assert.Equal((int)UnityEngine.KeyCode.F1, (int)cfg.ToggleKey.Value)` — 实际上还是要类型定义。如果完全无法在测试环境加载 UnityEngine,fallback:把 ToggleKey 类型改为 `string`,在 Plugin.Awake 用 `(KeyCode)Enum.Parse(typeof(KeyCode), cfg.ToggleKey.Value)` 转。本计划默认采用 KeyCode 类型;如冲突再降级。

- [ ] **Step 2: 跑失败测试**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

- [ ] **Step 3: 实现 ModConfig.cs**

```csharp
using BepInEx.Configuration;
using UnityEngine;

namespace LordsAndVilleinsCheats.Core
{
    /// <summary>
    /// Wraps the shared BepInEx ConfigFile. Module-specific entries are bound
    /// inside each module via this object's File property.
    /// </summary>
    public class ModConfig
    {
        public ConfigFile File { get; }

        public ConfigEntry<KeyCode> ToggleKey       { get; }
        public ConfigEntry<bool>    GlobalDisableAll { get; }
        public ConfigEntry<int>     PanelWidth      { get; }
        public ConfigEntry<int>     PanelHeight     { get; }

        public ModConfig(ConfigFile file)
        {
            File = file;

            ToggleKey = file.Bind(
                "General", "ToggleKey", KeyCode.F1,
                "Hotkey to toggle the cheats panel.");

            GlobalDisableAll = file.Bind(
                "General", "GlobalDisableAll", false,
                "When true, all module Lock/Force behaviors are suppressed regardless of per-module settings.");

            PanelWidth = file.Bind(
                "UI", "PanelWidth", 460,
                "Cheats panel width in pixels.");

            PanelHeight = file.Bind(
                "UI", "PanelHeight", 520,
                "Cheats panel height in pixels.");
        }
    }
}
```

- [ ] **Step 4: 跑测试**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

期望:8 passed。如果加载 UnityEngine 失败,按 Step 1 的 fallback 处理。

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Core/ModConfig.cs LordsAndVilleins/src/LordsAndVilleinsCheats.Tests/ModConfigTests.cs
git commit -m "core: add ModConfig with shared ConfigEntries"
```

---

### Task 3.6: ModuleRegistry

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Core/ModuleRegistry.cs`
- Test: `<PROJ>/src/LordsAndVilleinsCheats.Tests/ModuleRegistryTests.cs`

- [ ] **Step 1: 写失败测试**

```csharp
using System.Collections.Generic;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class ModuleRegistryTests
    {
        private class FakeModule : ICheatModule
        {
            public string Id   => "fake";
            public string Name => "Fake";
            public ModuleStatus Status { get; set; } = ModuleStatus.Pending;

            public bool RegisterCalled, OnGameReadyCalled, DrawGuiCalled, DisableAllCalled;

            public void Register(ModConfig cfg, Harmony harmony) { RegisterCalled = true; Status = ModuleStatus.Ok; }
            public void OnGameReady() => OnGameReadyCalled = true;
            public void DrawGui()     => DrawGuiCalled = true;
            public void DisableAll()  { DisableAllCalled = true; Status = ModuleStatus.Disabled; }
        }

        [Fact]
        public void Register_CallsEachModuleRegister()
        {
            var reg = new ModuleRegistry();
            var m = new FakeModule();
            reg.Add(m);
            reg.RegisterAll(cfg: null!, harmony: null!);

            Assert.True(m.RegisterCalled);
            Assert.Equal(ModuleStatus.Ok, m.Status);
        }

        [Fact]
        public void DisableAll_CallsEachModuleDisableAll()
        {
            var reg = new ModuleRegistry();
            var m1 = new FakeModule();
            var m2 = new FakeModule();
            reg.Add(m1); reg.Add(m2);

            reg.DisableAll();

            Assert.True(m1.DisableAllCalled);
            Assert.True(m2.DisableAllCalled);
        }

        [Fact]
        public void NotifyGameReady_CallsOnGameReadyExactlyOnce_AcrossMultipleCalls()
        {
            var reg = new ModuleRegistry();
            var m = new FakeModule();
            reg.Add(m);

            reg.NotifyGameReady();
            reg.NotifyGameReady();   // 第二次应被吞掉

            Assert.True(m.OnGameReadyCalled);
            // 没有计数,但 OnGameReadyCalled bool 检查已足够保证至少一次。
            // 二次调用的吞掉行为通过下一句保证:
            m.OnGameReadyCalled = false;
            reg.NotifyGameReady();
            Assert.False(m.OnGameReadyCalled);
        }
    }
}
```

- [ ] **Step 2: 跑失败测试**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

- [ ] **Step 3: 实现 ModuleRegistry.cs**

```csharp
using System.Collections.Generic;
using HarmonyLib;

namespace LordsAndVilleinsCheats.Core
{
    public class ModuleRegistry
    {
        private readonly List<ICheatModule> _modules = new();
        private bool _gameReadyDispatched;

        public IReadOnlyList<ICheatModule> Modules => _modules;

        public void Add(ICheatModule module) => _modules.Add(module);

        public void RegisterAll(ModConfig cfg, Harmony harmony)
        {
            foreach (var m in _modules)
            {
                m.Register(cfg, harmony);
            }
        }

        public void NotifyGameReady()
        {
            if (_gameReadyDispatched) return;
            _gameReadyDispatched = true;
            foreach (var m in _modules) m.OnGameReady();
        }

        public void ResetGameReady() => _gameReadyDispatched = false;

        public void DisableAll()
        {
            foreach (var m in _modules) m.DisableAll();
        }
    }
}
```

- [ ] **Step 4: 跑测试**

```bash
dotnet test "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats.Tests"
```

期望:11 passed。

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Core/ModuleRegistry.cs LordsAndVilleins/src/LordsAndVilleinsCheats.Tests/ModuleRegistryTests.cs
git commit -m "core: add ModuleRegistry with one-shot game-ready dispatch"
```

---

### Task 3.7: GuiManager

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Core/GuiManager.cs`

(无单元测试;依赖 UnityEngine OnGUI。集成测试在 Phase 4 终末游戏内手动验证。)

- [ ] **Step 1: 实现 GuiManager.cs**

```csharp
using UnityEngine;

namespace LordsAndVilleinsCheats.Core
{
    /// <summary>
    /// Renders the F-key panel: a tabbed window with one tab per module,
    /// plus a top-bar "Disable All" red button and a "module status" indicator.
    /// </summary>
    public class GuiManager
    {
        private readonly ModuleRegistry _registry;
        private readonly ModConfig _config;
        private bool _open;
        private int _activeTab;
        private Rect _windowRect;
        private const int WindowId = 0xCEA751;

        public GuiManager(ModuleRegistry registry, ModConfig config)
        {
            _registry   = registry;
            _config     = config;
            _windowRect = new Rect(40, 40, config.PanelWidth.Value, config.PanelHeight.Value);
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(_config.ToggleKey.Value))
                _open = !_open;
        }

        public void OnGUI()
        {
            if (!_open) return;
            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "Lords & Villeins Cheats");
            _config.PanelWidth.Value  = (int)_windowRect.width;
            _config.PanelHeight.Value = (int)_windowRect.height;
        }

        private void DrawWindow(int id)
        {
            // 顶栏:Disable All + GameRefs.IsReady 状态
            using (new GUILayout.HorizontalScope())
            {
                var prev = GUI.color;
                GUI.color = _config.GlobalDisableAll.Value ? Color.gray : Color.red;
                if (GUILayout.Button(_config.GlobalDisableAll.Value ? "All Disabled (click to re-enable)" : "Disable All",
                                     GUILayout.Height(24)))
                {
                    _config.GlobalDisableAll.Value = !_config.GlobalDisableAll.Value;
                    if (_config.GlobalDisableAll.Value) _registry.DisableAll();
                }
                GUI.color = prev;

                GUILayout.FlexibleSpace();
                GUILayout.Label(GameRefs.IsReady ? "● in-game" : "○ menu",
                                GUILayout.Width(80));
            }

            GUILayout.Space(4);

            // 未在游戏中:不画 Tab,提示等待
            if (!GameRefs.IsReady)
            {
                GUILayout.Label("Waiting for game world to load…");
                GUI.DragWindow();
                return;
            }

            // Tab 行
            var tabs = new string[_registry.Modules.Count];
            for (int i = 0; i < _registry.Modules.Count; i++)
            {
                var m = _registry.Modules[i];
                var marker = m.Status switch
                {
                    ModuleStatus.Broken   => " (!)",
                    ModuleStatus.Disabled => " (off)",
                    _ => "",
                };
                tabs[i] = m.Name + marker;
            }
            _activeTab = GUILayout.Toolbar(_activeTab, tabs);

            GUILayout.Space(4);
            using (var scroll = new GUILayout.ScrollViewScope(Vector2.zero))
            {
                if (_activeTab < _registry.Modules.Count)
                {
                    var module = _registry.Modules[_activeTab];
                    if (module.Status == ModuleStatus.Broken)
                    {
                        GUILayout.Label("This module's patches failed to apply. See BepInEx LogOutput.log.");
                    }
                    else
                    {
                        module.DrawGui();
                    }
                }
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
```

- [ ] **Step 2: build 验证**

```bash
dotnet build "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats" -c Release
```

期望:成功。

- [ ] **Step 3: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Core/GuiManager.cs
git commit -m "core: add GuiManager with toggle, tab bar, and Disable All button"
```

---

# Phase 4 — Plugin.Awake 完整接线 + 空面板冒烟

### Task 4.1: 接线所有 Core 组件 + 游戏内空面板验证

**Files:**
- Modify: `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`(整个重写)

- [ ] **Step 1: 重写 Plugin.cs,接线所有 Core 组件**

```csharp
using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginId      = "com.kk.lav-cheats";
        public const string PluginName    = "Lords & Villeins Cheats";
        public const string PluginVersion = "0.1.0";

        // 来自 refs/06-version-research.md;实施时把字符串替换为该笔记里记录的版本
        private static readonly string[] KnownCompatibleVersions = { "<FILL_FROM_06_RESEARCH>" };

        internal static ManualLogSource Log = null!;
        internal static ModConfig       Cfg = null!;
        internal static ModuleRegistry  Registry = null!;
        internal static GuiManager      Gui = null!;
        internal static Harmony         HarmonyInstance = null!;

        private void Awake()
        {
            Log = Logger;
            HarmonyHelpers.OnFailure = msg => Log.LogError(msg);

            try
            {
                CheckGameVersion();
                BackupSavesOnce();

                Cfg             = new ModConfig(Config);
                HarmonyInstance = new Harmony(PluginId);
                Registry        = new ModuleRegistry();

                // Phase 5–8 在此处 Add(new XxxCheats())
                // Registry.Add(new Modules.EconomyCheats());
                // Registry.Add(new Modules.PawnCheats());
                // Registry.Add(new Modules.TimeCheats());
                // Registry.Add(new Modules.BuildCheats());

                Registry.RegisterAll(Cfg, HarmonyInstance);

                // 顶层 PatchAll 抓本程序集所有 [HarmonyPatch] 类
                HarmonyHelpers.SafeRun("Harmony.PatchAll", () => HarmonyInstance.PatchAll());

                Gui = new GuiManager(Registry, Cfg);

                LogPatchSummary();
                Log.LogInfo($"{PluginName} v{PluginVersion} ready (modules: {Registry.Modules.Count}).");
            }
            catch (Exception ex)
            {
                Log.LogFatal($"Plugin failed to initialize: {ex}");
            }
        }

        private void Update()
        {
            try { Gui?.HandleInput(); } catch (Exception ex) { Log.LogError(ex); }
        }

        private void OnGUI()
        {
            try { Gui?.OnGUI(); } catch (Exception ex) { Log.LogError(ex); }
        }

        // -------------------------------------------------------------------

        private void CheckGameVersion()
        {
            var v = Application.version;
            if (Array.IndexOf(KnownCompatibleVersions, v) < 0)
            {
                Log.LogWarning(
                    $"Game version '{v}' is not in the compatibility whitelist. " +
                    $"Mod will continue loading; broken patches will be reported per-module.");
            }
            else
            {
                Log.LogInfo($"Game version '{v}' is in compatibility whitelist.");
            }
        }

        private void BackupSavesOnce()
        {
            // %USERPROFILE%\AppData\LocalLow\Honestly Games\Lords and Villeins\SaveData\<SteamID>\
            var localLow = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow",
                "Honestly Games", "Lords and Villeins", "SaveData");

            if (!Directory.Exists(localLow))
            {
                Log.LogWarning($"Save root not found: {localLow}. Skipping backup.");
                return;
            }
            // 每个 SteamID 子目录单独备份
            foreach (var steamDir in Directory.GetDirectories(localLow))
            {
                HarmonyHelpers.SafeRun($"SaveBackup({Path.GetFileName(steamDir)})",
                    () => SaveBackup.Run(steamDir, maxKeep: 5));
            }
        }

        private void LogPatchSummary()
        {
            var ok      = Registry.Modules.Count(m => m.Status == ModuleStatus.Ok);
            var broken  = Registry.Modules.Count(m => m.Status == ModuleStatus.Broken);
            var pending = Registry.Modules.Count(m => m.Status == ModuleStatus.Pending);
            Log.LogInfo($"Patch summary: {ok}/{Registry.Modules.Count} ok, {broken} broken, {pending} pending.");
        }
    }
}
```

> **占位** `<FILL_FROM_06_RESEARCH>`:从 `refs/06-version-research.md` 拷贝当前游戏版本字符串。例如 `"1.4.2"`。这是本任务必须执行的替换,不是延后项。

- [ ] **Step 2: 替换 KnownCompatibleVersions 占位**

打开 `refs/06-version-research.md`,把 "当前游戏内显示版本" 字段值替换到 `Plugin.cs` 第 17 行的字符串数组里。

- [ ] **Step 3: build**

```bash
dotnet build "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats" -c Release
```

期望:成功。

- [ ] **Step 4: install + 启动游戏 + 跟看日志**

```bash
powershell -ExecutionPolicy Bypass -File "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/tools/install.ps1"
tail -f "E:/SteamLibrary/steamapps/common/Lords & Villeins/BepInEx/LogOutput.log" &
# 然后 Steam 启动游戏
```

期望日志:
- `Game version '<X.Y.Z>' is in compatibility whitelist.`
- `Patch summary: 0/0 ok, 0 broken, 0 pending.`(此时还没注册任何模块)
- `Lords & Villeins Cheats v0.1.0 ready (modules: 0).`

- [ ] **Step 5: 进入游戏(开新存档/读存档)按 F1**

期望:
- F1 切出灰色窗口,标题 "Lords & Villeins Cheats"
- 顶部红色 "Disable All" 按钮
- 状态指示 "● in-game"
- Tab 行为空(Toolbar 没条目,允许 — 也可以暂时显示 "No modules" 文字)
- 再按 F1 关闭

如果窗口出不来,常见原因:
- `Application.version` 抛异常 → 看 Log
- `OnGUI` 在 BaseUnityPlugin 上不会自动调用?**会**,只要类继承 BaseUnityPlugin 且没禁用
- F1 被游戏吞了?试 F2/F3,在 ModConfig 改 ToggleKey 默认值

- [ ] **Step 6: 验证 SaveBackup 真的跑了**

```bash
ls "/c/Users/keven/AppData/LocalLow/Honestly Games/Lords and Villeins/SaveData/<SteamID>/_modbackup/"
```

期望:看到一个时间戳目录,里面有当前所有 .sgz 副本。

- [ ] **Step 7: 退出游戏,Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Plugin.cs
git commit -m "plugin: wire Core (version-check, save-backup, registry, harmony, gui)"
```

---

# Phase 5–8 — 4 个 CheatModule

> **每个模块都是同一个套路**(下方在每个 task 内重复):
> 1. 创建模块 skeleton:类、ConfigEntries、空 Register、空 DrawGui、空 DisableAll
> 2. 在 Plugin.Awake 取消注释 `Registry.Add(new Modules.XxxCheats());`
> 3. 在 GUI 里画出占位条目,可以保存/读出 ConfigEntry 值
> 4. 用 refs/0X-...md 的笔记填充 Harmony patch
> 5. install + 游戏内手动测一下(对应 docs/smoke-checklist.md 的对应条目)
> 6. Commit 单模块

下方每 task 标注模块特有的字段、patch 形态。

---

### Task 5: EconomyCheats

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Modules/EconomyCheats.cs`
- Modify: `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`(取消 Economy 那行注释)

**前置阅读:**`<PROJ>/refs/01-economy-research.md`,记下:
- `SETTLEMENT_TYPE` 全限定类名(例:`HonestlyGames.LAV.Settlement`)
- `GOLD_FIELD_NAME` / `FOOD_FIELD_NAME` / `WOOD_FIELD_NAME` / `STONE_FIELD_NAME`
- 字段是否 public(否则用 `Traverse.Create(__instance).Field(name).SetValue(v)`)
- `ECON_TICK_METHOD` 周期方法名

- [ ] **Step 1: 创建 EconomyCheats.cs(skeleton + ConfigEntries + GUI,patch 体先 stub)**

```csharp
using System;
using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class EconomyCheats : ICheatModule
    {
        public string Id   => "Economy";
        public string Name => "Economy";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // ===== Game-specific constants — fill from refs/01-economy-research.md =====
        // 在 Register 内通过 reflection 解析,以便 patch target 不存在时降级为 Broken。
        private const string SETTLEMENT_TYPE_FQN = "<FILL>";
        private const string ECON_TICK_METHOD    = "<FILL>";
        private const string GOLD_FIELD_NAME     = "<FILL>";
        private const string FOOD_FIELD_NAME     = "<FILL>";
        private const string WOOD_FIELD_NAME     = "<FILL>";
        private const string STONE_FIELD_NAME    = "<FILL>";

        // 静态共享给 Harmony patch 静态方法用
        internal static ConfigEntry<bool> LockGold  = null!;
        internal static ConfigEntry<int>  GoldValue = null!;
        internal static ConfigEntry<bool> LockFood  = null!;
        internal static ConfigEntry<int>  FoodValue = null!;
        internal static ConfigEntry<bool> LockWood  = null!;
        internal static ConfigEntry<int>  WoodValue = null!;
        internal static ConfigEntry<bool> LockStone  = null!;
        internal static ConfigEntry<int>  StoneValue = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg  = cfg;
            LockGold   = cfg.File.Bind("Economy", "LockGold",   false, "Force gold to GoldValue every tick.");
            GoldValue  = cfg.File.Bind("Economy", "GoldValue",  99999, "Target gold when LockGold is on.");
            LockFood   = cfg.File.Bind("Economy", "LockFood",   false, "Force food to FoodValue every tick.");
            FoodValue  = cfg.File.Bind("Economy", "FoodValue",  9999,  "Target food.");
            LockWood   = cfg.File.Bind("Economy", "LockWood",   false, "Force wood to WoodValue every tick.");
            WoodValue  = cfg.File.Bind("Economy", "WoodValue",  9999,  "Target wood.");
            LockStone  = cfg.File.Bind("Economy", "LockStone",  false, "Force stone to StoneValue every tick.");
            StoneValue = cfg.File.Bind("Economy", "StoneValue", 9999,  "Target stone.");

            var ok = HarmonyHelpers.SafeRun("EconomyCheats.PatchAll", () =>
            {
                var settlementType = AccessTools.TypeByName(SETTLEMENT_TYPE_FQN);
                if (settlementType == null) throw new InvalidOperationException($"Type {SETTLEMENT_TYPE_FQN} not found.");

                var tickMethod = AccessTools.Method(settlementType, ECON_TICK_METHOD);
                if (tickMethod == null) throw new InvalidOperationException($"Method {ECON_TICK_METHOD} not found on {SETTLEMENT_TYPE_FQN}.");

                var postfix = new HarmonyMethod(typeof(EconomyCheats), nameof(OnEconomyTick_Postfix));
                harmony.Patch(tickMethod, postfix: postfix);
            });

            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { /* nothing — patches are global */ }

        public void DrawGui()
        {
            DrawLockedField("Gold",  LockGold,  GoldValue);
            DrawLockedField("Food",  LockFood,  FoodValue);
            DrawLockedField("Wood",  LockWood,  WoodValue);
            DrawLockedField("Stone", LockStone, StoneValue);

            GUILayout.Space(8);
            GUILayout.Label("Quick add (one-shot):");
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+1000 Gold"))  AddTo(GOLD_FIELD_NAME, 1000);
                if (GUILayout.Button("+1000 Food"))  AddTo(FOOD_FIELD_NAME, 1000);
                if (GUILayout.Button("+1000 Wood"))  AddTo(WOOD_FIELD_NAME, 1000);
                if (GUILayout.Button("+1000 Stone")) AddTo(STONE_FIELD_NAME, 1000);
            }
        }

        public void DisableAll()
        {
            LockGold.Value  = false;
            LockFood.Value  = false;
            LockWood.Value  = false;
            LockStone.Value = false;
        }

        // -------------------------------------------------------------------

        private static void DrawLockedField(string label, ConfigEntry<bool> toggle, ConfigEntry<int> value)
        {
            using (new GUILayout.HorizontalScope())
            {
                toggle.Value = GUILayout.Toggle(toggle.Value, $"Lock {label}", GUILayout.Width(110));
                var s = GUILayout.TextField(value.Value.ToString(), GUILayout.Width(100));
                if (int.TryParse(s, out var n)) value.Value = n;
            }
        }

        private static void AddTo(string fieldName, int delta)
        {
            if (GameRefs.Settlement == null) return;
            HarmonyHelpers.SafeRun($"AddTo({fieldName})", () =>
            {
                var t = Traverse.Create(GameRefs.Settlement).Field(fieldName);
                t.SetValue(t.GetValue<int>() + delta);
            });
        }

        // -------- Harmony patches --------

        public static void OnEconomyTick_Postfix(object __instance)
        {
            if (SharedCfg.GlobalDisableAll.Value) return;
            HarmonyHelpers.SafeRun("OnEconomyTick_Postfix", () =>
            {
                var t = Traverse.Create(__instance);
                if (LockGold.Value)  t.Field(GOLD_FIELD_NAME).SetValue(GoldValue.Value);
                if (LockFood.Value)  t.Field(FOOD_FIELD_NAME).SetValue(FoodValue.Value);
                if (LockWood.Value)  t.Field(WOOD_FIELD_NAME).SetValue(WoodValue.Value);
                if (LockStone.Value) t.Field(STONE_FIELD_NAME).SetValue(StoneValue.Value);
            });
        }
    }
}
```

- [ ] **Step 2: 用 refs/01-economy-research.md 的笔记替换所有 `<FILL>` 占位**

打开 `refs/01-economy-research.md`,把 6 个 `<FILL>` 字符串值替换为研究结论中的真实类名/方法名/字段名。

> 假如某资源(例如 Stone)在游戏里不存在或叫别的名字,**临时把那段代码注释掉**并在 README "Known Limitations" 段加一行。不要 patch 一个不存在的字段 — `Traverse` 会静默失败但 `Status` 误判为 Ok。

- [ ] **Step 3: 在 Plugin.cs 取消 EconomyCheats 那行注释**

修改 `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`,把:

```csharp
// Registry.Add(new Modules.EconomyCheats());
```

改为:

```csharp
Registry.Add(new Modules.EconomyCheats());
```

- [ ] **Step 4: build + install**

```bash
dotnet build "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/src/LordsAndVilleinsCheats" -c Release
powershell -ExecutionPolicy Bypass -File "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/tools/install.ps1"
```

- [ ] **Step 5: 在游戏中冒烟测试 Economy**

启动游戏 → 加载/新建存档 → 按 F1 → 切到 Economy Tab。逐项验:

- [ ] 勾选 "Lock Gold",填 99999 → 等 30 秒,金币不掉
- [ ] 取消 "Lock Gold" → 金币恢复正常变化
- [ ] 点 "+1000 Gold" → 金币立即增加 1000
- [ ] 同样验 Food / Wood / Stone

如果 Lock 不生效但 +1000 一次性按钮生效:patch 目标方法不对,回 dnSpy 找别的 tick 方法。
如果 +1000 也不生效:GameRefs.Settlement 没拿到。回 Phase 1 Task 1.5 + Phase 5 后续 Bootstrap patch(见下文)。

- [ ] **Step 6: 加 Bootstrap patch 让 GameRefs.Settlement 真正被赋值**

在 `EconomyCheats.cs` 末尾(类外,同 namespace)加一个独立 patch 类:

```csharp
namespace LordsAndVilleinsCheats.Modules
{
    // Bootstrap patches — 用 refs/05-bootstrap-research.md 的结论填充
    [HarmonyPatch]
    internal static class GameRefsBootstrap
    {
        // 例:游戏 Settlement Awake 后赋值给 GameRefs
        [HarmonyPatch(typeof(/*<FILL_TYPE_FROM_05>*/object), "/*<FILL_METHOD_FROM_05>*/Awake")]
        [HarmonyPostfix]
        static void OnSettlementAwake_Postfix(object __instance)
        {
            LordsAndVilleinsCheats.Core.GameRefs.Settlement = __instance;
            LordsAndVilleinsCheats.Core.GameRefs.IsReady    = true;
            // 也通知 registry 触发 OnGameReady(经由 Plugin 静态字段)
            Plugin.Registry?.NotifyGameReady();
        }

        // 返主菜单/退出时清空(refs/05-bootstrap-research.md 找的"卸载"方法)
        [HarmonyPatch(typeof(/*<FILL_TYPE_FROM_05>*/object), "/*<FILL_METHOD_FROM_05>*/OnDestroy")]
        [HarmonyPostfix]
        static void OnSettlementDestroy_Postfix()
        {
            LordsAndVilleinsCheats.Core.GameRefs.Reset();
            Plugin.Registry?.ResetGameReady();
        }
    }
}
```

> 占位:把 `typeof(object)` 替换为 refs/05 中确认的 Settlement 类(用 `typeof(NS.RealClass)` 而非字符串,因为 `[HarmonyPatch]` 属性不能用 string 解析私有命名空间)。如果该类不在引用程序集里,fallback 用 `[HarmonyPatch(typeof(GameRefsBootstrap), nameof(NoopGetTarget))]` + 在 NoopGetTarget 上用 `[HarmonyTargetMethod]` 返回 `AccessTools.TypeByName(...)` 的方法。本计划倾向直接 typeof,因为 Assembly-CSharp 已通过 csproj `<Reference>` 引入。

- [ ] **Step 7: 替换 Bootstrap patch 中的占位类型/方法**

按 refs/05 笔记替换 4 个占位。

- [ ] **Step 8: build + install + 游戏内重测 Step 5 全部条目**

期望此时 Lock 与 +1000 都正常生效。

- [ ] **Step 9: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Modules/EconomyCheats.cs LordsAndVilleins/src/LordsAndVilleinsCheats/Plugin.cs
git commit -m "module: implement EconomyCheats (lock/add for gold,food,wood,stone)"
```

---

### Task 6: PawnCheats

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Modules/PawnCheats.cs`
- Modify: `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`

**前置阅读:** `<PROJ>/refs/02-pawn-research.md`,记下:
- `PAWN_TYPE_FQN`、`PAWNS_COLLECTION_PATH`(从 Settlement 拿到 IEnumerable<Pawn> 的字段路径,例如 `"pawns"` 或 `"_population.pawns"`)
- 状态字段:`HUNGER_FIELD_NAME`、`HEALTH_FIELD_NAME`、`MOOD_FIELD_NAME`(及"健康"是大值好还是小值好)
- 技能字段:`SKILLS_PATH`(可能是 `Dictionary<SkillType, int>`)

- [ ] **Step 1: 创建 PawnCheats.cs**

```csharp
using System.Collections;
using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class PawnCheats : ICheatModule
    {
        public string Id   => "Pawn";
        public string Name => "Pawn";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // ===== Fill from refs/02-pawn-research.md =====
        private const string PAWNS_COLLECTION_PATH = "<FILL>";  // 例 "pawns" 或 "population/pawns"
        private const string HUNGER_FIELD_NAME     = "<FILL>";
        private const string HEALTH_FIELD_NAME     = "<FILL>";
        private const string MOOD_FIELD_NAME       = "<FILL>";
        private const string SKILLS_PATH           = "<FILL>";  // 字段名;若 Dictionary 则 SetAllToMax 内 cast

        // 上述某些"健康"字段如果是 0–1 float,改为 1f;若是 0–100 int,改为 100。本计划默认 100/int。
        private const int   HEALTH_MAX = 100;
        private const int   MOOD_MAX   = 100;
        private const float HUNGER_MIN = 0f;        // 不饿 = 0(若反向把这值改为 100)

        internal static ConfigEntry<bool> ClearHungerOnTick = null!;
        internal static ConfigEntry<bool> ClearDiseaseOnTick = null!;
        internal static ConfigEntry<bool> MaxMoodOnTick = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg = cfg;
            ClearHungerOnTick  = cfg.File.Bind("Pawn", "ClearHunger",  false, "Each tick, set hunger to minimum on all pawns.");
            ClearDiseaseOnTick = cfg.File.Bind("Pawn", "ClearDisease", false, "Each tick, set health to max on all pawns.");
            MaxMoodOnTick      = cfg.File.Bind("Pawn", "MaxMood",      false, "Each tick, set mood to max on all pawns.");

            // 用 EconomyCheats 的同一个 settlement tick patch 已经覆盖了"周期回调"。
            // 我们这里不再注册自己的 Harmony patch;在 EconomyCheats.OnEconomyTick_Postfix
            // 里只走经济逻辑,人物逻辑通过单独 Update 拉取(性能可接受:几十 pawn × 每秒几次)。
            // 但更干净的做法是订阅同一个 settlement tick:加我们自己的 Postfix。
            var ok = HarmonyHelpers.SafeRun("PawnCheats.PatchAll", () =>
            {
                // 用 EconomyCheats 验证好的 settlement tick 方法
                var settlementType = AccessTools.TypeByName(EconomyCheats.SettlementTypeFqn);
                var tickMethod     = AccessTools.Method(settlementType, EconomyCheats.EconTickMethod);
                var postfix = new HarmonyMethod(typeof(PawnCheats), nameof(OnSettlementTick_Postfix));
                harmony.Patch(tickMethod, postfix: postfix);
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            ClearHungerOnTick.Value  = GUILayout.Toggle(ClearHungerOnTick.Value,  "Clear hunger every tick (all pawns)");
            ClearDiseaseOnTick.Value = GUILayout.Toggle(ClearDiseaseOnTick.Value, "Clear disease / max health (all pawns)");
            MaxMoodOnTick.Value      = GUILayout.Toggle(MaxMoodOnTick.Value,      "Max mood (all pawns)");

            GUILayout.Space(8);
            if (GUILayout.Button("Max all skills (all pawns) — one-shot")) MaxAllSkills();
        }

        public void DisableAll()
        {
            ClearHungerOnTick.Value = false;
            ClearDiseaseOnTick.Value = false;
            MaxMoodOnTick.Value = false;
        }

        // -------------------------------------------------------------------

        private static IEnumerable GetPawns()
        {
            if (GameRefs.Settlement == null) yield break;
            var pawns = Traverse.Create(GameRefs.Settlement).Field(PAWNS_COLLECTION_PATH).GetValue() as IEnumerable;
            if (pawns == null) yield break;
            foreach (var p in pawns) yield return p;
        }

        private static void MaxAllSkills()
        {
            HarmonyHelpers.SafeRun("MaxAllSkills", () =>
            {
                foreach (var pawn in GetPawns())
                {
                    var skills = Traverse.Create(pawn).Field(SKILLS_PATH).GetValue();
                    // skills 多半是 IDictionary。统一处理:
                    if (skills is IDictionary dict)
                    {
                        var keys = new System.Collections.Generic.List<object>();
                        foreach (var k in dict.Keys) keys.Add(k);
                        foreach (var k in keys) dict[k] = 100;  // 假设 int;若是 float 改为 1f
                    }
                    else if (skills is IList list)
                    {
                        for (int i = 0; i < list.Count; i++) list[i] = 100;
                    }
                }
            });
        }

        public static void OnSettlementTick_Postfix(object __instance)
        {
            if (SharedCfg.GlobalDisableAll.Value) return;
            HarmonyHelpers.SafeRun("PawnCheats.tick", () =>
            {
                if (!ClearHungerOnTick.Value && !ClearDiseaseOnTick.Value && !MaxMoodOnTick.Value) return;

                foreach (var pawn in GetPawns())
                {
                    var t = Traverse.Create(pawn);
                    if (ClearHungerOnTick.Value)  t.Field(HUNGER_FIELD_NAME).SetValue(HUNGER_MIN);
                    if (ClearDiseaseOnTick.Value) t.Field(HEALTH_FIELD_NAME).SetValue(HEALTH_MAX);
                    if (MaxMoodOnTick.Value)      t.Field(MOOD_FIELD_NAME).SetValue(MOOD_MAX);
                }
            });
        }
    }
}
```

> **依赖**:`EconomyCheats.SettlementTypeFqn` 和 `EconomyCheats.EconTickMethod` 需要在 EconomyCheats 把对应常量改成 `internal const`(原代码是 `private const`)。下一步处理。

- [ ] **Step 2: 把 EconomyCheats 的两个常量从 private 改为 internal**

修改 `<PROJ>/src/LordsAndVilleinsCheats/Modules/EconomyCheats.cs`:

```csharp
private const string SETTLEMENT_TYPE_FQN = "<FILL>";
private const string ECON_TICK_METHOD    = "<FILL>";
```

改为:

```csharp
internal const string SettlementTypeFqn = "<FILL>";
internal const string EconTickMethod    = "<FILL>";
```

并把模块内引用 `SETTLEMENT_TYPE_FQN` / `ECON_TICK_METHOD` 改为新名。

- [ ] **Step 3: 用 refs/02-pawn-research.md 替换 PawnCheats 的所有 `<FILL>`**

- [ ] **Step 4: 在 Plugin.cs 取消 PawnCheats 那行注释**

- [ ] **Step 5: build + install + 游戏内冒烟**

启动游戏 → 加载存档 → F1 → Pawn Tab。验:

- [ ] 选一村民,看其饥饿度 → 勾 "Clear hunger every tick" → 等几秒 → 饥饿降到底
- [ ] 同样验 Disease / Mood
- [ ] 点 "Max all skills" → 选任一村民 → 所有技能数值满
- [ ] 取消所有勾选 → 各值恢复自然变化

- [ ] **Step 6: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Modules/PawnCheats.cs LordsAndVilleins/src/LordsAndVilleinsCheats/Modules/EconomyCheats.cs LordsAndVilleins/src/LordsAndVilleinsCheats/Plugin.cs
git commit -m "module: implement PawnCheats (clear hunger/disease, max mood/skills)"
```

---

### Task 7: TimeCheats

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Modules/TimeCheats.cs`
- Modify: `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`

**前置阅读:** `<PROJ>/refs/03-time-research.md`,记下:
- `TIME_MGR_TYPE_FQN`、`SPEED_FIELD_NAME`(可能是 enum 档位 + `float currentMultiplier`)
- 是否需要 patch 一个 setter 才能让游戏 UI 同步显示

- [ ] **Step 1: 创建 TimeCheats.cs**

```csharp
using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class TimeCheats : ICheatModule
    {
        public string Id   => "Time";
        public string Name => "Time";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // ===== Fill from refs/03-time-research.md =====
        private const string TIME_MGR_TYPE_FQN = "<FILL>";
        private const string SPEED_FIELD_NAME  = "<FILL>";

        internal static ConfigEntry<bool>  OverrideSpeed = null!;
        internal static ConfigEntry<float> SpeedValue    = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg = cfg;
            OverrideSpeed = cfg.File.Bind("Time", "OverrideSpeed", false, "Force game speed multiplier.");
            SpeedValue    = cfg.File.Bind("Time", "SpeedValue",    1.0f,  "Speed multiplier when OverrideSpeed is on (1.0 = normal).");

            var ok = HarmonyHelpers.SafeRun("TimeCheats.PatchAll", () =>
            {
                var t  = AccessTools.TypeByName(TIME_MGR_TYPE_FQN);
                var update = AccessTools.Method(t, "Update");
                if (update == null) throw new System.Exception($"No Update on {TIME_MGR_TYPE_FQN}");
                var pf = new HarmonyMethod(typeof(TimeCheats), nameof(OnTimeUpdate_Postfix));
                harmony.Patch(update, postfix: pf);
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            OverrideSpeed.Value = GUILayout.Toggle(OverrideSpeed.Value, "Override game speed");
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Multiplier:", GUILayout.Width(80));
                var s = GUILayout.TextField(SpeedValue.Value.ToString("0.##"), GUILayout.Width(80));
                if (float.TryParse(s, out var n)) SpeedValue.Value = Mathf.Clamp(n, 0.0f, 50.0f);
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("0.5x"))  { OverrideSpeed.Value = true; SpeedValue.Value = 0.5f; }
                if (GUILayout.Button("1x"))    { OverrideSpeed.Value = true; SpeedValue.Value = 1f;   }
                if (GUILayout.Button("2x"))    { OverrideSpeed.Value = true; SpeedValue.Value = 2f;   }
                if (GUILayout.Button("5x"))    { OverrideSpeed.Value = true; SpeedValue.Value = 5f;   }
                if (GUILayout.Button("10x"))   { OverrideSpeed.Value = true; SpeedValue.Value = 10f;  }
            }
        }

        public void DisableAll() => OverrideSpeed.Value = false;

        public static void OnTimeUpdate_Postfix(object __instance)
        {
            if (SharedCfg.GlobalDisableAll.Value) return;
            if (!OverrideSpeed.Value) return;
            HarmonyHelpers.SafeRun("TimeCheats.OnTimeUpdate_Postfix", () =>
            {
                Traverse.Create(__instance).Field(SPEED_FIELD_NAME).SetValue(SpeedValue.Value);
            });
        }
    }
}
```

- [ ] **Step 2: 替换占位**

`refs/03-time-research.md` → 填 `TIME_MGR_TYPE_FQN`、`SPEED_FIELD_NAME`。如果游戏的速度字段是 int 档位而非 float,把字段类型与 `Field<...>().SetValue(...)` 调整。

- [ ] **Step 3: 在 Plugin.cs 取消 TimeCheats 那行注释**

- [ ] **Step 4: build + install + 游戏内冒烟**

- [ ] 调到 5x → 一天明显加快
- [ ] 调到 0.5x → 明显减慢
- [ ] 关掉 Override → 恢复游戏内速度按钮控制

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Modules/TimeCheats.cs LordsAndVilleins/src/LordsAndVilleinsCheats/Plugin.cs
git commit -m "module: implement TimeCheats (speed multiplier override)"
```

---

### Task 8: BuildCheats

**Files:**
- Create: `<PROJ>/src/LordsAndVilleinsCheats/Modules/BuildCheats.cs`
- Modify: `<PROJ>/src/LordsAndVilleinsCheats/Plugin.cs`

**前置阅读:** `<PROJ>/refs/04-build-research.md`,记下:
- `MATERIAL_CONSUMPTION_TYPE` + `MATERIAL_CONSUMPTION_METHOD`(返回 bool 的"试扣材料"方法,Prefix 短路)
- 可选:`UNLOCK_DATA_TYPE` + `UNLOCK_LIST_FIELD`(若想做"全建筑解锁")

- [ ] **Step 1: 创建 BuildCheats.cs**

```csharp
using System.Collections;
using BepInEx.Configuration;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using LordsAndVilleinsCheats.Util;
using UnityEngine;

namespace LordsAndVilleinsCheats.Modules
{
    public class BuildCheats : ICheatModule
    {
        public string Id   => "Build";
        public string Name => "Build";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        // ===== Fill from refs/04-build-research.md =====
        private const string MATERIAL_CONSUMPTION_TYPE   = "<FILL>";
        private const string MATERIAL_CONSUMPTION_METHOD = "<FILL>";

        internal static ConfigEntry<bool> FreeBuilding = null!;
        internal static ModConfig SharedCfg = null!;

        public void Register(ModConfig cfg, Harmony harmony)
        {
            SharedCfg = cfg;
            FreeBuilding = cfg.File.Bind("Build", "FreeBuilding", false,
                "When on, building blueprints consume zero materials.");

            var ok = HarmonyHelpers.SafeRun("BuildCheats.PatchAll", () =>
            {
                var t = AccessTools.TypeByName(MATERIAL_CONSUMPTION_TYPE);
                var m = AccessTools.Method(t, MATERIAL_CONSUMPTION_METHOD);
                if (m == null) throw new System.Exception($"No {MATERIAL_CONSUMPTION_METHOD} on {MATERIAL_CONSUMPTION_TYPE}");
                var prefix = new HarmonyMethod(typeof(BuildCheats), nameof(OnConsume_Prefix));
                harmony.Patch(m, prefix: prefix);
            });
            Status = ok ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() {}

        public void DrawGui()
        {
            FreeBuilding.Value = GUILayout.Toggle(FreeBuilding.Value, "Free building (skip material consumption)");
            GUILayout.Label("(Note: visual material display in HUD may still show cost; actual stockpile is unaffected.)");
        }

        public void DisableAll() => FreeBuilding.Value = false;

        // 短路 Prefix:跳过原方法,把 __result 设为 true(假设原方法返回 bool 表示"成功消耗")
        public static bool OnConsume_Prefix(ref bool __result)
        {
            if (SharedCfg.GlobalDisableAll.Value) return true;   // 走原方法
            if (!FreeBuilding.Value) return true;                // 走原方法
            __result = true;
            return false;                                        // 跳过原方法
        }
    }
}
```

> 如果原方法返回 void(不是 bool),改 prefix 签名为 `public static bool OnConsume_Prefix() { if (...) return false; return true; }`(只跳过,不设置返回值)。
> 如果原方法签名带了 out 参数(例:`bool TryConsume(out int actuallyConsumed)`),把 `out int actuallyConsumed` 也加进 Prefix 签名并赋 0。

- [ ] **Step 2: 替换占位**

`refs/04-build-research.md` → `MATERIAL_CONSUMPTION_TYPE` + `MATERIAL_CONSUMPTION_METHOD`。根据原方法真实签名调整 Prefix 签名(见上方 note)。

- [ ] **Step 3: 在 Plugin.cs 取消 BuildCheats 那行注释**

- [ ] **Step 4: build + install + 游戏内冒烟**

- [ ] 关闭 FreeBuilding → 正常造一个建筑,材料按预期扣除
- [ ] 开启 FreeBuilding → 再造一个,材料不扣

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/src/LordsAndVilleinsCheats/Modules/BuildCheats.cs LordsAndVilleins/src/LordsAndVilleinsCheats/Plugin.cs
git commit -m "module: implement BuildCheats (free material consumption)"
```

---

# Phase 9 — 工具脚本

### Task 9: tail-log.ps1 + run-and-check.ps1

**Files:**
- Create: `<PROJ>/tools/tail-log.ps1`
- Create: `<PROJ>/tools/run-and-check.ps1`

- [ ] **Step 1: 创建 tail-log.ps1**

```powershell
param(
    [string]$GameRoot = $env:LAV_GAME_ROOT
)
if (-not $GameRoot) { $GameRoot = "E:\SteamLibrary\steamapps\common\Lords & Villeins" }

$log = Join-Path $GameRoot "BepInEx\LogOutput.log"
if (-not (Test-Path $log)) { throw "Log file not found: $log. Launch the game once first." }

Write-Host "Tailing $log (Ctrl+C to stop)..." -ForegroundColor Cyan
Get-Content -Path $log -Wait -Tail 30
```

- [ ] **Step 2: 创建 run-and-check.ps1**

```powershell
param(
    [string]$GameRoot = $env:LAV_GAME_ROOT,
    [int]$WaitSeconds = 30
)
if (-not $GameRoot) { $GameRoot = "E:\SteamLibrary\steamapps\common\Lords & Villeins" }

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$exe      = Join-Path $GameRoot "Lords and Villeins.exe"
$log      = Join-Path $GameRoot "BepInEx\LogOutput.log"

# 先 install
& "$PSScriptRoot\install.ps1" -GameRoot $GameRoot
if ($LASTEXITCODE -ne 0) { throw "install.ps1 failed." }

# 清空旧 log(改名,以保留历史)
if (Test-Path $log) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    Move-Item $log "$log.$stamp.bak" -Force
}

# 启动游戏
$proc = Start-Process -FilePath $exe -PassThru
Write-Host "Game launched (PID $($proc.Id)). Waiting $WaitSeconds seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds $WaitSeconds

# 杀进程
if (-not $proc.HasExited) {
    Stop-Process -Id $proc.Id -Force
    Write-Host "Game terminated." -ForegroundColor Cyan
}

# 解析日志
if (-not (Test-Path $log)) { throw "Log file was not created: $log" }
$content = Get-Content $log -Raw

$summaryLine = ($content -split "`n") | Where-Object { $_ -match "Patch summary: (\d+)/(\d+) ok, (\d+) broken" } | Select-Object -First 1
$errorCount  = ([regex]::Matches($content, "\[Error|\[Fatal")).Count

Write-Host ""
Write-Host "=== run-and-check report ===" -ForegroundColor Yellow
if ($summaryLine) { Write-Host "Found: $summaryLine" } else { Write-Host "Patch summary line NOT found — plugin may not have loaded." -ForegroundColor Red }
Write-Host "Error/Fatal log lines: $errorCount"

if (-not $summaryLine -or $errorCount -gt 0) {
    Write-Host "RESULT: FAIL" -ForegroundColor Red
    exit 1
}
if ($summaryLine -match "Patch summary: (\d+)/(\d+) ok, (\d+) broken" -and $matches[3] -gt 0) {
    Write-Host "RESULT: WARN (broken patches present)" -ForegroundColor Yellow
    exit 2
}
Write-Host "RESULT: PASS" -ForegroundColor Green
exit 0
```

- [ ] **Step 3: 跑一遍 run-and-check.ps1 验证**

```bash
powershell -ExecutionPolicy Bypass -File "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/tools/run-and-check.ps1"
```

期望:输出 `RESULT: PASS`,exit code 0。

- [ ] **Step 4: 跑一遍 tail-log.ps1 确认能跟看**

```bash
powershell -ExecutionPolicy Bypass -File "C:/Users/keven/Desktop/gamecheats/LordsAndVilleins/tools/tail-log.ps1"
# Ctrl+C 退出
```

- [ ] **Step 5: Commit**

```bash
git add LordsAndVilleins/tools/tail-log.ps1 LordsAndVilleins/tools/run-and-check.ps1
git commit -m "tools: add tail-log and run-and-check scripts"
```

---

# Phase 10 — README + 冒烟清单 + 终验

### Task 10: README + 冒烟清单 + 全量回归

**Files:**
- Create: `<PROJ>/README.md`
- Create: `<PROJ>/docs/smoke-checklist.md`

- [ ] **Step 1: 创建 README.md**

```markdown
# Lords & Villeins Cheats (BepInEx mod)

In-game cheat panel for Lords & Villeins. Modular, IMGUI, F1 to toggle.

> Personal use first. Code is structured for future release; UI is currently English only.

## Install

1. Download BepInEx 5.4.21 (x64) from https://github.com/BepInEx/BepInEx/releases and extract into the game folder so `winhttp.dll` sits next to `Lords and Villeins.exe`.
2. Launch the game once (BepInEx will create its folders, then exit).
3. Build this project: `dotnet build -c Release`
4. Run `tools/install.ps1` — copies the plugin DLL to `BepInEx/plugins/LordsAndVilleinsCheats/`.
5. Launch the game. Press **F1** to toggle the cheat panel.

## Layout

- `src/LordsAndVilleinsCheats/` — the mod
- `src/LordsAndVilleinsCheats.Tests/` — xUnit tests for pure logic
- `tools/` — install / log-tail / run-and-check PowerShell helpers
- `docs/smoke-checklist.md` — manual smoke list to run before release
- `docs/superpowers/specs/` and `docs/superpowers/plans/` — design + implementation plan

## Develop

```bash
dotnet build -c Release
powershell tools/install.ps1
powershell tools/tail-log.ps1   # in a separate terminal
# launch game via Steam
```

After a code change: `dotnet build -c Release && powershell tools/install.ps1`, then restart game.

## Tests

```bash
dotnet test
```

## Tested game version

See `refs/06-version-research.md` (refs/ is git-ignored — only present on dev machines).

## Known limitations

- Game updates may break Harmony patches; check `BepInEx/LogOutput.log` for "Patch summary".
- Mods that change save format are out of scope. This mod only writes runtime values.
- Steam achievements are not protected; this mod does not claim "achievement-safe" mode.

## License

Personal use, no warranty.
```

- [ ] **Step 2: 创建 docs/smoke-checklist.md**

```markdown
# 冒烟清单 — Lords & Villeins Cheats

每次 release 候选前过一遍。建议在干净存档(或备份过的存档)上跑。

## 准备

- [ ] `dotnet test` 全绿
- [ ] `dotnet build -c Release` 无错
- [ ] `powershell tools/install.ps1` 完成
- [ ] `powershell tools/run-and-check.ps1` 输出 `RESULT: PASS`(自动校验加载)

## Loader

- [ ] 干净存档加载,无 mod 行为干扰(全部 Lock 默认 OFF)
- [ ] BepInEx LogOutput.log 显示 `Patch summary: N/N ok, 0 broken`

## Economy

- [ ] Lock Gold ON,值 99999 → 等 1 分钟金币不变
- [ ] +1000 Gold 一次性按钮 → 数额准确
- [ ] Lock Food / Wood / Stone 同样测一遍

## Pawn

- [ ] 选一村民 → Clear hunger ON → 几秒内饥饿降到底
- [ ] 选一村民 → Clear disease ON → 健康满
- [ ] Max all skills 一次性按钮 → 选任一村民确认所有技能值=100
- [ ] Max mood ON → 心情满

## Time

- [ ] 速度 ×10 → 一天用时明显缩短(对照游戏内时钟)
- [ ] 速度 ×0.5 → 明显变慢
- [ ] Override OFF → 恢复游戏内速度按钮控制

## Build

- [ ] FreeBuilding OFF → 造一个建筑,材料正常扣
- [ ] FreeBuilding ON → 造一个建筑,材料不扣

## Disable All / 持久化

- [ ] 顶部红色按钮 "Disable All" → 上述一切立即停止
- [ ] 重启游戏 → 之前在面板里设置的值(GoldValue 等)全部恢复
- [ ] BepInEx/config/com.kk.lav-cheats.cfg 内容与 GUI 上次状态一致

## 存档不损坏

- [ ] 开 mod 玩 5 分钟 → 保存
- [ ] 关游戏
- [ ] 把 `<PLUGIN_DIR>` 整个删除(模拟卸载)
- [ ] 启动游戏读同一存档 → 能正常加载,游戏行为无残留作弊
```

- [ ] **Step 3: 把 README + smoke-checklist 跑一遍**

按 `docs/smoke-checklist.md` 的所有勾选项过一遍。任何 FAIL → 修复 → 重提。

- [ ] **Step 4: Commit + merge 回主分支**

```bash
cd "C:/Users/keven/Desktop/gamecheats"
git add LordsAndVilleins/README.md LordsAndVilleins/docs/smoke-checklist.md
git commit -m "docs: add README and smoke checklist"

# 如果你之前是在 feat/initial-build 分支:
git checkout main
git merge --no-ff feat/initial-build -m "feat: initial Lords & Villeins cheats v0.1.0"
```

---

## 验收标准对照(spec §12 复核)

执行计划完成后逐项确认:

- [ ] 全新机器按 README 步骤,30 分钟内能装起、F1 弹面板、四类作弊各至少 1 个开关可用
- [ ] §9.2 冒烟清单全过(对应本计划 Task 10 Step 3)
- [ ] `dotnet test` 全绿
- [ ] 卸载 mod 后游戏完全恢复原版行为(冒烟清单"存档不损坏"段)
- [ ] 存档与原版双向兼容(同上)

---

## 失败回退

如果某模块在游戏内完全跑不通:
1. 在 GUI 顶部点 "Disable All",停止所有锁定行为
2. 退出游戏
3. 把 `<PLUGIN_DIR>\LordsAndVilleinsCheats.dll` 移走
4. 启动游戏验证恢复原版
5. 把存档从 `_modbackup/<最新时间戳>/` 复制回 `<save dir>` 覆盖

如果游戏完全启不来(BepInEx 把游戏弄坏了 — 极罕见):
1. 删掉 `<GAME>\winhttp.dll`(就完全卸载了 BepInEx 注入)
2. 游戏立刻恢复原版

---

## 计划自审清单(完成于编写时)

- [x] Spec 覆盖:§1–§12 每一节都有对应 task
- [x] No placeholders:所有 `<FILL>` 都标了来源 refs/0X-X.md;所有 TODO/TBD 已删
- [x] 类型一致性:`ModConfig.File`、`Plugin.Registry`、`EconomyCheats.SettlementTypeFqn` 等命名跨 task 一致
- [x] 测试-实现-验证-提交 节奏在每个 Core/Module task 完整
- [x] 安全红线(spec §8)落地:save backup 在 patch 前(Plugin.Awake)、默认全 OFF(每个 ConfigEntry 默认值)、Disable All(GuiManager + ModuleRegistry)、不碰成就(plan 不引用任何 achievement API)、log 汇总(Plugin.LogPatchSummary)
