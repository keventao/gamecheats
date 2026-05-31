# 轮回修仙路 — Handoff (v0.0.3 → in-game smoke)

> 上一阶段：作弊面板 + 4 模块已实现并合并到 `main`(代码完成,编译/单测绿)。
> 本阶段(Windows / opencode)：**真机跑 smoke-checklist,确认行为**。游戏内行为尚未验证。

## 跑起来

```powershell
git pull                                  # 取 main 的 v0.0.3
cd "<STEAM>\steamapps\common\轮回修仙路"   # 或你的游戏路径
# 在仓库里:
cd <repo>\轮回修仙路\src\LunHuiCheats
$env:LUNHUI_GAME_ROOT = "<STEAM>\steamapps\common\轮回修仙路"
dotnet build -c Release
powershell ..\..\tools\install.ps1
# Steam 启动游戏,进存档(到游戏世界),按 P 开面板
```

## 跑什么

照 `轮回修仙路/docs/smoke-checklist.md` 逐项勾。面板:分类侧栏(战斗/角色/背包/修为/通用/调试)+ 顶部搜索/排序。

**重点验(最可能不工作,按序):**
1. **背包列出物品** — 选「背包」分类,列表是否填充。空=IL2CPP `Count`/`Item` 反射迭代或 `All*` 字段名错。
2. **背包 by-id 添加** — 输 ID + 「尝试添加」。`Activator.CreateInstance(BaseRewardData)` 在 IL2CPP 下大概率失败 → 看 LogOutput.log,**只要不崩**即可。
3. **修为 写入** — 「道心」应 ✓ 生效;**「经验/等级」预期 ✗**(只读属性)。若也 ✓ 更好。
4. **GodMode 锁血** — 开关后受击 curHp 是否保持=maxHp(多 CharacterData 实例时可能锁错对象)。
5. **PlayerStats** — 改物攻/法攻/移速/飞行 + 锁定是否生效。

**先备份存档**再测改经验/等级/道心(插件启动会自动备份,但手动也备一份)。

## 回报

- 哪些 checklist 项 ✅ / ❌。
- ❌ 的:贴 `BepInEx\LogOutput.log` 相关行。
- 若某游戏字段/方法名不对(反射查不到 → 模块显 `(!)` 或日志报 null),记下真实名字,我据此修(别猜)。
- 背包列表若为空:把 `All*` 字段在该游戏版本的真实情况说一下。

## 已知 caveat(解读结果用)

- 经验/等级是 `{get;}` 只读 → 写失败是预期,真写入路径可能是 `MySkillLib.AddExp`,待定。
- 灵根目前只读显示,编辑留二期。
- 单测里 3 个失败(SaveBackup×2 + RegisterAll)只因非游戏机缺 Unity runtime,Windows 上应过 —— 不是 bug。

参考:`docs/superpowers/specs/` 设计、`docs/superpowers/plans/` 实现计划、`refs/01-discovered-types-summary.md` 类型表。
