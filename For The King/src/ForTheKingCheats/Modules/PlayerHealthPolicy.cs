namespace ForTheKingCheats.Modules
{
    public static class PlayerHealthPolicy
    {
        public static int GetProtectedHealth(bool lockEnabled, int currentHealth, int requestedHealth, int maxHealth)
        {
            if (!lockEnabled)
            {
                return requestedHealth;
            }

            if (requestedHealth >= currentHealth)
            {
                return requestedHealth;
            }

            return currentHealth > maxHealth ? maxHealth : currentHealth;
        }
    }
}
