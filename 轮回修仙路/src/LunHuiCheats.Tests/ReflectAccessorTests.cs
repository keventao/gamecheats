using LunHuiCheats.Core;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class ReflectAccessorTests
    {
        private class Dummy
        {
            public long Hp;                       // field
            public int Level { get; set; }        // property
            public float Speed { get; set; }
            private long Secret = 7;
            public long GetSecret() => Secret;
        }

        [Fact]
        public void Get_Field_And_Property()
        {
            var d = new Dummy { Hp = 100, Level = 5 };
            Assert.True(ReflectAccessor.TryGet(d, "Hp", out var hp));
            Assert.Equal(100L, hp);
            Assert.True(ReflectAccessor.TryGet(d, "Level", out var lv));
            Assert.Equal(5, lv);
        }

        [Fact]
        public void Set_Coerces_Int64_To_Int32()
        {
            var d = new Dummy();
            ReflectAccessor.SetInt64(d, "Level", 42L);   // Level is int
            Assert.Equal(42, d.Level);
        }

        [Fact]
        public void Set_Coerces_To_Single()
        {
            var d = new Dummy();
            ReflectAccessor.SetSingle(d, "Speed", 3.5f);
            Assert.Equal(3.5f, d.Speed);
        }

        [Fact]
        public void Missing_Member_Returns_False_And_Fallback()
        {
            var d = new Dummy();
            Assert.False(ReflectAccessor.TryGet(d, "Nope", out _));
            Assert.Equal(-1L, ReflectAccessor.GetInt64(d, "Nope", -1));
        }

        [Fact]
        public void Reads_Private_Field()
        {
            var d = new Dummy();
            Assert.Equal(7L, ReflectAccessor.GetInt64(d, "Secret"));
        }
    }
}
