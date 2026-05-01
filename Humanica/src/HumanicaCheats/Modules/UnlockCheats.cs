using System;
using MelonLoader;
using UnityEngine;
using HumanicaCheats.Core;

namespace HumanicaCheats.Modules
{
    public class UnlockCheats : ICheatModule
    {
        public string       Name   => "解锁";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Ok;

        // 无 Harmony patch 需要注册
        public void Register(HarmonyLib.Harmony harmony) { }

        public void DrawGui(Layout l)
        {
            if (!GameRefs.IsReady) { l.Label("等待游戏加载…"); return; }

            // 建筑解锁由科技驱动，无独立建筑解锁系统（refs/01-resource-research.md 确认）
            l.Label("[!] 建筑解锁由科技驱动，解锁全部科技后建筑自动可用");
            l.Space(4);

            if (l.Button("解锁全部科技 (InstantResearchAll)", 28f))
                TryUnlockAllTech();
        }

        // ── 解锁全部科技 ──────────────────────────────────────────────
        // 服务定位器 Il2Cpp.S.TechManager 取单例，调用无参 InstantResearchAll()。
        // TechManager_Public_Static_get_TechManager_0 单例已通过二进制确认 (refs/00-research-checklist.md)。
        // InstantResearchAll_Public_Void_0 方法已通过二进制确认。
        // 调用路径：Il2Cpp.S.TechManager（与 Il2Cpp.S.VillageData / Il2Cpp.S.CreatureManager 同模式）。
        // TODO: 游戏内需确认 — F1 → "解锁" Tab → 点击按钮 → 科技树全亮。
        private static void TryUnlockAllTech()
        {
            try
            {
                var tm = Il2Cpp.S.TechManager;
                if (tm == null)
                {
                    MelonLogger.Warning("[UnlockCheats] Il2Cpp.S.TechManager 为 null — 游戏世界未加载?");
                    return;
                }
                tm.InstantResearchAll();
                MelonLogger.Msg("[UnlockCheats] InstantResearchAll 已调用");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[UnlockCheats] TryUnlockAllTech 失败: {ex.Message}");
            }
        }
    }
}
