using System.Linq;
using LunHuiCheats.Core;
using Xunit;

namespace LunHuiCheats.Tests
{
    public class ItemBrowserModelTests
    {
        private static ItemBrowserModel Built()
        {
            var m = new ItemBrowserModel();
            m.SetRows(new[]
            {
                new ItemRow("回血丹", "丹药", new object()),
                new ItemRow("铁剑",   "装备", new object()),
                new ItemRow("聚气丹", "丹药", new object()),
            });
            return m;
        }

        [Fact]
        public void Categories_Lead_With_All_Then_Distinct()
        {
            var m = Built();
            Assert.Equal(new[] { "全部", "丹药", "装备" }, m.Categories().ToArray());
        }

        [Fact]
        public void Visible_All_Category_Sorted_By_Name()
        {
            var m = Built();
            Assert.Equal(new[] { "回血丹", "聚气丹", "铁剑" }, m.Visible().Select(r => r.Name).ToArray());
        }

        [Fact]
        public void Visible_Filtered_By_Category()
        {
            var m = Built();
            m.SelectedCategory = "丹药";
            Assert.Equal(2, m.Visible().Count);
            Assert.All(m.Visible(), r => Assert.Equal("丹药", r.Category));
        }

        [Fact]
        public void Visible_Filtered_By_Query()
        {
            var m = Built();
            m.Query = "聚气";
            Assert.Single(m.Visible());
            Assert.Equal("聚气丹", m.Visible()[0].Name);
        }
    }
}
