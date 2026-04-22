using System;
using LordsAndVilleinsCheats.Util;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class HarmonyHelpersTests
    {
        [Fact]
        public void SafeRun_ReturnsTrue_WhenActionSucceeds()
        {
            var result = HarmonyHelpers.SafeRun("test-op", () => { });
            Assert.True(result);
        }

        [Fact]
        public void SafeRun_ReturnsFalse_AndSwallows_WhenActionThrows()
        {
            var result = HarmonyHelpers.SafeRun("test-op", () => throw new InvalidOperationException("boom"));
            Assert.False(result);
        }
    }
}
