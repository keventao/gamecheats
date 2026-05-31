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
        private bool _synced;
        private string _writeStatus = "";

        public void Register(ModConfig cfg, Harmony harmony)
        {
            Status = AccessTools.TypeByName("CharacterData") != null
                ? ModuleStatus.Ok : ModuleStatus.Broken;
        }

        public void OnGameReady() { _synced = false; }
        public void OnUpdate() { }   // exp/level are write-on-demand, not locked

        public void DrawGui()
        {
            var c = new LayoutCursor(0, 0, 360);
            var cd = GameRefs.CharacterData;
            if (cd == null) { GuiWidgets.Label(c.Line(), "未找到 CharacterData（进入游戏世界后生效）"); return; }

            if (!_synced)
            {
                _exp    = ReflectAccessor.GetInt64(cd, "currentExp");
                _level  = ReflectAccessor.GetInt32(cd, "currentLevel");
                _daoxin = ReflectAccessor.GetInt32(cd, "curDaoxin");
                _synced = true;
            }

            var l1 = c.Line();
            GuiWidgets.Label(new Rect(l1.x, l1.y, 70, l1.height), "经验");
            _exp = GuiWidgets.Int64Field(new Rect(l1.x + 72, l1.y, 140, l1.height), "cult.exp", _exp);

            var l2 = c.Line();
            GuiWidgets.Label(new Rect(l2.x, l2.y, 70, l2.height), "等级");
            _level = (int)GuiWidgets.Int64Field(new Rect(l2.x + 72, l2.y, 140, l2.height), "cult.level", _level);

            var l3 = c.Line();
            GuiWidgets.Label(new Rect(l3.x, l3.y, 70, l3.height), "道心");
            _daoxin = (int)GuiWidgets.Int64Field(new Rect(l3.x + 72, l3.y, 140, l3.height), "cult.daoxin", _daoxin);

            if (GuiWidgets.Button(c.Line(), "写入 道心(经验/等级只读,尝试写)"))
            {
                bool dao = ReflectAccessor.TrySet(cd, "curDaoxin", _daoxin);
                bool exp = ReflectAccessor.TrySet(cd, "currentExp", _exp);
                bool lvl = ReflectAccessor.TrySet(cd, "currentLevel", _level);
                _writeStatus = $"道心{(dao ? "✓" : "✗")} 经验{(exp ? "✓" : "✗")} 等级{(lvl ? "✓" : "✗")}";
                Plugin.LogSrc?.LogInfo($"[Cultivation] write daoxin={dao} exp={exp} level={lvl}");
            }
            if (_writeStatus != "") GuiWidgets.Label(c.Line(), _writeStatus);

            // spirit-root read-only display, if reachable
            var unit = GameRefs.UnitData;
            if (unit != null && ReflectAccessor.TryGet(unit, "discipleSpiritData", out var dsd) && dsd != null)
                GuiWidgets.Label(c.Line(), "灵根数据已找到（编辑功能后续迭代）");
        }

        public void DisableAll() { }
    }
}
