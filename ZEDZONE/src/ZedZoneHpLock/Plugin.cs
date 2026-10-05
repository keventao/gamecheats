using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using System;
using UnityEngine;

namespace ZedZoneHpLock;

[BepInPlugin("com.keventao.zedzone.hplock", "ZedZone HP Lock", "0.1.0")]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        ClassInjector.RegisterTypeInIl2Cpp<HpLock>();
        ClassInjector.RegisterTypeInIl2Cpp<StaminaLock>();
        ClassInjector.RegisterTypeInIl2Cpp<Panel>();
        ClassInjector.RegisterTypeInIl2Cpp<ItemPanel>();
        ClassInjector.RegisterTypeInIl2Cpp<AmmoLock>();
        var go = new GameObject("ZedZoneHpLock");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<HpLock>();
        go.AddComponent<StaminaLock>();
        go.AddComponent<AmmoLock>();
        go.AddComponent<Panel>();
        try {
            new HarmonyLib.Harmony("com.keventao.zedzone.hplock").PatchAll();
            Log.LogInfo("Harmony patches applied (NoMalfunction)");
        } catch (Exception e) { Log.LogError("Harmony PatchAll failed: " + e.GetType().Name + " " + e.Message); }
        Log.LogInfo("ZedZoneHpLock v0.5.0 loaded: infinite reserve ammo + CanFire + uGUI ammo toggle");
    }
}
