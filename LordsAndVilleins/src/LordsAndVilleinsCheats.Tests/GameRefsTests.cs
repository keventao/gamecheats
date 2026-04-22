using LordsAndVilleinsCheats.Core;
using Xunit;

namespace LordsAndVilleinsCheats.Tests
{
    public class GameRefsTests
    {
        [Fact]
        public void Reset_ClearsIsReady()
        {
            GameRefs.IsReady = true;
            GameRefs.Reset();
            Assert.False(GameRefs.IsReady);
        }
    }
}
