using System;

namespace LordsAndVilleinsCheats.Util
{
    /// <summary>
    /// Wraps Harmony patch registration / patch body invocations so any failure
    /// is logged and absorbed instead of breaking the plugin or the game.
    /// </summary>
    public static class HarmonyHelpers
    {
        public static Action<string>? OnFailure { get; set; }

        public static bool SafeRun(string opName, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                OnFailure?.Invoke($"[SafeRun] {opName} failed: {ex}");
                return false;
            }
        }
    }
}
