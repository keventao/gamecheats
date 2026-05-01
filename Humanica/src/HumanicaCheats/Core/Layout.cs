using UnityEngine;

namespace HumanicaCheats.Core
{
    // 竖向 cursor 布局 helper + 自实现的命中检测控件。
    //
    // 为何全部自实现?这个版本的 MelonLoader (0.7.2) + Il2CppInterop 给本游戏 Unity 二进制
    // 生成的 UnityEngine.IMGUIModule 桩有两层坑:
    //   (1) GUILayout.* 一族(BeginHorizontal、Window 等)走 GUILayoutUtility.BeginLayoutGroup,
    //       该路径触发 ExitGUIException..ctor / LayoutedWindow..ctor 等内部 ctor 签名不匹配,
    //       直接抛 MissingMethodException
    //   (2) GUI.Button / GUI.Toggle / GUI.Window 这类有返回值的方法:绘制能跑、IMGUI 内部
    //       hot control 状态也对(诊断日志可见 hot 翻转、Event 被 Used),但 bool/Rect
    //       返回值在 trampoline 回程时丢失,我方代码拿到的永远是 false / 输入 Rect。
    //
    // 因此本 helper 只用 GUI.Box / GUI.Label(纯绘制,无返回),交互全部走
    // Event.current.MouseDown + Rect.Contains 自己判定。
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
        // 自绘 button:GUI.Box 当背景,Event.current.MouseDown + Contains 判定点击。
        // 不用 GUI.Button 因其 bool 返回在本 IL2CPP 绑定下永远 false。
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

        // 自绘 toggle:左侧小方块显示 X / 空,右侧 label。MouseDown 翻转。
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
