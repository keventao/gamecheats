using System.Collections.Generic;
using UnityEngine;

namespace LunHuiCheats.Core.Gui
{
    /// <summary>A top-down layout cursor for Rect-based IMGUI (no GUILayout).</summary>
    public struct LayoutCursor
    {
        public float X, Y, Width, LineHeight, Pad;

        public LayoutCursor(float x, float y, float width, float lineHeight = 24f, float pad = 4f)
        { X = x; Y = y; Width = width; LineHeight = lineHeight; Pad = pad; }

        public Rect Line(float? height = null)
        {
            var h = height ?? LineHeight;
            var r = new Rect(X, Y, Width, h);
            Y += h + Pad;
            return r;
        }

        public Rect Slice(ref Rect line, float w, float gap = 4f)
        {
            var r = new Rect(line.x, line.y, w, line.height);
            line.x += w + gap;
            line.width -= w + gap;
            return r;
        }
    }

    /// <summary>Rect-based widget helpers. Number fields keep per-control text buffers.</summary>
    public static class GuiWidgets
    {
        private static readonly Dictionary<string, string> _buffers = new();

        public static void Label(Rect r, string text) => GUI.Label(r, text);

        public static bool Button(Rect r, string text) => GUI.Button(r, text);

        public static bool Toggle(Rect r, bool value, string label)
            => GUI.Toggle(r, value, " " + label);

        public static float Slider(Rect r, float value, float min, float max)
            => GUI.HorizontalSlider(r, value, min, max);

        /// <summary>
        /// Editable Int64 field. `id` must be unique per logical field so the text
        /// buffer survives between frames while the user types.
        /// </summary>
        public static long Int64Field(Rect r, string id, long value)
        {
            if (!_buffers.TryGetValue(id, out var buf) || !GUI.GetNameOfFocusedControl().Equals(id))
                buf = value.ToString();

            GUI.SetNextControlName(id);
            var text = GUI.TextField(r, buf);
            _buffers[id] = text;

            return long.TryParse(text, out var parsed) ? parsed : value;
        }
    }
}
