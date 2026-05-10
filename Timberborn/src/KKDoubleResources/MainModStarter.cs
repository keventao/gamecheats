using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace KKDoubleResources;

public sealed class MainModStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        Debug.Log("[KKDoubleResources] StartMod");
        new Harmony("public.timberborn.double-resources").PatchAll(typeof(MainModStarter).Assembly);
        Debug.Log("[KKDoubleResources] Harmony patches applied");
    }
}
