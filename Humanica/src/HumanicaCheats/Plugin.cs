using MelonLoader;
using HumanicaCheats.Core;

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
            // 模块在 Task 3-6 中逐步添加
            Gui = new GuiManager(Registry);
            LoggerInstance.Msg($"HumanicaCheats v{Version} 已加载。");
        }

        public override void OnGUI() => Gui?.OnGUI();
    }
}
