using ClanfolkCheats.Core;

namespace ClanfolkCheats.Modules
{
    public class BuildCheats : ICheatModule
    {
        public string       Name   => "建造";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        public void Register(HarmonyLib.Harmony harmony)
        {
            // TODO: patch build/construct time methods after decompile
        }

        public void DrawGui(Layout l)
        {
            l.Label("[SKELETON] Build module — needs game API research.");
            l.Space(4);
            l.Label("Planned features:");
            l.Label("  - Instant build (construction time → 0).");
            l.Label("  - Free build (no resource cost).");
            l.Label("  - Build speed multiplier.");
            l.Space(8);
            l.Label("Next step: decompile game with dnSpy to find");
            l.Label("construction/build API entry points.");
        }
    }
}
