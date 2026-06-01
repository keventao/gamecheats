using ClanfolkCheats.Core;

namespace ClanfolkCheats.Modules
{
    public class CharacterCheats : ICheatModule
    {
        public string       Name   => "角色";
        public ModuleStatus Status { get; private set; } = ModuleStatus.Pending;

        public void Register(HarmonyLib.Harmony harmony)
        {
            // TODO: patch character stat getters/setters after decompile
        }

        public void DrawGui(Layout l)
        {
            l.Label("[SKELETON] Character module — needs game API research.");
            l.Space(4);
            l.Label("Planned features:");
            l.Label("  - Health control (set/max).");
            l.Label("  - Mood/happiness control.");
            l.Label("  - Skill level modification.");
            l.Label("  - Age/death prevention.");
            l.Space(8);
            l.Label("Next step: decompile game with dnSpy to find");
            l.Label("character/pawn API entry points.");
        }
    }
}
