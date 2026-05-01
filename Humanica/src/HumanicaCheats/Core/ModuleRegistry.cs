using System.Collections.Generic;

namespace HumanicaCheats.Core
{
    public class ModuleRegistry
    {
        private readonly List<ICheatModule> _modules = new();
        public IReadOnlyList<ICheatModule> Modules => _modules;

        public void Add(ICheatModule m) => _modules.Add(m);

        public void RegisterAll(HarmonyLib.Harmony harmony)
        {
            foreach (var m in _modules) m.Register(harmony);
        }
    }
}
