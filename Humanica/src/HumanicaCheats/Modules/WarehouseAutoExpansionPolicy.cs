namespace HumanicaCheats.Modules
{
    internal static class WarehouseAutoExpansionPolicy
    {
        public static bool ShouldApply(int selectedMultiplier, int loadedWorldMultiplier, bool alreadyAppliedForLoadedWorld)
        {
            return loadedWorldMultiplier > 1
                && selectedMultiplier == loadedWorldMultiplier
                && !alreadyAppliedForLoadedWorld;
        }
    }
}
