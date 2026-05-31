using HarmonyLib;

namespace LunHuiCheats.Core
{
    public interface ICheatModule
    {
        string Id       { get; }
        string Name     { get; }
        string Category { get; }
        ModuleStatus Status { get; }

        void Register(ModConfig cfg, Harmony harmony);
        void OnGameReady();
        void OnUpdate();
        void DrawGui();
        void DisableAll();
    }
}
