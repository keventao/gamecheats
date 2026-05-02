using HarmonyLib;

namespace ForTheKingCheats.Core
{
    public interface ICheatModule
    {
        string Name { get; }
        ModuleStatus Status { get; }
        void Register(Harmony harmony);
        void Draw();
    }
}
