using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasySpawner.UI
{
    public static class Style
    {
        private static Font cjkFont;

        public static void ApplyAll(GameObject root, EasySpawnerMenu menu)
        {
            FindFonds();
            ApplyPanel(root.GetComponent<Image>());

            foreach (Text text in root.GetComponentsInChildren<Text>(true))
                ApplyText(text, new Color(219f / 255f, 219f / 255f, 219f / 255f));

            foreach (InputField inputField in root.GetComponentsInChildren<InputField>(true))
                ApplyInputField(inputField);

            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                ApplyButton(button);

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>(true))
                ApplyToogle(toggle);

            foreach (ScrollRect scrollRect in root.GetComponentsInChildren<ScrollRect>(true))
                ApplyScrollRect(scrollRect);

            foreach (Dropdown dropdown in root.GetComponentsInChildren<Dropdown>(true))
                ApplyDropdown(dropdown);

            Localize(root);
        }

        private static readonly Dictionary<string, string> TextTranslations = new Dictionary<string, string>
        {
            { "Easy Spawner", "简易生成器" },
            { "Hotkeys:", "快捷键:" },
            { "Search...", "搜索..." },
            { "Amount...", "数量..." },
            { "Level...", "等级..." },
            { "Spawn", "生成" },
            { "Put in inventory", "放入背包" },
            { "Ignore max stack size", "忽略堆叠上限" },
            { "Show favourites only", "只看收藏" },
            { "Options:", "选项:" },
        };

        public static void Localize(GameObject root)
        {
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                string trimmed = text.text.Trim();
                if (TextTranslations.TryGetValue(trimmed, out string translated))
                    text.text = text.text.StartsWith(" ") ? " " + translated : translated;
            }
        }

        public static void ApplyPanel(Image image)
        {
            image.sprite = GetSprite("woodpanel_settings");
        }

        public static void ApplyText(Text text, Color color)
        {
            // 原版使用 Averia Serif Libre，不含中文字形，统一替换为系统中文字体（动态字体）。
            // 加粗样式保留，由该字体合成粗体显示。
            text.font = cjkFont;
            text.color = color;
        }

        public static void ApplyInputField(InputField inputField)
        {
            inputField.GetComponent<Image>().sprite = GetSprite("text_field");
        }

        public static void ApplyButton(Button button)
        {
            button.GetComponent<Image>().sprite = GetSprite("button");
        }

        public static void ApplyToogle(Toggle toggle)
        {
            switch (toggle.gameObject.name)
            {
                case "_Template":
                case "_Template(Clone)":
                    ((Image)toggle.targetGraphic).sprite = GetSprite("button");
                    ((Image)toggle.targetGraphic).type = Image.Type.Sliced;
                    ((Image)toggle.targetGraphic).pixelsPerUnitMultiplier = 2f;
                    break;
                case "Star":
                case "Star(Clone)":
                    break;
                default:
                    ((Image)toggle.targetGraphic).sprite = GetSprite("checkbox");
                    ((Image)toggle.targetGraphic).type = Image.Type.Sliced;
                    ((Image)toggle.targetGraphic).pixelsPerUnitMultiplier = 2f;
                    break;
            }

            if (toggle.graphic != null)
            {
                toggle.graphic.GetComponent<Image>().color = new Color(1f, 0.678f, 0.103f, 1f);
                toggle.graphic.GetComponent<Image>().sprite = GetSprite("checkbox_marker");
                toggle.graphic.GetComponent<Image>().maskable = true;
            }
        }

        public static void ApplyScrollRect(ScrollRect scrollRect)
        {
            scrollRect.GetComponent<Image>().sprite = GetSprite("item_background_sunken");

            if ((bool)scrollRect.horizontalScrollbar)
            {
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).sprite = GetSprite("text_field");
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).color = Color.grey;
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).type = Image.Type.Sliced;
                ((Image)scrollRect.horizontalScrollbar.targetGraphic).pixelsPerUnitMultiplier = 2f;
                scrollRect.horizontalScrollbar.GetComponent<Image>().sprite = GetSprite("text_field");
                scrollRect.horizontalScrollbar.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
            }

            if ((bool)scrollRect.verticalScrollbar)
            {
                ((Image)scrollRect.verticalScrollbar.targetGraphic).sprite = GetSprite("text_field");
                ((Image)scrollRect.verticalScrollbar.targetGraphic).color = Color.grey;
                ((Image)scrollRect.verticalScrollbar.targetGraphic).type = Image.Type.Sliced;
                ((Image)scrollRect.verticalScrollbar.targetGraphic).pixelsPerUnitMultiplier = 2f;
                scrollRect.verticalScrollbar.GetComponent<Image>().sprite = GetSprite("text_field");
                scrollRect.verticalScrollbar.GetComponent<Image>().pixelsPerUnitMultiplier = 3f;
            }
        }

        public static void ApplyDropdown(Dropdown dropdown)
        {
            ((Image)dropdown.targetGraphic).sprite = GetSprite("button");
            ApplyScrollRect(dropdown.template.GetComponent<ScrollRect>());
        }

        public static Sprite GetSprite(string name)
        {
            return Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name == name);
        }

        private static readonly string[] PreferredCjkFontNames =
        {
            "Microsoft YaHei",
            "微软雅黑",
            "SimHei",
            "黑体",
            "Microsoft JhengHei",
            "PingFang SC",
            "Noto Sans CJK SC",
            "Arial Unicode MS",
            "Arial"
        };

        private static void FindFonds()
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();

            foreach (string fontName in PreferredCjkFontNames)
            {
                Font font = fonts.FirstOrDefault(x => x.name == fontName);
                if (font != null && font.dynamic)
                {
                    cjkFont = font;
                    return;
                }
            }

            // 游戏内未加载时，直接创建操作系统动态字体（Windows 上必有微软雅黑）
            cjkFont = Font.CreateDynamicFontFromOSFont(PreferredCjkFontNames, 16);
        }
    }
}
