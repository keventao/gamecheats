using HarmonyLib;

namespace LordsAndVilleinsCheats.Core
{
    public interface ICheatModule
    {
        string Id   { get; }
        string Name { get; }
        ModuleStatus Status { get; }

        void Register(ModConfig cfg, Harmony harmony);
        void OnGameReady();
        void DrawGui();
        void DisableAll();
    }
}
