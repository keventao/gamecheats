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
        var go = new GameObject("ZedZoneHpLock");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<HpLock>();
        go.AddComponent<StaminaLock>();
        go.AddComponent<Panel>();
        Log.LogInfo("ZedZoneHpLock v0.4.7 loaded: primary path adds max stack");
    }
}
