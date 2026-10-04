# ZEDZONE Cheats（隔离区-丧尸末日生存）

BepInEx 6 IL2CPP 修改器：F1 呼出 uGUI 面板，血量/耐力锁定 + 全物品库浏览添加。

- 游戏：Steam AppID 1211600，Unity 2023.1.18f1 IL2CPP x64（BETA 0.9.99.7 实测）
- 加载器：BepInEx 6.0.0-be.733（`winhttp.dll` 注入，`BepInEx/Plugins/`）
- 构建：`dotnet build -c Release`（需 .NET 6 SDK），见 `Directory.Build.props` 的 `GameRoot` 约定

## 功能（v0.4.7）

- 血量锁定：每帧 `hp` 回满到 `maxhp`，`maxhp` 只读（升级/加点涨上限不受影响），面板可开关
- 耐力/疲劳锁定：`energy→maxEnergy`、`fatigue→0`，面板可开关
- 物品添加：`ItemManager.instance.itemAttrDic` 全枚举（滤 `hiddenItem`），17 分类 + 中文搜索 + 真滚动列表，点击按 `stackNumber` 最大堆叠入库（走游戏原生 `GameController.AddItemToPlayer`）
- 面板：F1 开关，深色可拖动，中文（游戏自带思源黑体直赋）

## 目录

- `src/ZedZoneHpLock/` — 插件源码（`Core` 尚未拆分，见 ROADMAP）
- `docs/smoke-checklist.md` — v0.1.0→v0.4.7 完整验证记录（含 Jim97 样本逆向、两次加载失败根因、IMGUI 裁剪探针、字体链、uGUI 转换坑）
- `refs/api.md` — 游戏 API 访问链、补丁目标表、被裁 API 清单
- `tools/install.ps1` — 编译产物拷进游戏 `Plugins/`（游戏运行时锁文件，需退出后覆盖）

## 状态与下一步

见 `ROADMAP.md`。
