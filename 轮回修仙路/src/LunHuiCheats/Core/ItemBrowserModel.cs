using System.Collections.Generic;
using System.Linq;

namespace LunHuiCheats.Core
{
    public sealed class ItemRow
    {
        public string Name { get; }
        public string Category { get; }
        public object Payload { get; }   // game BaseRewardData / CoinData reference

        public ItemRow(string name, string category, object payload)
        {
            Name = name;
            Category = category;
            Payload = payload;
        }
    }

    public sealed class ItemBrowserModel
    {
        public const string AllCategories = "全部";

        private readonly List<ItemRow> _all = new();

        public string Query = "";
        public string SelectedCategory = AllCategories;
        public SortKey Sort = SortKey.Name;

        public void SetRows(IEnumerable<ItemRow> rows)
        {
            _all.Clear();
            if (rows != null) _all.AddRange(rows);
        }

        public IReadOnlyList<string> Categories()
        {
            var cats = _all.Select(r => r.Category).Distinct().ToList();
            cats.Insert(0, AllCategories);
            return cats;
        }

        public IReadOnlyList<ItemRow> Visible()
        {
            IEnumerable<ItemRow> seq = _all;
            if (SelectedCategory != AllCategories)
                seq = seq.Where(r => r.Category == SelectedCategory);
            return FilterSort.Apply(seq, Query, Sort, r => r.Name, r => r.Category, _ => 0);
        }
    }
}
