// HarmonyLib.Harmony 全限定:0Harmony.dll 顶层有 `Harmony` 命名空间会与类同名冲突。
namespace HumanicaCheats.Core
{
    public interface ICheatModule
    {
        string Name   { get; }
        ModuleStatus Status { get; }
        void Register(HarmonyLib.Harmony harmony);
        void DrawGui();
        void OnUpdate() { } // 默认空实现,需要的模块覆盖
    }
}
