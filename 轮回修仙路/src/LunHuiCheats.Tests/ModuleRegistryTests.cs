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
            registry.RegisterAll(new Core.ModConfig(null!), new HarmonyLib.Harmony("test"));
            Assert.Equal(Core.ModuleStatus.Ok, module.Status);
        }

        private class TestModule : Core.ICheatModule
        {
            public string Id => "test";
            public string Name => "Test";
            public Core.ModuleStatus Status { get; set; } = Core.ModuleStatus.Pending;
            public void Register(Core.ModConfig cfg, HarmonyLib.Harmony harmony) => Status = Core.ModuleStatus.Ok;
            public void OnGameReady() { }
            public void DrawGui() { }
            public void DisableAll() { }
        }
    }
}
