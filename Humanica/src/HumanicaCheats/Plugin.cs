using MelonLoader;
using HumanicaCheats.Core;
using HumanicaCheats.Modules;

[assembly: MelonInfo(typeof(HumanicaCheats.Plugin), "HumanicaCheats", HumanicaCheats.Plugin.Version, "kk")]
[assembly: MelonGame]

namespace HumanicaCheats
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
            // VillageCheats 在 Task 5 后添加
            Gui = new GuiManager(Registry);
            LoggerInstance.Msg($"HumanicaCheats v{Version} 已加载 ({Registry.Modules.Count} 模块)。");
        }

        public override void OnGUI()    => Gui?.OnGUI();
        public override void OnUpdate() { foreach (var m in Registry.Modules) m.OnUpdate(); }
    }
}
