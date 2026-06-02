using Xunit;
using ClanfolkCheats.Core;

namespace ClanfolkCheats.Tests
{
    public class ReflectAccessorTests
    {
        private class TestObj
        {
            public string Name { get; set; } = "";
            public int Age { get; set; }
            public float Height { get; set; }
            private bool _hidden;
            public bool Hidden { get => _hidden; set => _hidden = value; }
        }

        [Fact]
        public void TryGet_PublicProperty_ReturnsValue()
        {
            var obj = new TestObj { Name = "Alice", Age = 30 };
            Assert.True(ReflectAccessor.TryGet(obj, "Name", out var v));
            Assert.Equal("Alice", v);
        }

        [Fact]
        public void TryGet_MissingMember_ReturnsFalse()
        {
            var obj = new TestObj();
            Assert.False(ReflectAccessor.TryGet(obj, "NonExistent", out _));
        }

        [Fact]
        public void TryGet_NullInstance_ReturnsFalse()
        {
            Assert.False(ReflectAccessor.TryGet(null, "Name", out _));
        }

        [Fact]
        public void TrySet_PublicProperty_SetsValue()
        {
            var obj = new TestObj();
            Assert.True(ReflectAccessor.TrySet(obj, "Name", "Bob"));
            Assert.Equal("Bob", obj.Name);
        }

        [Fact]
        public void TrySet_ReadonlyProperty_ReturnsFalse()
        {
            var obj = new TestObj();
            Assert.False(ReflectAccessor.TrySet(obj, "NonExistent", "x"));
        }

        [Fact]
        public void GetInt64_FromInt32_Converts()
        {
            var obj = new TestObj { Age = 42 };
            Assert.Equal(42L, ReflectAccessor.GetInt64(obj, "Age"));
        }

        [Fact]
        public void GetInt64_MissingMember_ReturnsFallback()
        {
            var obj = new TestObj();
            Assert.Equal(99L, ReflectAccessor.GetInt64(obj, "Nope", 99));
        }

        [Fact]
        public void SetInt64_CoercesValue()
        {
            var obj = new TestObj();
            ReflectAccessor.SetInt64(obj, "Age", 100L);
            Assert.Equal(100, obj.Age);
        }

        [Fact]
        public void GetSingle_FromFloat_ReturnsValue()
        {
            var obj = new TestObj { Height = 1.75f };
            Assert.Equal(1.75f, ReflectAccessor.GetSingle(obj, "Height"));
        }

        [Fact]
        public void GetSingle_MissingMember_ReturnsFallback()
        {
            var obj = new TestObj();
            Assert.Equal(3.14f, ReflectAccessor.GetSingle(obj, "Nope", 3.14f));
        }

        [Fact]
        public void GetInt32_ReturnsValue()
        {
            var obj = new TestObj { Age = 25 };
            Assert.Equal(25, ReflectAccessor.GetInt32(obj, "Age"));
        }

        [Fact]
        public void GetInt32_MissingMember_ReturnsFallback()
        {
            var obj = new TestObj();
            Assert.Equal(-1, ReflectAccessor.GetInt32(obj, "Nope", -1));
        }

        [Fact]
        public void TryGet_CachesLookups()
        {
            var a = new TestObj { Name = "A" };
            var b = new TestObj { Name = "B" };
            Assert.True(ReflectAccessor.TryGet(a, "Name", out var va));
            Assert.Equal("A", va);
            Assert.True(ReflectAccessor.TryGet(b, "Name", out var vb));
            Assert.Equal("B", vb);
        }
    }
}
