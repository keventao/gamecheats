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

        public void OnUpdateAll()
        {
            foreach (var m in _modules)
            {
                try { m.OnUpdate(); }
                catch { /* a faulty module must not break the frame loop */ }
            }
        }

        public IReadOnlyList<string> Categories()
        {
            var seen = new List<string>();
            foreach (var m in _modules)
                if (!seen.Contains(m.Category)) seen.Add(m.Category);
            return seen;
        }

        public void DisableAll()
        {
            foreach (var m in _modules) m.DisableAll();
        }
    }
}
