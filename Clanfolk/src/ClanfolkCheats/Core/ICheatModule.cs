namespace ClanfolkCheats.Core
{
    public interface ICheatModule
    {
        string       Name   { get; }
        ModuleStatus Status { get; }
        void Register(HarmonyLib.Harmony harmony);
        void DrawGui(Layout l);
        void OnUpdate() { }
    }
}
