# 冒烟清单 — Lords & Villeins Cheats

每次 release 候选前过一遍。建议在干净存档(或备份过的存档)上跑。

## 准备

- [ ] `dotnet test` 全绿(10 passed, 1 skipped 是当前正常状态)
- [ ] `dotnet build -c Release` 无错
- [ ] `powershell tools/install.ps1` 完成
- [ ] `powershell tools/run-and-check.ps1` 输出 `RESULT: PASS`(自动校验 mod 加载与 patch summary)

## Loader

- [x] BepInEx LogOutput.log 显示 `Patch summary: 4/4 ok, 0 broken`(2026-04-25)
- [x] **进存档**后 F1 弹出灰色面板(2026-04-25)
- [ ] 主菜单 F1 ⛔ 不可用 — 这游戏 Player Loop 不调度 BepInEx GameObject,我们寄生在 `GameManager.gameObject`,主菜单 GameManager 还没实例化所以 F1 此时无效。规划在 v0.2 通过 hook `MainMenu`/`LoadingScreen` 类作为 fallback host
- [ ] 顶栏 "Disable All" 按钮 — v0.1 自用阶段 Economy panel 未重显该控件;Time/Pawn/Build tab 仍可勾选具体 toggle

## Economy(2026-04-25 实测)

- [x] +100000 Money 按钮 → Money 数额实时增长,无回归
- [x] +1000 Food 按钮 → Food (Grain) 数额实时增长
- [ ] Wood / Stone — ⛔ 不支持,见 ROADMAP "已知限制"。`Inventory.AddResource` 在玩家所有 personal inventory 全 reject(`allowedResources` 不含)
- [ ] Lock 路径 — v0.1 panel 已删该 UI(自用阶段 user 只需要 +N 一次性按钮)。代码保留,v0.2 真锁需要补 `Inventory.SpendResources` patch

## Pawn(待测)

- [ ] 选一村民 → Clear hunger ON → 几秒内饥饿降到底
- [ ] 选一村民 → Clear disease ON → 健康满
- [ ] Max all skills 一次性按钮 → 选任一村民确认所有技能值=100
- [ ] Max mood ON → 心情满

## Time(2026-04-25 实测)

- [x] 速度 override → 游戏速度被实测覆写
- [ ] 速度 ×0.5 → 明显变慢
- [ ] Override OFF → 恢复游戏内速度按钮控制

## Build(待测)

- [ ] FreeBuilding OFF → 造一个建筑,材料正常扣
- [ ] FreeBuilding ON → 造一个建筑,材料不扣

## Disable All / 持久化

- [ ] 顶部红色按钮 "Disable All" → 上述一切立即停止
- [ ] 重启游戏 → 之前在面板里设置的值(GoldValue 等)全部恢复
- [ ] `BepInEx/config/com.kk.lav-cheats.cfg` 内容与 GUI 上次状态一致

## 存档不损坏

- [ ] 开 mod 玩 5 分钟 → 保存
- [ ] 关游戏
- [ ] 把 `<GAME>/BepInEx/plugins/LordsAndVilleinsCheats/` 整个删除(模拟卸载)
- [ ] 启动游戏读同一存档 → 能正常加载,游戏行为无残留作弊

## 失败回退

如果某模块在游戏内完全跑不通:
1. 在 GUI 顶部点 "Disable All",停止所有锁定行为
2. 退出游戏
3. 把 `<GAME>/BepInEx/plugins/LordsAndVilleinsCheats/LordsAndVilleinsCheats.dll` 移走
4. 启动游戏验证恢复原版
5. 把存档从 `_modbackup/<最新时间戳>/` 复制回 SaveData 覆盖

如果游戏完全启不来:
1. 删掉 `<GAME>/winhttp.dll`(完全卸载 BepInEx 注入)
2. 游戏立刻恢复原版
