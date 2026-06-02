using System;
using System.Reflection;
using UnityEngine;
using ClanfolkCheats.Core;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Modules
{
    // Money / 金钱 — wraps the game's own MoneyManager (verified, refs/04):
    //   GameManager.GetMoneyManager()  -> MoneyManager (static)
    //   MoneyManager.GetMoney()        -> int   (current settlement money)
    //   MoneyManager.ChangeMoney(int)  -> void  (add/subtract delta)
    //   MoneyManager.SetMoney(int, bool reset) -> void (set absolute)
    public class MoneyCheats : ICheatModule
    {
        public string Name => "金钱";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        private object? _moneyManager;
        private MethodInfo? _getMoney;
        private MethodInfo? _changeMoney;
        private MethodInfo? _setMoney;

        private int _customAmount = 10000;
        private int _lastShown = -1;

        public void Register(HarmonyLib.Harmony harmony)
        {
            MelonLogger.Msg("[Money] Registered — will bind MoneyManager when game world loads.");
        }

        public void DrawGui(Layout l)
        {
            l.Label("金钱修改", 22f);

            if (!EnsureManager())
            {
                l.Label("等待游戏世界加载…");
                return;
            }

            l.Space(4);
            l.Label($"当前金钱: {_lastShown}", 22f);
            l.Space(2);

            float bx = l.X;
            float by = l.Y;
            if (ImguiUtil.Button(new Rect(bx, by, 60f, 24f), "+100")) Change(100);
            if (ImguiUtil.Button(new Rect(bx + 64f, by, 70f, 24f), "+1000")) Change(1000);
            if (ImguiUtil.Button(new Rect(bx + 138f, by, 80f, 24f), "+10000")) Change(10000);
            if (ImguiUtil.Button(new Rect(bx + 222f, by, 60f, 24f), "清零")) SetAbsolute(0);
            l.Y += 28f;

            l.Space(4);
            l.Label($"自定义金额: {_customAmount}", 20f);
            _customAmount = ClampIntInput(l.X + 130f, l.Y - 20f, _customAmount, 0, 9_999_999, 1000);
            l.Y += 4f;

            if (ImguiUtil.Button(new Rect(l.X, l.Y, 90f, 24f), "增加")) Change(_customAmount);
            if (ImguiUtil.Button(new Rect(l.X + 94f, l.Y, 90f, 24f), "设为")) SetAbsolute(_customAmount);
            l.Y += 28f;
        }

        public void OnUpdate()
        {
            if (_moneyManager == null) return;
            // keep the displayed value fresh without spamming reflection from OnGUI
            try { if (_getMoney?.Invoke(_moneyManager, null) is int m) _lastShown = m; }
            catch { }
        }

        private bool EnsureManager()
        {
            if (_moneyManager != null) return true;

            _moneyManager = GameRefs.GetManager("GetMoneyManager");
            if (_moneyManager == null) return false;

            var t = _moneyManager.GetType();
            _getMoney = AccessTools.Method(t, "GetMoney");
            _changeMoney = AccessTools.Method(t, "ChangeMoney", new[] { typeof(int) });
            _setMoney = AccessTools.Method(t, "SetMoney", new[] { typeof(int), typeof(bool) });

            Status = ModuleStatus.Ok;
            MelonLogger.Msg("[Money] OK — MoneyManager bound");
            return true;
        }

        private void Change(int delta)
        {
            if (_moneyManager == null) return;
            try { _changeMoney?.Invoke(_moneyManager, new object[] { delta }); }
            catch (Exception ex) { MelonLogger.Warning($"[Money] ChangeMoney({delta}): {ex.Message}"); }
        }

        private void SetAbsolute(int value)
        {
            if (_moneyManager == null) return;
            try { _setMoney?.Invoke(_moneyManager, new object[] { value, false }); }
            catch (Exception ex) { MelonLogger.Warning($"[Money] SetMoney({value}): {ex.Message}"); }
        }

        // Scroll-wheel int input; step is large for money. Mirrors ResourceCheats' helper.
        private static int ClampIntInput(float x, float y, int current, int min, int max, int step)
        {
            var r = new Rect(x, y, 90f, 20f);
            GUI.Box(r, current.ToString());
            var ev = Event.current;
            if (ev == null || !r.Contains(ev.mousePosition)) return current;
            if (ev.type == EventType.ScrollWheel)
            {
                ev.Use();
                return Math.Clamp(current + (ev.delta.y > 0 ? -step : step), min, max);
            }
            return current;
        }
    }
}
