using ClanfolkCheats.Core;

namespace ClanfolkCheats.Modules
{
    public class StorageCheats : ICheatModule
    {
        public string       Name   => "存储";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        public void Register(HarmonyLib.Harmony harmony)
        {
            // TODO: patch storage capacity methods after decompile
        }

        public void DrawGui(Layout l)
        {
            l.Label("[SKELETON] Storage module — needs game API research.");
            l.Space(4);
            l.Label("Planned features:");
            l.Label("  - Storage capacity multiplier.");
            l.Label("  - Stack size multiplier.");
            l.Label("  - Inventory size control.");
            l.Space(8);
            l.Label("Next step: decompile game with dnSpy to find");
            l.Label("storage/inventory API entry points.");
        }
    }
}
