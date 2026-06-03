using System;
using System.IO;
using BepInEx.Configuration;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class ModuleRegistryTests
    {
        [Fact]
        public void Add_Module_CountIncreases()
        {
            var registry = new Core.ModuleRegistry();
            registry.Add(new TestModule());
            Assert.Equal(1, registry.Modules.Count);
        }

        [Fact]
        public void RegisterAll_SetsStatus()
        {
            var registry = new Core.ModuleRegistry();
            var module = new TestModule();
            registry.Add(module);
            var cfgPath = Path.Combine(Path.GetTempPath(), $"lunhui-tests-{Guid.NewGuid():N}.cfg");
            try
            {
                registry.RegisterAll(new Core.ModConfig(new ConfigFile(cfgPath, false)), new HarmonyLib.Harmony("test"));
            }
            finally
            {
                if (File.Exists(cfgPath)) File.Delete(cfgPath);
            }
            Assert.Equal(Core.ModuleStatus.Ok, module.Status);
        }

        [Fact]
        public void OnUpdateAll_Calls_Each_Module()
        {
            var registry = new Core.ModuleRegistry();
            var m = new CountingModule();
            registry.Add(m);
            registry.OnUpdateAll();
            registry.OnUpdateAll();
            Assert.Equal(2, m.Updates);
        }

        private class CountingModule : Core.ICheatModule
        {
            public int Updates;
            public string Id => "count";
            public string Name => "Count";
            public string Category => "测试";
            public Core.ModuleStatus Status => Core.ModuleStatus.Ok;
            public void Register(Core.ModConfig cfg, HarmonyLib.Harmony harmony) { }
            public void OnGameReady() { }
            public void OnUpdate() => Updates++;
            public void DrawGui() { }
            public void DisableAll() { }
        }

        private class TestModule : Core.ICheatModule
        {
            public string Id => "test";
            public string Name => "Test";
            public string Category => "测试";
            public Core.ModuleStatus Status { get; set; } = Core.ModuleStatus.Pending;
            public void Register(Core.ModConfig cfg, HarmonyLib.Harmony harmony) => Status = Core.ModuleStatus.Ok;
            public void OnGameReady() { }
            public void OnUpdate() { }
            public void DrawGui() { }
            public void DisableAll() { }
        }
    }
}
