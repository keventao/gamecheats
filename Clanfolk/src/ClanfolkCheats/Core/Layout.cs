using UnityEngine;

namespace ClanfolkCheats.Core
{
    public class Layout
    {
        public float X;
        public float Y;
        public float Width;

        public Layout(float x, float y, float width) { X = x; Y = y; Width = width; }

        public void Label(string text, float h = 22f)
        {
            GUI.Label(new Rect(X, Y, Width, h), text);
            Y += h + 2f;
        }

        public bool Button(string text, float h = 24f)
        {
            bool clicked = ImguiUtil.Button(new Rect(X, Y, Width, h), text);
            Y += h + 4f;
            return clicked;
        }

        public bool Toggle(bool value, string text, float h = 22f)
        {
            bool newVal = ImguiUtil.Toggle(new Rect(X, Y, Width, h), value, text);
            Y += h + 2f;
            return newVal;
        }

        public void Space(float px) { Y += px; }
    }

    public static class ImguiUtil
    {
        public static bool Button(Rect r, string label, bool active = false)
        {
            var prev = GUI.color;
            GUI.color = active ? Color.cyan : Color.white;
            GUI.Box(r, label);
            GUI.color = prev;

            var ev = Event.current;
            if (ev != null && ev.type == EventType.MouseDown && ev.button == 0
                && r.Contains(ev.mousePosition))
            {
                ev.Use();
                return true;
            }
            return false;
        }

        public static bool Toggle(Rect r, bool value, string label)
        {
            const float boxW = 18f;
            var boxRect = new Rect(r.x, r.y + (r.height - boxW) / 2f, boxW, boxW);
            var lblRect = new Rect(r.x + boxW + 6f, r.y, r.width - boxW - 6f, r.height);
            GUI.Box(boxRect, value ? "X" : "");
            GUI.Label(lblRect, label);

            var ev = Event.current;
            if (ev != null && ev.type == EventType.MouseDown && ev.button == 0
                && r.Contains(ev.mousePosition))
            {
                ev.Use();
                return !value;
            }
            return value;
        }
    }
}
