using System.Collections.Generic;
using HarmonyLib;

namespace ForTheKingCheats.Core
{
    public sealed class ModuleRegistry
    {
        private readonly List<ICheatModule> _modules = new List<ICheatModule>();

        public IList<ICheatModule> Modules => _modules;

        public void Add(ICheatModule module)
        {
            _modules.Add(module);
        }

        public void RegisterAll(Harmony harmony)
        {
            foreach (var module in _modules)
            {
                module.Register(harmony);
            }
        }
    }
}
