using MelonLoader;
using ClanfolkCheats.Core;
using ClanfolkCheats.Modules;

[assembly: MelonInfo(typeof(ClanfolkCheats.Plugin), "ClanfolkCheats", ClanfolkCheats.Plugin.Version, "kk")]
[assembly: MelonGame]

namespace ClanfolkCheats
{
    public class Plugin : MelonMod
    {
        public const string Version = "0.1.0";
        internal static ModuleRegistry Registry = null!;
        internal static GuiManager     Gui      = null!;

        public override void OnInitializeMelon()
        {
            Registry = new ModuleRegistry();
            Registry.Add(new TimeCheats());
            Registry.Add(new ResourceCheats());
            Registry.Add(new BuildCheats());
            Registry.Add(new CharacterCheats());
            Registry.Add(new GodModeCheats());
            Registry.Add(new StorageCheats());

            var harmony = new HarmonyLib.Harmony("com.kk.clanfolk-cheats");
            Registry.RegisterAll(harmony);

            Gui = new GuiManager(Registry);
            LoggerInstance.Msg($"ClanfolkCheats v{Version} loaded ({Registry.Modules.Count} modules).");
        }

        public override void OnGUI()    => Gui?.OnGUI();
        public override void OnUpdate() { foreach (var m in Registry.Modules) m.OnUpdate(); }
    }
}
