using FactoryTownCheats.Core;
using Xunit;

namespace FactoryTownCheats.Tests
{
    public sealed class OmniFactoryDefinitionTests
    {
        [Fact]
        public void CustomBuildingId_UsesStableHighRange()
        {
            Assert.Equal(90000, OmniFactoryDefinition.CustomBuildingId);
        }

        [Theory]
        [InlineData(27)]
        [InlineData(56)]
        public void CustomBuildingId_DoesNotReuseVanillaBuildings(int vanillaBuildingId)
        {
            Assert.NotEqual(OmniFactoryDefinition.CustomBuildingId, vanillaBuildingId);
        }

        [Fact]
        public void DisplayName_IsExplicitCheatBuildingName()
        {
            Assert.Equal("KK 万能工坊", OmniFactoryDefinition.DisplayName);
        }
    }
}
