using HarmonyLib;
using LunHuiCheats.Core;
using LunHuiCheats.Core.Gui;
using UnityEngine;

namespace LunHuiCheats.Modules
{
    /// <summary>
    /// Cultivation editor (increment model): shows current 等级/道心, each with a delta box +
    /// "+N" button. 等级 → UpdateLevel(current+delta, save); 道心 → AddDaoxin(delta). 经验
    /// (currentExp) is read-only (no setter; derived — set level instead). Spirit-root read-only.
    /// </summary>
    public sealed class Cultivation : ICheatModule
    {
        public string Id => "cultivation";
        public string Name => "修为 Cultivation";
        public string Category => "修为";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private int _levelDelta = 1;
        private int _daoxinDelta = 100;
        private string _status = "";

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("CharacterData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { _status = ""; }
        public void OnUpdate() { }   // increment-on-button, nothing locked per frame

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            var cd = GameRefs.CharacterData;
            if (cd == null) { GuiWidgets.Label(c.Line(), "未找到 CharacterData（进入游戏世界后生效）"); return; }

            // Read live each frame (no snapshot) so "当前值" always reflects the game.
            int curLevel  = ReflectAccessor.GetInt32(cd, "currentLevel");
            int curDaoxin = ReflectAccessor.GetInt32(cd, "curDaoxin");
            long curExp   = ReflectAccessor.GetInt64(cd, "currentExp");

            // 等级：显示当前 + 增量框 + 加按钮。currentLevel 只读，用 UpdateLevel(目标绝对值, 存档) 设到 当前+增量。
            var lr = c.Line();
            GuiWidgets.Label(c.Slice(ref lr, 150), $"当前等级 {curLevel}");
            _levelDelta = (int)GuiWidgets.Int64Field(c.Slice(ref lr, 60), "cult.levelDelta", _levelDelta);
            if (GuiWidgets.Button(c.Slice(ref lr, 110), $"等级 +{_levelDelta}"))
            {
                // UpdateLevel(n) ADDS n levels (observed: 13 + UpdateLevel(14) → 27), not set-to-n.
                // So pass the delta directly to add exactly _levelDelta.
                bool ok = ReflectAccessor.TryInvoke(cd, "UpdateLevel", out _, _levelDelta, true);
                _status = $"等级 +{_levelDelta}（原 {curLevel}）{(ok ? "✓" : "✗")}";
                Plugin.LogSrc?.LogInfo($"[Cultivation] level +{_levelDelta} via UpdateLevel(delta) ok={ok}");
            }

            // 道心：显示当前 + 增量框 + 加按钮。用游戏自带增量方法 AddDaoxin(Int32)。
            var dr = c.Line();
            GuiWidgets.Label(c.Slice(ref dr, 150), $"当前道心 {curDaoxin}");
            _daoxinDelta = (int)GuiWidgets.Int64Field(c.Slice(ref dr, 60), "cult.daoxinDelta", _daoxinDelta);
            if (GuiWidgets.Button(c.Slice(ref dr, 110), $"道心 +{_daoxinDelta}"))
            {
                bool ok = ReflectAccessor.TryInvoke(cd, "AddDaoxin", out _, _daoxinDelta);
                _status = $"道心 +{_daoxinDelta} {(ok ? "✓" : "✗")}";
                Plugin.LogSrc?.LogInfo($"[Cultivation] daoxin +{_daoxinDelta} via AddDaoxin ok={ok}");
            }

            // 经验：只读。currentExp 无 setter、是派生值；直接设等级即可，不提供写入。
            GuiWidgets.Label(c.Line(), $"当前经验 {curExp}（只读 · 设等级即可）");

            if (_status.Length > 0) GuiWidgets.Label(c.Line(), _status);

            // 灵根只读显示（编辑后续迭代）。
            var unit = GameRefs.UnitData;
            if (unit != null && ReflectAccessor.TryGet(unit, "discipleSpiritData", out var dsd) && dsd != null)
                GuiWidgets.Label(c.Line(), "灵根数据已找到（编辑功能后续迭代）");
        }

        public void DisableAll() { }
    }
}
