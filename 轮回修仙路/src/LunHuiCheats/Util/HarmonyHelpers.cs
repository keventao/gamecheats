using System;
using HarmonyLib;

namespace LunHuiCheats.Util
{
    /// <summary>
    /// Common Harmony helpers used across modules.
    /// </summary>
    public static class HarmonyHelpers
    {
        public static Action<string>? OnFailure { get; set; }

        public static void SafeRun(string label, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var msg = $"[HarmonyHelpers] {label} failed: {ex}";
                OnFailure?.Invoke(msg);
            }
        }

        public static bool TryGetMethod(string typeName, string methodName, out System.Reflection.MethodInfo? method)
        {
            method = null;
            try
            {
                var t = AccessTools.TypeByName(typeName);
                if (t == null) return false;
                method = AccessTools.Method(t, methodName);
                return method != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
