using MelonLoader;

namespace FarthestFrontier.FastVillagers
{
    internal static class FastVillagersConfig
    {
        private static MelonPreferences_Category _category;
        private static MelonPreferences_Entry<bool> _enableVillagerSpeed;
        private static MelonPreferences_Entry<float> _villagerMoveSpeedMultiplier;
        private static MelonPreferences_Entry<float> _villagerShoeBonusBase;
        private static MelonPreferences_Entry<float> _villagerTurningSpeedMultiplier;
        private static MelonPreferences_Entry<bool> _enableWagonSpeed;
        private static MelonPreferences_Entry<float> _wagonMoveSpeed;
        private static MelonPreferences_Entry<float> _wagonTurningSpeedMultiplier;
        private static MelonPreferences_Entry<float> _wagonCarryCapacity;

        public static bool EnableVillagerSpeed
        {
            get { return _enableVillagerSpeed == null || _enableVillagerSpeed.Value; }
        }

        public static float VillagerShoeBonusBase
        {
            get { return GetPositive(_villagerShoeBonusBase, 1.0f); }
        }

        public static float VillagerMoveSpeedMultiplier
        {
            get { return GetPositive(_villagerMoveSpeedMultiplier, 3.0f); }
        }

        public static float VillagerTurningSpeed
        {
            get { return GetPositive(_villagerTurningSpeedMultiplier, 3.0f); }
        }

        public static bool EnableWagonSpeed
        {
            get { return _enableWagonSpeed == null || _enableWagonSpeed.Value; }
        }

        public static float WagonMoveSpeed
        {
            get { return GetPositive(_wagonMoveSpeed, 8.0f); }
        }

        public static float WagonTurningSpeed
        {
            get { return WagonMoveSpeed * GetPositive(_wagonTurningSpeedMultiplier, 50.0f); }
        }

        public static float WagonCarryCapacity
        {
            get { return GetPositive(_wagonCarryCapacity, 400.0f); }
        }

        public static void Load()
        {
            _category = MelonPreferences.CreateCategory("KKFastVillagers", "KK Fast Villagers");
            _enableVillagerSpeed = _category.CreateEntry("EnableVillagerSpeed", true, "Enable villager speed patch");
            _villagerMoveSpeedMultiplier = _category.CreateEntry("VillagerMoveSpeedMultiplier", 3.0f, "Multiplier applied to villager walk and run base speed");
            _villagerShoeBonusBase = _category.CreateEntry("VillagerShoeBonusBase", 1.0f, "Value written to Character._shoeBonusBase for villagers");
            _villagerTurningSpeedMultiplier = _category.CreateEntry("VillagerTurningSpeedMultiplier", 3.0f, "Multiplier applied to villager turning speed");
            _enableWagonSpeed = _category.CreateEntry("EnableWagonSpeed", true, "Enable transport wagon speed patch");
            _wagonMoveSpeed = _category.CreateEntry("WagonMoveSpeed", 8.0f, "Value written to TransportWagon._movementSpeed");
            _wagonTurningSpeedMultiplier = _category.CreateEntry("WagonTurningSpeedMultiplier", 50.0f, "Wagon turning speed = move speed * this value");
            _wagonCarryCapacity = _category.CreateEntry("WagonCarryCapacity", 400.0f, "Value written to TransportWagon.carryCapacity");
            MelonPreferences.Save();

            MelonLogger.Msg("[KK Fast Villagers] VillagerSpeed enabled=" + EnableVillagerSpeed
                + " moveSpeedMultiplier=x" + VillagerMoveSpeedMultiplier
                + " shoeBonusBase=" + VillagerShoeBonusBase
                + " turningMultiplier=x" + VillagerTurningSpeed);
            MelonLogger.Msg("[KK Fast Villagers] WagonSpeed enabled=" + EnableWagonSpeed
                + " moveSpeed=" + WagonMoveSpeed
                + " turningSpeed=" + WagonTurningSpeed
                + " carryCapacity=" + WagonCarryCapacity);
        }

        private static float GetPositive(MelonPreferences_Entry<float> entry, float fallback)
        {
            if (entry == null || entry.Value <= 0.0f)
            {
                return fallback;
            }

            return entry.Value;
        }
    }
}
