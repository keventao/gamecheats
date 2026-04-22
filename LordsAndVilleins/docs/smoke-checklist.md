# 冒烟清单 — Lords & Villeins Cheats

每次 release 候选前过一遍。建议在干净存档(或备份过的存档)上跑。

## 准备

- [ ] `dotnet test` 全绿(10 passed, 1 skipped 是当前正常状态)
- [ ] `dotnet build -c Release` 无错
- [ ] `powershell tools/install.ps1` 完成
- [ ] `powershell tools/run-and-check.ps1` 输出 `RESULT: PASS`(自动校验 mod 加载与 patch summary)

## Loader

- [ ] 干净存档加载,无 mod 行为干扰(全部 Lock 默认 OFF)
- [ ] BepInEx LogOutput.log 显示 `Patch summary: N/N ok, 0 broken`
- [ ] F1 弹出灰色面板,标题 "Lords & Villeins Cheats"
- [ ] 顶栏红色 "Disable All" 按钮可见
- [ ] 状态指示在主菜单显示 "○ menu",在游戏内显示 "● in-game"

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
