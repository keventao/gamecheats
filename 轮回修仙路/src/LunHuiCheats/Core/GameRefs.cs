using System;
using HarmonyLib;
using UnityEngine;

namespace LunHuiCheats.Core
{
    /// <summary>
    /// Tracks whether the game world is loaded and exposes common game-object lookups.
    /// All game-specific types are accessed by name through AccessTools to avoid hard IL2CPP dependencies.
    /// </summary>
    public static class GameRefs
    {
        public static bool IsReady { get; private set; }

        public static void SetReady(bool ready)
        {
            IsReady = ready;
            Plugin.LogSrc?.LogInfo($"[GameRefs] IsReady = {ready}");
            if (ready) Plugin.Registry?.NotifyGameReady();
        }

        public static T? FindByType<T>(string typeName) where T : UnityEngine.Object
        {
            try
            {
                var t = AccessTools.TypeByName(typeName);
                if (t == null) return null;
                return UnityEngine.Object.FindObjectOfType(t) as T;
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] FindObjectOfType<{typeName}> failed: {ex.Message}");
                return null;
            }
        }
    }
}
