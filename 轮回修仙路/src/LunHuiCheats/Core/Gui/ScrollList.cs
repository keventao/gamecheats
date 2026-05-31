using System;
using UnityEngine;

namespace LunHuiCheats.Core.Gui
{
    /// <summary>
    /// Wraps GUI.BeginScrollView for a vertical list of fixed-height rows.
    /// Holds its own scroll position; one instance per scrolling region.
    /// </summary>
    public sealed class ScrollList
    {
        private Vector2 _scroll;

        /// <param name="viewport">on-screen rect</param>
        /// <param name="rowCount">number of rows</param>
        /// <param name="rowHeight">height per row</param>
        /// <param name="drawRow">callback (index, rowRect) drawn in content space</param>
        public void Draw(Rect viewport, int rowCount, float rowHeight, Action<int, Rect> drawRow)
        {
            var contentHeight = Mathf.Max(viewport.height, rowCount * rowHeight);
            var content = new Rect(0, 0, viewport.width - 16, contentHeight);
            _scroll = GUI.BeginScrollView(viewport, _scroll, content);
            for (int i = 0; i < rowCount; i++)
                drawRow(i, new Rect(0, i * rowHeight, content.width, rowHeight - 2));
            GUI.EndScrollView();
        }
    }
}
