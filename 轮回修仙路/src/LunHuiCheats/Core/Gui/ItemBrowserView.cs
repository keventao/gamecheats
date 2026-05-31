using System;
using UnityEngine;

namespace LunHuiCheats.Core.Gui
{
    /// <summary>
    /// Renders an ItemBrowserModel: category buttons, a search box, a sort toggle,
    /// and a scrollable list of rows each with a quantity field + Add button.
    /// </summary>
    public sealed class ItemBrowserView
    {
        private readonly ScrollList _list = new();
        private long _qty = 1;

        public void Draw(Rect area, ItemBrowserModel model, Action<ItemRow, long> onAdd)
        {
            var c = new LayoutCursor(area.x, area.y, area.width);

            // Category row
            var catLine = c.Line();
            foreach (var cat in model.Categories())
            {
                var btn = c.Slice(ref catLine, 72f);
                var prev = GUI.color;
                if (model.SelectedCategory == cat) GUI.color = new Color(0.3f, 0.6f, 1f);
                if (GUI.Button(btn, cat)) model.SelectedCategory = cat;
                GUI.color = prev;
                if (catLine.width < 72f) break;
            }

            // Search + sort + qty
            var ctl = c.Line();
            GUI.Label(c.Slice(ref ctl, 40f), "搜索");
            model.Query = GUI.TextField(c.Slice(ref ctl, 140f), model.Query ?? "");
            if (GUI.Button(c.Slice(ref ctl, 80f), $"排序:{model.Sort}"))
                model.Sort = (SortKey)(((int)model.Sort + 1) % 3);
            GUI.Label(c.Slice(ref ctl, 40f), "数量");
            _qty = GuiWidgets.Int64Field(c.Slice(ref ctl, 80f), "itembrowser.qty", _qty);

            // List
            var rows = model.Visible();
            var listTop = c.Line();
            var viewport = new Rect(area.x, listTop.y, area.width, area.yMax - listTop.y);
            _list.Draw(viewport, rows.Count, 26f, (i, rowRect) =>
            {
                var row = rows[i];
                var nameRect = new Rect(rowRect.x, rowRect.y, rowRect.width - 70, rowRect.height);
                var addRect  = new Rect(rowRect.xMax - 64, rowRect.y, 60, rowRect.height);
                GUI.Label(nameRect, $"{row.Name}  ({row.Category})");
                if (GUI.Button(addRect, "Add")) onAdd(row, _qty);
            });
        }
    }
}
