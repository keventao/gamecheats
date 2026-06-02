using System;
using HarmonyLib;
using MelonLoader;

namespace ClanfolkCheats.Core
{
    public static class GameRefs
    {
        private static object? _cachedGM;
        private static bool _cached;

        public static object? GetGameManager()
        {
            if (_cached) return _cachedGM;
            try
            {
                var gmType = AccessTools.TypeByName("Il2Cpp.GameManager");
                if (gmType == null) return null;
                var instProp = AccessTools.Property(gmType, "instance");
                _cachedGM = instProp?.GetValue(null, null);
                if (_cachedGM != null)
                    _cached = true;
                return _cachedGM;
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[GameRefs] GetGameManager failed: {ex.Message}");
                return null;
            }
        }

        public static void InvalidateCache()
        {
            _cached = false;
            _cachedGM = null;
        }

        public static bool IsReady
        {
            get
            {
                try
                {
                    var gm = GetGameManager();
                    if (gm == null) return false;

                    // Check if EntityManager is populated
                    var ecType = AccessTools.TypeByName("Il2Cpp.EntityClass");
                    if (ecType == null) return false;

                    var itemEC = Enum.Parse(ecType, "Item");
                    var getEM = AccessTools.Method(gm.GetType(), "GetEntityManager", new Type[] { ecType });
                    if (getEM == null) return false;

                    var em = getEM.Invoke(gm, new object[] { itemEC });
                    if (em == null) return false;

                    var getCount = AccessTools.Method(em.GetType(), "GetEntityCount");
                    if (getCount == null) return false;

                    var count = (int)(getCount.Invoke(em, null) ?? 0);
                    return count > 0;
                }
                catch { return false; }
            }
        }

        public static object? GetManager(string methodName)
        {
            var gm = GetGameManager();
            if (gm == null) return null;
            try
            {
                var method = AccessTools.Method(gm.GetType(), methodName);
                return method?.Invoke(gm, null);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[GameRefs] GetManager({methodName}) failed: {ex.Message}");
                return null;
            }
        }
    }
}
