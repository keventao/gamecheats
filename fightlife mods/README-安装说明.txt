FightLife Vanguard 修改器 — Windows 安装说明
=========================================

功能：
  1. Heal Team (K 热键)  — 全队满血 + 满防
  2. +9999 Gold          — 金币 +9999（可连点叠加）
  3. 3x Speed            — 切换 3 倍速（再点关闭）
  4. 3x Damage           — 切换我方 3 倍伤害（再点关闭）
  右下角常驻 UI。

前置：
  游戏必须为 Unity Mono 后端（有 FightLife Vanguard_Data/Managed 目录）。
  本 mod 在 Unity 6000.2.6f2 版本下验证通过。其他版本可能因字段名变化失效。

安装步骤：

1. 关闭游戏。

2. 进入游戏安装目录下的 FightLife Vanguard_Data\Managed\
   - 若存在 CheatMenu.dll，备份为 CheatMenu.dll.original.bak
   - 将本文件夹的 CheatMenu.dll 复制进去

3. 进入游戏目录 FightLife Vanguard_Data\
   - 备份 RuntimeInitializeOnLoads.json → RuntimeInitializeOnLoads.json.bak
   - 用本文件夹的 RuntimeInitializeOnLoads.json 覆盖

4. 若你的游戏版本 ScriptingAssemblies.json 里已经有 CheatMenu.dll（原游戏已有），
   无需替换；若没有，用本文件夹的 ScriptingAssemblies.json 覆盖。
   快速检查：用记事本打开 ScriptingAssemblies.json，搜索 CheatMenu 有无出现。

5. 启动游戏。右下角出现 4 个按钮即为成功。

卸载：
  把 CheatMenu.dll.original.bak 改回 CheatMenu.dll，
  把 RuntimeInitializeOnLoads.json.original.bak 改回 RuntimeInitializeOnLoads.json。

注意事项：
  - 3x Damage 只对点 ON 时已在场的我方单位生效；
    读档、新关卡、新单位进场可能恢复基础伤害，需再切 OFF/ON 重应用。
  - 3x Speed 走 Time.timeScale=3；暂停菜单可能把它重置，需再按一次。
  - Heal 只治"当前 HP < 基础 HP"的我方非 NPC 单位；
    死亡（CurHP=0）单位不复活；
    已超出基础上限（被 buff 过）的单位保留超量血，不降回基础值。

日志查看（出问题时）：
  Windows: %USERPROFILE%\AppData\LocalLow\StartImpulse\FightLife Vanguard\Player.log
  在文件里搜 "[CheatMenu]" 看输出。
