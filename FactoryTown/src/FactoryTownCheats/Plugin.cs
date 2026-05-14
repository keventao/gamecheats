using System;
using BepInEx;
using BepInEx.Logging;
using FactoryTownCheats.Core;
using FactoryTownCheats.Modules;
using HarmonyLib;
using UnityEngine;

namespace FactoryTownCheats
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginId = "com.kk.factorytown-cheats";
        public const string PluginName = "Factory Town Cheats";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log = null!;
        internal static Harmony HarmonyInstance = null!;

        private void Awake()
        {
            Log = Logger;

            try
            {
                Log.LogInfo($"Loading {PluginName} {PluginVersion} for game version {Application.version}.");

                HarmonyInstance = new Harmony(PluginId);
                OmniFactoryRecipes.Register(HarmonyInstance);

                var runnerObject = new GameObject("FactoryTownCheatsRunner");
                DontDestroyOnLoad(runnerObject);
                var runner = runnerObject.AddComponent<CheatsRunner>();
                runner.Bind(Log);

                Log.LogInfo($"{PluginName} ready. Waiting for Crafting.Init to inject Workshop recipes.");
            }
            catch (Exception ex)
            {
                Log.LogFatal($"Plugin failed to initialize: {ex}");
            }
        }
    }
}
