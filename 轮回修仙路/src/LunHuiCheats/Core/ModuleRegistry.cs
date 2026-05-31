using System.Collections.Generic;
using HarmonyLib;

namespace LunHuiCheats.Core
{
    public class ModuleRegistry
    {
        private readonly List<ICheatModule> _modules = new();
        private bool _gameReadyDispatched;

        public IReadOnlyList<ICheatModule> Modules => _modules;

        public void Add(ICheatModule module) => _modules.Add(module);

        public void RegisterAll(ModConfig cfg, Harmony harmony)
        {
            foreach (var m in _modules) m.Register(cfg, harmony);
        }

        public void NotifyGameReady()
        {
            if (_gameReadyDispatched) return;
            _gameReadyDispatched = true;
            foreach (var m in _modules) m.OnGameReady();
        }

        public void ResetGameReady() => _gameReadyDispatched = false;

        public void DisableAll()
        {
            foreach (var m in _modules) m.DisableAll();
        }
    }
}
