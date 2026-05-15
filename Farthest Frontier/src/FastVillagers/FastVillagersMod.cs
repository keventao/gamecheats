using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(FarthestFrontier.FastVillagers.FastVillagersMod), "KK Fast Villagers", "0.1.0", "KK")]
[assembly: MelonGame("Crate Entertainment", "Farthest Frontier")]

namespace FarthestFrontier.FastVillagers
{
    public sealed class FastVillagersMod : MelonPlugin
    {
        private const string HarmonyId = "kk.farthestfrontier.fastvillagers";

        public override void OnPreModsLoaded()
        {
            FastVillagersConfig.Load();
            SpeedPatchInstaller.Install(new HarmonyLib.Harmony(HarmonyId));

            LoggerInstance.Msg("plugin loaded for Farthest Frontier Mono. Config: UserData/MelonPreferences.cfg [KKFastVillagers]");
        }
    }
}
