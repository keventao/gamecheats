using MelonLoader;

namespace HumanicaCheats.Modules
{
    internal static class WarehouseCapacityPatch
    {
        public static bool Register(HarmonyLib.Harmony harmony)
        {
            MelonLogger.Warning("[WarehouseCapacityPatch] Disabled: runtime warehouse slot resizing caused repeatable combat crashes.");
            return false;
        }
    }
}
