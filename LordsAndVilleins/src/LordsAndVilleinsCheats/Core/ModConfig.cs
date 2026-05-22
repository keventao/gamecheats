using BepInEx.Configuration;
using UnityEngine;

namespace LordsAndVilleinsCheats.Core
{
    public class ModConfig
    {
        public ConfigFile File { get; }

        public ConfigEntry<KeyCode> ToggleKey       { get; }
        public ConfigEntry<bool>    GlobalDisableAll { get; }
        public ConfigEntry<int>     PanelWidth      { get; }
        public ConfigEntry<int>     PanelHeight     { get; }

        public ModConfig(ConfigFile file)
        {
            File = file;
            ToggleKey        = file.Bind("General", "ToggleKey",        KeyCode.P,  "Hotkey to toggle the cheats panel.");
            GlobalDisableAll = file.Bind("General", "GlobalDisableAll", false,      "When true, all module Lock/Force behaviors are suppressed regardless of per-module settings.");
            PanelWidth       = file.Bind("UI",      "PanelWidth",       460,        "Cheats panel width in pixels.");
            PanelHeight      = file.Bind("UI",      "PanelHeight",      520,        "Cheats panel height in pixels.");
        }
    }
}
