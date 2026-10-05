using HarmonyLib;

namespace ZedZoneHpLock;

// v0.4.17: suppress gun malfunctions entirely (same as Jim97's
// BasicRangedWeaponOverridePatch_WeaponMalfunction). Single overload +
// explicit name = no ambiguity risk (the RefreshTotalItemWeight lesson).
// Freshly spawned guns arrive in a half-initialized state that the game
// flags as malfunction; blocking the flag keeps them firing.
[HarmonyPatch(typeof(BasicRangedWeapon), "WeaponMalfunction")]
public static class NoMalfunctionPatch
{
    static bool Prefix() { return false; }
}

// v0.5.0: CanFire forced true while InfiniteAmmo is on (single overload,
// explicit name — no ambiguity risk). Reload then always succeeds because
// AmmoLock keeps reserves topped up.
[HarmonyPatch(typeof(BasicRangedWeapon), "CanFire")]
public static class CanFirePatch
{
    static void Postfix(BasicRangedWeapon __instance, ref bool __result)
    {
        if (Toggles.InfiniteAmmo) __result = true;
    }
}
