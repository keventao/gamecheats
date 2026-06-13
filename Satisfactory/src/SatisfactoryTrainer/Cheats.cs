using System;
using System.Collections.Generic;

namespace SatisfactoryTrainer
{
    /// <summary>
    /// Holds the two cheats and applies them against the live object graph.
    /// Class pointers and instance lists are cached and refreshed on an interval
    /// so the per-tick cost stays low.
    /// </summary>
    public sealed class CheatEngine
    {
        private readonly ProcessMemory _mem;
        private readonly UnrealRuntime _ue;

        private long _onlineBackendClass;
        private long _workBenchClass;

        private List<long> _backendInstances = new();
        private List<long> _workBenchInstances = new();

        private int _ticksSinceRescan = int.MaxValue;

        public bool AchievementsEnabled { get; set; }

        public bool InstantCraftEnabled { get; set; }

        /// <summary>Rescan the object graph every N ticks (cheap amortization).</summary>
        public int RescanEveryTicks { get; set; } = 20;

        public CheatEngine(ProcessMemory mem, UnrealRuntime ue)
        {
            _mem = mem;
            _ue = ue;
        }

        public int BackendCount => _backendInstances.Count;

        public int WorkBenchCount => _workBenchInstances.Count;

        /// <summary>Resolve the class pointers once. Returns false if not found.</summary>
        public bool ResolveClasses(out string error)
        {
            _onlineBackendClass = _ue.FindClass(Offsets.ClassOnlineIntegrationBackend);
            _workBenchClass = _ue.FindClass(Offsets.ClassWorkBench);

            if (_onlineBackendClass == 0 && _workBenchClass == 0)
            {
                error = "Could not find either target class — load a save first, then attach.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void RescanIfDue()
        {
            if (_ticksSinceRescan < RescanEveryTicks)
            {
                _ticksSinceRescan++;
                return;
            }

            _ticksSinceRescan = 0;

            if (_onlineBackendClass != 0)
            {
                _backendInstances = _ue.FindInstancesOfClass(_onlineBackendClass);
            }

            if (_workBenchClass != 0)
            {
                _workBenchInstances = _ue.FindInstancesOfClass(_workBenchClass);
            }
        }

        /// <summary>Apply all enabled cheats for this tick.</summary>
        public void Tick()
        {
            if (!AchievementsEnabled && !InstantCraftEnabled)
            {
                return;
            }

            RescanIfDue();

            if (AchievementsEnabled)
            {
                ApplyAchievements();
            }

            if (InstantCraftEnabled)
            {
                ApplyInstantCraft();
            }
        }

        // Force bSuppressAchievements = false on every backend instance.
        private void ApplyAchievements()
        {
            foreach (long backend in _backendInstances)
            {
                try
                {
                    long addr = backend + Offsets.OnlineBackend_bSuppressAchievements;
                    if (_mem.ReadByte(addr) != 0)
                    {
                        _mem.WriteByte(addr, 0);
                    }
                }
                catch (Exception)
                {
                    // instance went away between rescans — ignore, next rescan fixes it
                }
            }
        }

        // While a bench is actively crafting, snap progress to complete.
        // Least-destructive lever (option B in refs/RE-notes.md): we don't change
        // speed permanently, just push the in-flight craft to done.
        private void ApplyInstantCraft()
        {
            foreach (long bench in _workBenchInstances)
            {
                try
                {
                    bool producing = _mem.ReadByte(bench + Offsets.WorkBench_mIsProducing) != 0;
                    long recipe = _mem.ReadPtr(bench + Offsets.WorkBench_mCurrentRecipe);
                    if (!producing || recipe == 0)
                    {
                        continue;
                    }

                    long progAddr = bench + Offsets.WorkBench_mCurrentManufacturingProgress;
                    if (_mem.ReadFloat(progAddr) < 1.0f)
                    {
                        _mem.WriteFloat(progAddr, 1.0f);
                    }
                }
                catch (Exception)
                {
                    // ignore transient failures
                }
            }
        }
    }
}
