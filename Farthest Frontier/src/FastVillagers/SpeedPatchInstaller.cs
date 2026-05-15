using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;

namespace FarthestFrontier.FastVillagers
{
    internal static class SpeedPatchInstaller
    {
        private const BindingFlags InstanceMethodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static void Install(HarmonyLib.Harmony harmony)
        {
            PatchAwake(harmony, "Character", "CharacterAwakePostfix");
            PatchMethod(harmony, "Character", "get_movementSpeed", "CharacterMovementSpeedPostfix");
            PatchMethod(harmony, "Character", "get_turningSpeed", "CharacterTurningSpeedPostfix");
            PatchAwake(harmony, "TransportWagon", "TransportWagonAwakePostfix");
        }

        private static void PatchAwake(HarmonyLib.Harmony harmony, string typeName, string postfixName)
        {
            Type targetType = AccessTools.TypeByName(typeName);
            if (targetType == null)
            {
                Log.Warn("missing game type: " + typeName);
                return;
            }

            MethodInfo awake = targetType.GetMethod("Awake", InstanceMethodFlags);
            if (awake == null)
            {
                Log.Warn("missing Awake method on " + typeName);
                return;
            }

            MethodInfo postfix = typeof(SpeedPatches).GetMethod(postfixName, BindingFlags.Static | BindingFlags.NonPublic);
            harmony.Patch(awake, null, new HarmonyMethod(postfix));
            Log.Msg("patched " + typeName + ".Awake");
        }

        private static void PatchMethod(HarmonyLib.Harmony harmony, string typeName, string methodName, string postfixName)
        {
            Type targetType = AccessTools.TypeByName(typeName);
            if (targetType == null)
            {
                Log.Warn("missing game type: " + typeName);
                return;
            }

            MethodInfo method = targetType.GetMethod(methodName, InstanceMethodFlags);
            if (method == null)
            {
                Log.Warn("missing method: " + typeName + "." + methodName);
                return;
            }

            MethodInfo postfix = typeof(SpeedPatches).GetMethod(postfixName, BindingFlags.Static | BindingFlags.NonPublic);
            harmony.Patch(method, null, new HarmonyMethod(postfix));
            Log.Msg("patched " + typeName + "." + methodName);
        }
    }
}
