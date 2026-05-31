using System.Collections.Generic;
using System.Linq;
using LunHuiCheats.Core;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class FilterSortTests
    {
        private record Row(string Name, string Cat, int Recent);

        private static List<Row> Sample() => new()
        {
            new("丹药A", "丹药", 1),
            new("装备B", "装备", 3),
            new("丹药C", "丹药", 2),
        };

        private static List<Row> Run(string q, SortKey k) =>
            FilterSort.Apply(Sample(), q, k, r => r.Name, r => r.Cat, r => r.Recent);

        [Fact]
        public void Empty_Query_Returns_All_Sorted_By_Name()
        {
            var r = Run("", SortKey.Name);
            Assert.Equal(new[] { "丹药A", "丹药C", "装备B" }, r.Select(x => x.Name).ToArray());
        }

        [Fact]
        public void Query_Filters_By_Substring()
        {
            var r = Run("丹药", SortKey.Name);
            Assert.Equal(2, r.Count);
            Assert.All(r, x => Assert.Contains("丹药", x.Name));
        }

        [Fact]
        public void Sort_By_Recent_Descends()
        {
            var r = Run("", SortKey.Recent);
            Assert.Equal(new[] { "装备B", "丹药C", "丹药A" }, r.Select(x => x.Name).ToArray());
        }

        [Fact]
        public void Sort_By_Category_Then_Name()
        {
            var r = Run("", SortKey.Category);
            Assert.Equal(new[] { "丹药A", "丹药C", "装备B" }, r.Select(x => x.Name).ToArray());
        }

        [Fact]
        public void Null_Items_Yields_Empty()
        {
            var r = FilterSort.Apply<Row>(null!, "", SortKey.Name, x => x.Name, x => x.Cat, x => x.Recent);
            Assert.Empty(r);
        }
    }
}
