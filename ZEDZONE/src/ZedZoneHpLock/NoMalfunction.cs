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
