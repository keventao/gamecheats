using System;
using HarmonyLib;
using UnityEngine;

namespace LunHuiCheats.Core
{
    /// <summary>
    /// Tracks whether the game world is loaded and exposes cached, name-based
    /// lookups of the player's data objects. All game types are resolved by name
    /// through AccessTools to avoid hard IL2CPP dependencies.
    /// </summary>
    public static class GameRefs
    {
        public static bool IsReady { get; private set; }

        private static UnityEngine.Object? _characterData;
        private static int _lastResolveFrame = -1000;

        public static void SetReady(bool ready)
        {
            IsReady = ready;
            if (!ready) _characterData = null;
            Plugin.LogSrc?.LogInfo($"[GameRefs] IsReady = {ready}");
            if (ready) Plugin.Registry?.NotifyGameReady();
        }

        /// <summary>The player's CharacterData component (cached; throttled re-resolve).</summary>
        public static object? CharacterData
        {
            get
            {
                if (_characterData != null) return _characterData;
                if (Time.frameCount - _lastResolveFrame < 30) return null; // throttle FindObjectOfType
                _lastResolveFrame = Time.frameCount;
                _characterData = FindByTypeObj("CharacterData");
                return _characterData;
            }
        }

        /// <summary>CharacterData.unitData (DataLib.UnitData) — battle/base stats.</summary>
        public static object? UnitData
        {
            get
            {
                var c = CharacterData;
                return c != null && ReflectAccessor.TryGet(c, "unitData", out var u) ? u : null;
            }
        }

        /// <summary>FakeInventoryData instance, if it is a UnityEngine.Object in the scene.</summary>
        public static object? Inventory => FindByTypeObj("FakeInventoryData");

        public static T? FindByType<T>(string typeName) where T : UnityEngine.Object
            => FindByTypeObj(typeName) as T;

        public static UnityEngine.Object? FindByTypeObj(string typeName)
        {
            try
            {
                var t = AccessTools.TypeByName(typeName);
                if (t == null) return null;
                return UnityEngine.Object.FindObjectOfType(t);
            }
            catch (Exception ex)
            {
                Plugin.LogSrc?.LogWarning($"[GameRefs] FindObjectOfType<{typeName}> failed: {ex.Message}");
                return null;
            }
        }
    }
}
