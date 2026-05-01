# Humanica Cheats — ROADMAP

## v0.1.0 (当前)

**状态:** code complete,等待游戏内冒烟验证。

**已实现:**
- 时间控制: Time.timeScale ×1/×2/×5/×10 + 重置,面板实时显示目标 vs 实际值
- 资源作弊: 仓库 +100/+1000 木材/石材/食物/金币 + 锁定 ≥500(每帧补差值)
- 村庄作弊: 添加村民(SpawnRandomVillager) + 建造速度 ×10 + 生产速度 ×10
- 解锁作弊: 解锁全部科技(InstantResearchAll)

**已知限制 / 待游戏内确认:**

1. **ResourceIndex 4 个值为占位推断** — `LOG/STONE_BRICKS/BREAD/TECHNOLOGY_KNOWLEDGE`
   分别映射到"木材/石材/食物/金币"仅为推断。`TECHNOLOGY_KNOWLEDGE` 作为金币尤为可疑，
   游戏内可能对应科技点数而非金币。需按 `docs/smoke-checklist.md` 逐个 +100 对照仓库变化确认。

2. **建造速度 patch 方向未确认** — `CalculateProgressPerTimeStep` postfix `×10` 假设该方法
   返回"每步进度速率"(加快 → 工期缩短)。若游戏内建筑反而变慢，说明返回的是工期，
   需改为 `__result /= 10f`。

3. **patch 目标依赖 IL2CPP 命名空间路径** — `ConstructionProduction` 和 `BuffController`
   通过 `AccessTools.TypeByName` 运行时绑定。游戏 IL2CPP 重编译或更新版本可能导致
   类名/命名空间变化，届时需重新 dump 并更新路径。

4. **`AddResourceIntoFreeWarehouse` 第三参数语义未确认** — 当前传 `false`（不新建仓库槽），
   若仓库已满资源无法添加，需改为 `true`。

5. **未测试: 多存档并发、从主菜单切换存档后单例重置**。

**MelonLoader 版本:** [填入安装时的版本号]  
**游戏版本:** [填入]  
**最后验证日期:** [填入]

---

## v0.2.0 (待规划)

- 修正 ResourceIndex 实际映射(基于 v0.1.0 游戏内验证结果)
- 若时间 timeScale 被游戏覆写: OnUpdate 强写或 patch 覆写源
- 若建造速度方向错误: 修正 postfix 为 `÷10`
- 属性/技能作弊(若游戏有村民属性系统)
- 天气/季节控制
- 存档备份

---

## 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| v0.1.0 | 2026-05-01 | 初版合并到 main。4 模块实现(时间/资源/村庄/解锁)。等待游戏内冒烟。 |
