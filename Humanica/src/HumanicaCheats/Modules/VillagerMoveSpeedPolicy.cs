namespace HumanicaCheats.Modules
{
    internal static class VillagerMoveSpeedPolicy
    {
        public static float ApplyMultiplier(float originalSpeed, int multiplier, bool isPlayerVillager)
        {
            if (!isPlayerVillager || (multiplier != 2 && multiplier != 5))
            {
                return originalSpeed;
            }

            return originalSpeed * multiplier;
        }
    }
}
