using ClanfolkCheats.Core;

namespace ClanfolkCheats.Modules
{
    public class GodModeCheats : ICheatModule
    {
        public string       Name   => "上帝";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        public void Register(HarmonyLib.Harmony harmony)
        {
            // TODO: patch damage/death methods after decompile
        }

        public void DrawGui(Layout l)
        {
            l.Label("[SKELETON] God Mode module — needs game API research.");
            l.Space(4);
            l.Label("Planned features:");
            l.Label("  - Invulnerability (no damage).");
            l.Label("  - No freezing/starvation.");
            l.Label("  - No aging.");
            l.Space(8);
            l.Label("Next step: decompile game with dnSpy to find");
            l.Label("damage/death/needs API entry points.");
        }
    }
}
