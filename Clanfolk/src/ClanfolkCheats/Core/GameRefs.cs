using System;
using System.Linq;
using System.Reflection;
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
                var gmType = ResolveType("GameManager");
                if (gmType == null) return null;
                _cachedGM = GetStaticSingleton(gmType, "instance")
                    ?? GetStaticSingleton(gmType, "Instance")
                    ?? GetStaticSingleton(gmType, "singleton")
                    ?? GetStaticSingleton(gmType, "Singleton");
                if (_cachedGM != null)
                {
                    _cached = true;
                    MelonLogger.Msg($"[GameRefs] GameManager resolved via {gmType.FullName}");
                }
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
                    var ecType = ResolveType("EntityClass");
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

        public static Type? ResolveType(string name)
        {
            var fullName = name.StartsWith("Il2Cpp.", StringComparison.Ordinal) ? name : $"Il2Cpp.{name}";
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = null;
                try
                {
                    type = assembly.GetType(fullName, false)
                        ?? assembly.GetType(name, false);
                }
                catch { }
                if (type != null) return type;
            }

            return Type.GetType(fullName, false) ?? Type.GetType(name, false);
        }

        private static object? GetStaticSingleton(Type type, string memberName)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            var prop = type.GetProperty(memberName, flags);
            if (prop != null)
            {
                try { return prop.GetValue(null, null); }
                catch (Exception ex) { MelonLogger.Warning($"[GameRefs] {type.FullName}.{memberName} property failed: {ex.Message}"); }
            }

            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                try { return field.GetValue(null); }
                catch (Exception ex) { MelonLogger.Warning($"[GameRefs] {type.FullName}.{memberName} field failed: {ex.Message}"); }
            }

            return null;
        }
    }
}
