---
title: Lords & Villeins 修改器 — 设计文档
date: 2026-04-22
status: implemented (v0.1.0, 2026-04-22)
audience: 实施计划编写 (writing-plans 阶段)
followup: 当前进度与下一版规划见 ../../../../ROADMAP.md
---

# Lords & Villeins 修改器 — 设计文档

## 1. 背景与目标

为单机游戏 *Lords & Villeins*(Steam 2943600,Honestly Games)开发一个游戏内修改器(以下简称 **mod**),作为个人使用工具,代码质量按"未来可发布"的标准来写,但 UI 文案双语、详尽 README 等发布层物料延后再补。

**功能范围(全部 4 类):**

1. 资源/经济作弊 — 金币、食物、木材、石材等的实时锁定与一次性增发
2. 人物属性/技能作弊 — 村民/领主属性提升、技能学满、清除饥饿/疾病/低心情
3. 时间/天气/事件控制 — 游戏速度倍率、季节快进、事件触发或屏蔽
4. 建造/生产作弊 — 解锁全建筑、跳过建造材料、生产倍率

## 2. 游戏环境调研结论

| 项目 | 值 |
|---|---|
| Steam App ID | 2943600 |
| 游戏目录 | `E:\SteamLibrary\steamapps\common\Lords & Villeins` |
| 引擎 | Unity,**Mono 后端**(关键,意味着 Harmony patch 直接可用,非 IL2CPP) |
| 主程序集 | `Lords and Villeins_Data/Managed/Assembly-CSharp.dll` |
| 序列化栈 | `Newtonsoft.Json` + `OdinSerializer` |
| 已知 mod loader | **无**(未安装 BepInEx / MelonLoader / UnityModManager) |
| 存档目录 | `%USERPROFILE%\AppData\LocalLow\Honestly Games\Lords and Villeins\SaveData\<SteamID>\` |
| 存档格式 | `.sgz`(扩展名暗示 gzip + 序列化二进制,本 mod 不解析,只备份) |
| 配置目录 | `%USERPROFILE%\AppData\LocalLow\Honestly Games\Lords and Villeins\ConfigData\<SteamID>\config.ini` |

## 3. 决策汇总(brainstorming 阶段确认)

| 维度 | 决策 | 排除项 |
|---|---|---|
| 形态 | 游戏内 mod,BepInEx 5 + HarmonyX | 外部 trainer / 纯存档编辑器 |
| 功能 | 4 大类全做 | 仅单类(范围太窄) |
| UI | Unity IMGUI(F1 总面板 + Tab 切换) | UGUI 原生风格(工作量 3-5 倍) / 纯热键(失去动态调参) |
| 受众 | 自用为主,代码质量按发布写,文档/双语延后 | 强发布(过早投入文案) / 极简自用(架构会失控) |
| 代码组织 | 模块化:Core + 4 个独立 CheatModule | 单文件大 Plugin(方案 A,会失控)/ 数据驱动反射框架(方案 C,过度工程) |
| 开发环境 | 已具备 .NET SDK + IDE + dnSpy,无需环境准备步骤 | — |

## 4. 架构

### 4.1 项目结构

```
gamecheats/
└── LordsAndVilleins/
    ├── README.md                            # 自用阶段精简,后期补
    ├── LordsAndVilleins.sln
    ├── .gitignore                           # bin/ obj/ refs/ *.dll
    ├── src/
    │   ├── LordsAndVilleinsCheats/
    │   │   ├── LordsAndVilleinsCheats.csproj
    │   │   ├── Plugin.cs                    # BepInEx 入口 [BepInPlugin]
    │   │   ├── Core/
    │   │   │   ├── ICheatModule.cs          # 模块接口
    │   │   │   ├── ModuleRegistry.cs        # 启停、热键收发
    │   │   │   ├── GuiManager.cs            # F1 总面板 + Tab
    │   │   │   ├── GameRefs.cs              # 缓存游戏单例引用
    │   │   │   └── ModConfig.cs             # BepInEx ConfigFile 包装
    │   │   ├── Modules/
    │   │   │   ├── EconomyCheats.cs
    │   │   │   ├── PawnCheats.cs
    │   │   │   ├── TimeCheats.cs
    │   │   │   └── BuildCheats.cs
    │   │   └── Util/
    │   │       ├── HarmonyHelpers.cs        # SafePatch 等
    │   │       └── SaveBackup.cs            # 启动时自动备份
    │   └── LordsAndVilleinsCheats.Tests/    # xUnit,仅纯逻辑层
    ├── refs/                                # 反编译笔记 / 抠出的方法签名,git 忽略
    ├── tools/
    │   ├── install.ps1                      # 把构建产物推到游戏 BepInEx/plugins
    │   ├── tail-log.ps1                     # 跟看 LogOutput.log
    │   └── run-and-check.ps1                # 启动游戏 + 解析 log 校验加载
    └── docs/
        ├── smoke-checklist.md               # 手动冒烟清单
        └── superpowers/
            └── specs/
                └── 2026-04-22-lords-villeins-modder-design.md   # 本文件
```

### 4.2 构建配置

- **目标框架**:`netstandard2.1`(Unity Mono / BepInEx 5 推荐)
- **依赖**:
  - NuGet: `BepInEx.Core` 5.4.21、`HarmonyX` 2.10.x
  - HintPath 引用(不入仓): `UnityEngine.CoreModule.dll`、`UnityEngine.IMGUIModule.dll`、`Assembly-CSharp.dll` — 全部从游戏 `Lords and Villeins_Data/Managed/` 引用
  - 测试项目: `xunit` + `xunit.runner.visualstudio`,target `net8.0`
- **PostBuild**: `LordsAndVilleinsCheats.dll` 拷贝到 `bin/Release/plugin/`,`tools/install.ps1` 再推送到游戏目录
- **`.gitignore`** 强制排除 `bin/` `obj/` `refs/` `**/*.dll`,防止把 NuGet 与游戏 DLL 提交

### 4.3 模块接口契约

```
ICheatModule
  ├─ string Name { get; }                         // Tab 名
  ├─ string Id   { get; }                         // 配置键前缀
  ├─ ModuleStatus Status { get; }                 // OK / Broken(patch 失败)/ Disabled
  ├─ void Register(ModConfig cfg, Harmony harmony)
  ├─ void OnGameReady()                           // GameRefs.IsReady 转为 true 时回调一次
  ├─ void DrawGui()                               // 该 Tab 选中时调用
  └─ void DisableAll()                            // "Disable All" 红按钮调用
```

## 5. 数据流

### 5.1 一次"锁金币 99999"完整链路

```
[F1 弹面板]
  → GuiManager.OnGUI() → 切到 Economy Tab
    → EconomyCheats.DrawGui()
      ├─ TextField  "目标金币" → cfg.GoldTarget = 99999      (BepInEx ConfigEntry<int>,自动持久化)
      └─ Toggle     "锁定金币" → cfg.LockGold   = true        (BepInEx ConfigEntry<bool>,自动持久化)

[游戏 tick]
  → Settlement.UpdateEconomy()                              (类名占位,实施期 dnSpy 确认)
    └─ Harmony Postfix: EconomyCheats.OnEconomyTick_Postfix
        if (cfg.LockGold) __instance.gold = cfg.GoldTarget;
```

### 5.2 三种修改手法(模块按需混用)

| 手法 | 用法 | 例子 |
|---|---|---|
| Harmony **Postfix 覆写字段** | 持续锁定一个数值 | 锁金币、锁食物 |
| Harmony **Prefix 短路** | 拦截扣费/检查逻辑,`return false` 跳过原方法 | 建造不消耗资源 |
| **直接调单例方法** | 一次性触发 | "+1000 金"按钮、"立刻清饥饿"按钮 — 通过 `GameRefs` 拿单例直接写 |

### 5.3 GameRefs 角色

启动时游戏单例尚未初始化,通过对游戏 Bootstrap 类的 `Awake/Start` 加 Postfix 抓取并缓存:

```csharp
[HarmonyPatch(typeof(SomeBootstrapClass), "Start")]
static void Postfix(SomeBootstrapClass __instance) {
    GameRefs.Settlement = __instance.GetSettlement();
    GameRefs.IsReady = true;
}
```

`ModuleRegistry` 每帧渲染前检查 `GameRefs.IsReady`,未就绪时模块逻辑全部跳过,UI 只显示"等待游戏加载…"。

### 5.4 配置持久化

使用 BepInEx `ConfigEntry<T>` 而非自写 JSON,理由:

- 自动保存与重载,F5 在游戏内立即生效
- 兼容第三方 BepInEx ConfigurationManager,等于免费多一个备用 UI
- 无需自己处理路径、IO 异常、版本迁移

存储路径:`BepInEx/config/com.kk.lav-cheats.cfg`

## 6. 启动顺序

```
1. Steam 启动游戏
2. winhttp.dll (BepInEx doorstop) 注入 → BepInEx 初始化
3. BepInEx 加载 plugins/LordsAndVilleinsCheats/LordsAndVilleinsCheats.dll
4. Plugin.Awake():
     ├─ 读取 Application.version,与已知兼容版本白名单对比 → 不匹配仅 log warn,继续
     ├─ ModConfig 初始化(ConfigEntry 自动从 .cfg 还原上次设置)
     ├─ SaveBackup.RunOnce()
     │     └─ 复制 SaveData/<SteamID>/*.sgz → SaveData/<SteamID>/_modbackup/<时间戳>/
     │        最多保留 5 份,超出删最旧
     ├─ ModuleRegistry 注册所有模块(空操作,只登记)
     └─ Harmony.PatchAll() — 一次性挂载所有 patch,记录 success/fail
5. 游戏 Awake/Start 链:GameRefs 通过 patch 抓取单例 → IsReady = true
6. 模块 OnGameReady 回调
7. F1 弹出总面板可用
```

## 7. 错误处理

| 故障层 | 策略 |
|---|---|
| **Patch target 不存在**(游戏更新后方法改名) | `HarmonyHelpers.SafePatch` 包 try/catch,失败的 patch 跳过并 log,所属模块 `Status = Broken` 在 UI 灰显;**绝不让 Plugin.Awake 抛异常** |
| **Patch 内部异常** | 每个 Prefix/Postfix body 顶层 try/catch,异常仅 log,不抛 |
| **GameRefs 未就绪时被调用** | 模块 `DrawGui()` 入口 `if (!GameRefs.IsReady) { GUILayout.Label("等待游戏加载…"); return; }` |
| **用户输入非法值** | TextField 用 `int.TryParse`,失败保留旧值,旁边红字小提示 |
| **存档备份失败** | log warn,首次面板顶部一次性提示"备份失败,作弊功能仍可用,但出问题无法回滚",用户可继续 |

## 8. 安全红线

1. **第一次启动游戏永远先备份**(在 Patch 之前),即使本次未启用任何作弊
2. **默认所有作弊开关 OFF** — config 不存在时,加载即原版体验
3. **F1 面板顶部红色 "Disable All" 按钮** — 一键将所有 Lock 类置 false,效果立即停止(不解 patch,但行为消失)
4. **不碰 Steam 成就相关代码** — 即使已知怎么改,这块完全不动
5. **崩溃日志** — `BepInEx/LogOutput.log` 自带,Plugin.Awake 末尾额外打 patch success/fail 汇总表,便于截图反馈

## 9. 测试策略

### 9.1 三层金字塔

**层 1 — 单元测试 (xUnit)**:仅纯逻辑,不依赖 UnityEngine / Assembly-CSharp。

- `SaveBackup` 备份轮转(临时目录 7 文件 → 5 文件)
- `ModConfig` 边界值(负数、`int.MaxValue`)
- `ModuleRegistry` 状态机(`DisableAll` 后所有模块状态)
- `HarmonyHelpers.SafePatch` 失败路径

测试项目放 `src/LordsAndVilleinsCheats.Tests/`,target `net8.0`(与 mod 本体 netstandard2.1 解耦)。

**层 2 — 半自动启动检查 (`tools/run-and-check.ps1`)**:

1. 调 `install.ps1` 推送最新构建
2. 启动游戏,等 30 秒
3. 解析 `BepInEx/LogOutput.log`:`[Plugin] Patch summary: N/N success` + 无 `[Error]/[Fatal]`
4. 杀进程,报告结果

**层 3 — 手动冒烟清单 (`docs/smoke-checklist.md`)**:每次 release 候选前过一遍,见 §9.2。

### 9.2 冒烟清单条目

- [ ] 干净存档加载,无 mod 行为干扰
- [ ] Economy: 锁金币 ON → 1 分钟内不变
- [ ] Economy: +1000 金按钮 → 数额准确
- [ ] Pawn: 选村民 → 拉满技能 → 立即体现
- [ ] Time: 速度 ×10 → 一天用时缩短
- [ ] Build: 跳过建造材料 → 蓝图直接落实体
- [ ] Disable All → 上述效果立即停止
- [ ] 重启游戏 → config 持久化设置自动恢复
- [ ] **存档不损坏校验**:开 mod 玩 → 保存 → 关游戏卸 mod → 原版能正常加载存档

### 9.3 不做的事

- ❌ 游戏内集成测试(Unity Test Framework 仅 Editor 内可用,无源码做不了)
- ❌ Mock UnityEngine 类型(易写出"过测试但游戏崩"的假象)
- ❌ 追求覆盖率指标
- ❌ CI(自用阶段不上,本地 `dotnet test` + `run-and-check.ps1` 足够)

## 10. Non-goals(明确不做)

- 离线存档编辑(.sgz 解析)
- 外部 trainer(内存扫描)
- 数据驱动反射框架(方案 C,过度工程)
- IL2CPP 兼容层(本游戏不需要)
- 多语言 UI(自用阶段中文够用,英文延后)
- 自动游戏更新检测与 mod 自更新
- Steam 成就守护逻辑(完全不碰成就相关代码)

## 11. 实施期需调研事项

以下条目设计阶段无法定稿,实施第一步需用 dnSpy 反编译 `Assembly-CSharp.dll` 确认:

1. **游戏经济类的真实类名 / 字段名**:`Settlement.gold` / `UpdateEconomy()` 均为占位
2. **村民/领主对象的访问入口**(单例?子系统?哪个 Bootstrap 类负责注入)
3. **时间系统的速度乘数字段**(可能在 `TimeManager`-like 类内)
4. **建造系统的材料消耗钩子点**(蓝图落地、阶段性消耗、完成回调)
5. **游戏版本兼容白名单初始值**:取当前安装版本 `Application.version` 作为已知兼容
6. **是否要主动集成 BepInEx ConfigurationManager**:暂不强依赖,仅文档建议用户自行装

实施计划(writing-plans 阶段)的第一阶段任务必须覆盖以上调研。

## 12. 验收标准(整体)

- [ ] 全新机器按 README 步骤,30 分钟内能装起、F1 弹面板、四类作弊各至少 1 个开关可用
- [ ] §9.2 冒烟清单全过
- [ ] `dotnet test` 全绿
- [ ] 卸载 mod 后游戏完全恢复原版行为(BepInEx 卸载 = 删 winhttp.dll + BepInEx 目录)
- [ ] 存档与原版双向兼容(开 mod 存 → 卸 mod 读 OK)
