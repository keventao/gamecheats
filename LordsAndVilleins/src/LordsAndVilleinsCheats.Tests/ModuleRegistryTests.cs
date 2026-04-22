using System.Collections.Generic;
using HarmonyLib;
using LordsAndVilleinsCheats.Core;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class ModuleRegistryTests
    {
        private class FakeModule : ICheatModule
        {
            public string Id   => "fake";
            public string Name => "Fake";
            public ModuleStatus Status { get; set; } = ModuleStatus.Pending;
            public bool RegisterCalled, OnGameReadyCalled, DrawGuiCalled, DisableAllCalled;

            public void Register(ModConfig cfg, Harmony harmony) { RegisterCalled = true; Status = ModuleStatus.Ok; }
            public void OnGameReady() => OnGameReadyCalled = true;
            public void DrawGui()     => DrawGuiCalled = true;
            public void DisableAll()  { DisableAllCalled = true; Status = ModuleStatus.Disabled; }
        }

        [Fact]
        public void Register_CallsEachModuleRegister()
        {
            var reg = new ModuleRegistry();
            var m = new FakeModule();
            reg.Add(m);
            reg.RegisterAll(cfg: null!, harmony: null!);
            Assert.True(m.RegisterCalled);
            Assert.Equal(ModuleStatus.Ok, m.Status);
        }

        [Fact]
        public void DisableAll_CallsEachModuleDisableAll()
        {
            var reg = new ModuleRegistry();
            var m1 = new FakeModule();
            var m2 = new FakeModule();
            reg.Add(m1); reg.Add(m2);
            reg.DisableAll();
            Assert.True(m1.DisableAllCalled);
            Assert.True(m2.DisableAllCalled);
        }

        [Fact]
        public void NotifyGameReady_CallsOnGameReadyExactlyOnce_AcrossMultipleCalls()
        {
            var reg = new ModuleRegistry();
            var m = new FakeModule();
            reg.Add(m);
            reg.NotifyGameReady();
            reg.NotifyGameReady();
            Assert.True(m.OnGameReadyCalled);
            m.OnGameReadyCalled = false;
            reg.NotifyGameReady();
            Assert.False(m.OnGameReadyCalled);
        }
    }
}
