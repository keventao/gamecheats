namespace HumanicaCheats.Modules
{
    internal static class CropGrowthPolicy
    {
        private const float SelfPlantedCropMultiplier = 10f;

        public static float ApplyMultiplier(
            float originalSpeed,
            bool enabled,
            bool hasPlantGrowingTrigger,
            bool isPlayerPlanted)
        {
            if (!enabled || !hasPlantGrowingTrigger || !isPlayerPlanted)
            {
                return originalSpeed;
            }

            return originalSpeed * SelfPlantedCropMultiplier;
        }
    }
}
