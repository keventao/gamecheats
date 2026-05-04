namespace HumanicaCheats.Modules
{
    internal static class WarehouseCapacityBaselinePolicy
    {
        public static int InferBaseline(int savedBaseline, int currentPacks, int previousMultiplier)
        {
            if (currentPacks <= 0)
            {
                return currentPacks;
            }

            if (previousMultiplier > 1 && savedBaseline > 0 && currentPacks == savedBaseline * previousMultiplier)
            {
                return savedBaseline;
            }

            if (previousMultiplier > 1 && savedBaseline <= 0 && currentPacks % previousMultiplier == 0)
            {
                int inferred = currentPacks / previousMultiplier;
                if (inferred > 0)
                {
                    return inferred;
                }
            }

            return currentPacks;
        }
    }
}
