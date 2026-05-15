using System;
using System.Reflection;

namespace FarthestFrontier.FastVillagers
{
    internal static class SpeedPatches
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static void CharacterAwakePostfix(object __instance)
        {
            if (!FastVillagersConfig.EnableVillagerSpeed || __instance == null)
            {
                return;
            }

            Type runtimeType = __instance.GetType();
            if (!IsTypeOrBaseNamed(runtimeType, "Villager"))
            {
                return;
            }

            TrySetFloat(__instance, "_shoeBonusBase", FastVillagersConfig.VillagerShoeBonusBase);
        }

        private static void CharacterMovementSpeedPostfix(object __instance, ref float __result)
        {
            if (!FastVillagersConfig.EnableVillagerSpeed || __instance == null)
            {
                return;
            }

            if (!IsTypeOrBaseNamed(__instance.GetType(), "Villager"))
            {
                return;
            }

            __result *= FastVillagersConfig.VillagerMoveSpeedMultiplier;
            Log.MsgOnce("villager-movement-getter-active", "villager movement getter active, multiplier=x" + FastVillagersConfig.VillagerMoveSpeedMultiplier);
        }

        private static void CharacterTurningSpeedPostfix(object __instance, ref float __result)
        {
            if (!FastVillagersConfig.EnableVillagerSpeed || __instance == null)
            {
                return;
            }

            if (!IsTypeOrBaseNamed(__instance.GetType(), "Villager"))
            {
                return;
            }

            __result *= FastVillagersConfig.VillagerTurningSpeed;
        }

        private static void TransportWagonAwakePostfix(object __instance)
        {
            if (!FastVillagersConfig.EnableWagonSpeed || __instance == null)
            {
                return;
            }

            TrySetFloat(__instance, "_movementSpeed", FastVillagersConfig.WagonMoveSpeed);
            TrySetFloat(__instance, "_turningSpeed", FastVillagersConfig.WagonTurningSpeed);
            TrySetFloat(__instance, "carryCapacity", FastVillagersConfig.WagonCarryCapacity);
        }

        private static bool IsTypeOrBaseNamed(Type type, string typeName)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                if (current.Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void TrySetFloat(object instance, string fieldName, float value)
        {
            FieldInfo field = FindField(instance.GetType(), fieldName);
            if (field == null)
            {
                Log.WarnOnce(instance.GetType().FullName + "." + fieldName, "missing field: " + instance.GetType().FullName + "." + fieldName);
                return;
            }

            if (field.FieldType == typeof(float))
            {
                field.SetValue(instance, value);
                return;
            }

            Log.WarnOnce(instance.GetType().FullName + "." + fieldName, "field is not float: " + instance.GetType().FullName + "." + fieldName);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(fieldName, FieldFlags);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }
    }
}
