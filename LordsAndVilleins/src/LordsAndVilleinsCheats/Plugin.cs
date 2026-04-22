using BepInEx;
using BepInEx.Logging;

namespace LordsAndVilleinsCheats
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginId      = "com.kk.lav-cheats";
        public const string PluginName    = "Lords & Villeins Cheats";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log = null!;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }
    }
}
