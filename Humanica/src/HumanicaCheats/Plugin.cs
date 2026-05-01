using MelonLoader;
using HumanicaCheats.Core;
using HumanicaCheats.Modules;

[assembly: MelonInfo(typeof(HumanicaCheats.Plugin), "HumanicaCheats", HumanicaCheats.Plugin.Version, "kk")]
[assembly: MelonGame]

namespace HumanicaCheats
{
    public class Plugin : MelonMod
    {
        public const string Version = "0.1.1";
        internal static ModuleRegistry Registry = null!;
        internal static GuiManager     Gui      = null!;

        public override void OnInitializeMelon()
        {
            Registry = new ModuleRegistry();
            Registry.Add(new TimeCheats());
            Registry.Add(new ResourceCheats());
            Registry.Add(new VillageCheats());
            Registry.Add(new UnlockCheats());

            // 触发各模块的 Harmony patch 注册 (VillageCheats 依赖)
            var harmony = new HarmonyLib.Harmony("com.kk.humanica-cheats");
            Registry.RegisterAll(harmony);

            Gui = new GuiManager(Registry);
            LoggerInstance.Msg($"HumanicaCheats v{Version} 已加载 ({Registry.Modules.Count} 模块)。");

            // 启动时 dump 全部 ResourceIndex 枚举名/idx 到日志,便于校准 ResourceCheats 配置。
            ResourceCheats.DumpResourceIndex();
        }

        public override void OnGUI()    => Gui?.OnGUI();
        public override void OnUpdate() { foreach (var m in Registry.Modules) m.OnUpdate(); }
    }
}
