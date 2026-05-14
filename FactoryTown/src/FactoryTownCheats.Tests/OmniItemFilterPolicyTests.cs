using FactoryTownCheats.Core;
using Xunit;

namespace FactoryTownCheats.Tests
{
    public sealed class OmniItemFilterPolicyTests
    {
        [Fact]
        public void IsCandidateOutput_RejectsSentinelFilterAndUtilityItems()
        {
            Assert.False(OmniItemFilterPolicy.IsCandidateOutput("None"));
            Assert.False(OmniItemFilterPolicy.IsCandidateOutput("Invalid"));
            Assert.False(OmniItemFilterPolicy.IsCandidateOutput("FilterCurrency"));
            Assert.False(OmniItemFilterPolicy.IsCandidateOutput("UtilityExperiencePoint"));
        }

        [Fact]
        public void IsCandidateOutput_AllowsPhysicalCurrenciesResearchPointsAndWorkers()
        {
            Assert.True(OmniItemFilterPolicy.IsCandidateOutput("Wood"));
            Assert.True(OmniItemFilterPolicy.IsCandidateOutput("Gold"));
            Assert.True(OmniItemFilterPolicy.IsCandidateOutput("ResearchPointsGeneral"));
            Assert.True(OmniItemFilterPolicy.IsCandidateOutput("Worker"));
        }

        [Fact]
        public void RecipeIdForOutput_UsesStableHighRangeOutsideKnownRecipeEnum()
        {
            Assert.Equal(50042, OmniItemFilterPolicy.RecipeIdForOutput(42));
        }
    }
}
