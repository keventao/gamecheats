using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace ZedZoneHpLock;

// v0.4.0: runtime uGUI panel, Jim97-style. IMGUI is out (stripped calls +
// stripped font setter). All visuals are engine-native uGUI so the game's
// own SourceHanSans font assigns directly with no stripping involved.
public class ItemPanel : MonoBehaviour
{
    public ItemPanel(IntPtr ptr) : base(ptr) { }

    public static ItemPanel Instance;
    public static void Toggle()
    {
        if (Instance == null) {
            var go = NewGO("ZedZoneItemPanel");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<ItemPanel>();
        }
        Instance.ToggleShow();
    }

    private GameObject root;
    private bool built;
    private bool visible;
    private Font cjk;

    private Toggle hpToggle;
    private Toggle stToggle;
    private Toggle ammoToggle;
    private InputField searchInput;
    private string lastSearch = "@@init@@";
    private int catIndex;
    private Transform content;
    private Text statusText;
    private RectTransform rootRT;
    private bool dragging;
    private Vector3 dragPrev;

    private List<ItemAttr> cache;

    private static readonly string[] CatName = {
        "全部", "材料", "食物", "面罩", "近战", "远程", "服装", "弹药",
        "背包", "投掷", "弹匣", "箭矢", "配件", "部署物", "容器", "液体", "斗篷",
    };

    private void ToggleShow()
    {
        if (!built) Build();
        visible = !visible;
        if (root != null) root.SetActive(visible);
    }

    private static GameObject NewGO(string name)
    {
        return new GameObject(name, Il2CppType.Of<RectTransform>());
    }

    private static RectTransform RT(GameObject go, Transform parent, float w, float h)
    {
        var rt = go.GetComponent(Il2CppType.Of<RectTransform>()).TryCast<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    private Text MkText(Transform parent, string str, int size, float w)
    {
        var go = NewGO("T");
        var t = go.AddComponent<Text>();
        t.text = str;
        t.fontSize = size;
        t.color = Color.white;
        try { if (cjk != null) t.font = cjk; } catch { }
        var rt = RT(go, parent, w, size + 8);
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
        return t;
    }

    private Button MkButton(Transform parent, string str, float w, float h, Action onClick)
    {
        var go = NewGO("B");
        var img = go.AddComponent<Image>();
        img.color = new Color(0.20f, 0.23f, 0.29f, 1f);
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.highlightedColor = new Color(0.29f, 0.33f, 0.42f, 1f);
        cb.pressedColor = new Color(0.16f, 0.18f, 0.23f, 1f);
        btn.colors = cb;
        var txt = NewGO("T").AddComponent<Text>();
        txt.text = str;
        txt.fontSize = 14;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        try { if (cjk != null) txt.font = cjk; } catch { }
        var trt = txt.gameObject.GetComponent(Il2CppType.Of<RectTransform>()).TryCast<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var rt = RT(go, parent, w, h);
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
        btn.onClick.AddListener(new Action(onClick));
        return btn;
    }

    private Toggle MkToggle(Transform parent, string str, bool init, Action<bool> onChange)
    {
        var go = NewGO("TG");
        var tg = go.AddComponent<Toggle>();
        var bg = NewGO("BG").AddComponent<Image>();
        bg.color = new Color(0.20f, 0.23f, 0.29f, 1f);
        var brt = bg.gameObject.GetComponent(Il2CppType.Of<RectTransform>()).TryCast<RectTransform>();
        brt.SetParent(go.transform, false);
        brt.sizeDelta = new Vector2(20, 20);
        var ck = NewGO("CK").AddComponent<Image>();
        ck.color = new Color(0.3f, 0.85f, 0.4f, 1f);
        var crt = ck.gameObject.GetComponent(Il2CppType.Of<RectTransform>()).TryCast<RectTransform>();
        crt.SetParent(bg.transform, false);
        crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
        crt.offsetMin = new Vector2(4, 4); crt.offsetMax = new Vector2(-4, -4);
        tg.targetGraphic = bg;
        tg.graphic = ck;
        tg.isOn = init;
        var rt = RT(go, parent, 300, 26);
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
        MkText(go.transform, str, 15, 260);
        tg.onValueChanged.AddListener(new Action<bool>(onChange));
        return tg;
    }

    private void PickupFont()
    {
        if (cjk != null) return;
        try {
            var all = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Font>());
            foreach (var o in all) {
                var f = o.TryCast<Font>();
                if (f == null || f.name == null) continue;
                if (f.name.Contains("SourceHanSans") || f.name.Contains("Chinese")) { cjk = f; break; }
            }
        } catch { }
        try {
            if (cjk != null)
                cjk.RequestCharactersInTexture("状态锁定血量耐力疲劳添加物品搜索全部材料食物面罩近战远程服装弹药背包投掷弹匣箭矢配件部署物容器液体斗篷关闭0123456789x[]<>+-./:×", 14);
        } catch { }
    }

    private static void Log(string s)
    {
        try { BepInEx.Logging.Logger.CreateLogSource("ZedZoneHpLock").LogInfo(s); } catch { }
    }

    private void Build()
    {
        try { BuildInner(); } catch (Exception e) { Log("BUILD FAIL: " + e.ToString()); }
    }

    private void BuildInner()
    {
        PickupFont();
        var cgo = NewGO("ZedCanvas");
        UnityEngine.Object.DontDestroyOnLoad(cgo);
        var cv = cgo.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 1000;
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        cgo.AddComponent<GraphicRaycaster>();

        root = NewGO("Root");
        var rrt = RT(root, cgo.transform, 640, 700);
        rootRT = rrt;
        rrt.anchorMin = new Vector2(0.5f, 0.5f); rrt.anchorMax = new Vector2(0.5f, 0.5f); rrt.pivot = new Vector2(0.5f, 0.5f);
        rrt.anchoredPosition = Vector2.zero;
        var rimg = root.AddComponent<Image>();
        rimg.color = new Color(0.09f, 0.10f, 0.13f, 0.95f);

        float y = -10;
        var title = MkText(root.transform, "ZedZone Mod v0.5.0", 17, 300);
        title.rectTransform.anchoredPosition = new Vector2(10, y);
        var census = MkButton(root.transform, "普查武器", 80, 30, () => CensusWeapons());
        census.GetComponent<RectTransform>().anchoredPosition = new Vector2(330, y);
        var clear = MkButton(root.transform, "清除卡壳", 80, 30, () => ClearMalfunction());
        clear.GetComponent<RectTransform>().anchoredPosition = new Vector2(420, y);
        var close = MkButton(root.transform, "X", 50, 30, () => ToggleShow());
        close.GetComponent<RectTransform>().anchoredPosition = new Vector2(580, y);
        y -= 36;
        hpToggle = MkToggle(root.transform, "血量锁定", Toggles.Hp, v => Toggles.Hp = v);
        hpToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, y);
        y -= 30;
        stToggle = MkToggle(root.transform, "耐力/疲劳锁定", Toggles.Stamina, v => Toggles.Stamina = v);
        stToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, y);
        y -= 30;
        ammoToggle = MkToggle(root.transform, "无限弹药", Toggles.InfiniteAmmo, v => Toggles.InfiniteAmmo = v);
        ammoToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, y);
        y -= 36;
        var sl = MkText(root.transform, "搜索:", 14, 60);
        sl.rectTransform.anchoredPosition = new Vector2(10, y);
        var inp = NewGO("Search");
        searchInput = inp.AddComponent<InputField>();
        var irt = RT(inp, root.transform, 540, 30);
        irt.anchorMin = new Vector2(0, 1); irt.anchorMax = new Vector2(0, 1); irt.pivot = new Vector2(0, 1);
        irt.anchoredPosition = new Vector2(80, y);
        var ibg = inp.AddComponent<Image>();
        ibg.color = new Color(0.13f, 0.15f, 0.2f, 1f);
        var itxt = MkText(inp.transform, string.Empty, 14, 530);
        searchInput.textComponent = itxt;
        y -= 36;

        for (int r = 0; r < 3; r++) {
            for (int c = 0; c < 6; c++) {
                int i = r * 6 + c;
                if (i >= CatName.Length) break;
                int idx = i;
                string cap = CatName[i];
                var b = MkButton(root.transform, cap, 98, 28, () => { catIndex = idx; RebuildList(); });
                b.GetComponent<RectTransform>().anchoredPosition = new Vector2(10 + c * 104, y);
            }
            y -= 32;
        }

        var scrollGO = NewGO("Scroll");
        var srt = RT(scrollGO, root.transform, 620, 350);
        srt.anchorMin = new Vector2(0, 1); srt.anchorMax = new Vector2(0, 1); srt.pivot = new Vector2(0, 1);
        srt.anchoredPosition = new Vector2(10, y - 8);
        var simg = scrollGO.AddComponent<Image>();
        simg.color = new Color(0.07f, 0.08f, 0.10f, 1f);
        var sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = false;
        var vp = NewGO("Viewport");
        var vrt = RT(vp, scrollGO.transform, 600, 380);
        vrt.anchorMin = new Vector2(0, 0); vrt.anchorMax = new Vector2(1, 1); vrt.pivot = new Vector2(0.5f, 0.5f);
        vrt.offsetMin = Vector2.zero; vrt.offsetMax = new Vector2(-20, 0);
        vp.AddComponent<RectMask2D>();
        var cgo2 = NewGO("Content");
        var crt = RT(cgo2, vp.transform, 600, 10);
        crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1); crt.pivot = new Vector2(0, 1);
        crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
        var vlg = cgo2.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        var fit = cgo2.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        content = cgo2.transform;
        var sbGO = NewGO("Scrollbar");
        var sbrt = RT(sbGO, scrollGO.transform, 18, 380);
        sbrt.anchorMin = new Vector2(1, 0); sbrt.anchorMax = new Vector2(1, 1); sbrt.pivot = new Vector2(0.5f, 0.5f);
        sbrt.offsetMin = new Vector2(-18, 0); sbrt.offsetMax = Vector2.zero;
        var sbar = sbGO.AddComponent<Scrollbar>();
        sbar.direction = Scrollbar.Direction.BottomToTop;
        var handle = NewGO("Handle");
        var himg = handle.AddComponent<Image>();
        himg.color = new Color(0.35f, 0.4f, 0.5f, 1f);
        var hrt = handle.GetComponent(Il2CppType.Of<RectTransform>()).TryCast<RectTransform>();
        hrt.SetParent(sbGO.transform, false);
        hrt.anchorMin = Vector2.zero; hrt.anchorMax = Vector2.one;
        hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;
        sbar.handleRect = hrt;
        sr.content = crt;
        sr.viewport = vrt;
        sr.verticalScrollbar = sbar;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        try { Log("scroll top=" + srt.anchoredPosition + " view=" + vrt.rect.size + " contentKids=" + content.childCount + " handle=" + hrt.rect.size); }
        catch (Exception e) { Log("scroll diag FAIL " + e.GetType().Name); }

        statusText = MkText(root.transform, string.Empty, 13, 620);
        statusText.rectTransform.anchoredPosition = new Vector2(10, -656);

        root.SetActive(false);
        built = true;
        EnsureCache();
        RebuildList();
    }

    private void Update()
    {
        if (!visible || !built) return;
        if (searchInput != null && searchInput.text != lastSearch) {
            lastSearch = searchInput.text;
            RebuildList();
        }
        if (hpToggle != null && hpToggle.isOn != Toggles.Hp) hpToggle.isOn = Toggles.Hp;
        if (stToggle != null && stToggle.isOn != Toggles.Stamina) stToggle.isOn = Toggles.Stamina;
        if (ammoToggle != null && ammoToggle.isOn != Toggles.InfiniteAmmo) ammoToggle.isOn = Toggles.InfiniteAmmo;
        // Title-bar drag (top 40px of panel). A press without movement still
        // reaches the buttons normally; only a real move drags the window.
        try {
            if (rootRT == null) return;
            Vector3 mp = Input.mousePosition;
            float cx = Screen.width * 0.5f + rootRT.anchoredPosition.x;
            float cy = Screen.height * 0.5f + rootRT.anchoredPosition.y;
            bool inTitle = mp.x >= cx - 320 && mp.x <= cx + 320 && mp.y >= cy + 310 && mp.y <= cy + 350;
            if (Input.GetMouseButtonDown(0) && inTitle) { dragging = false; dragPrev = mp; }
            if (Input.GetMouseButton(0) && dragPrev != Vector3.zero) {
                if (!dragging && (mp - dragPrev).magnitude > 4f && inTitle) dragging = true;
                if (dragging) {
                    rootRT.anchoredPosition += new Vector2(mp.x - dragPrev.x, mp.y - dragPrev.y);
                    dragPrev = mp;
                }
            }
            if (Input.GetMouseButtonUp(0)) { dragging = false; dragPrev = Vector3.zero; }
        } catch { }
    }

    private void EnsureCache()
    {
        if (cache != null) return;
        cache = new List<ItemAttr>();
        try {
            var dic = ItemManager.instance.itemAttrDic;
            foreach (var kv in dic) {
                var a = kv.Value;
                if (a == null || a.hiddenItem) continue;
                cache.Add(a);
            }
            cache.Sort((x, y) => x.itemId.CompareTo(y.itemId));
        } catch { }
    }

    private static string DispName(ItemAttr a)
    {
        try { var s = a.itemName_Runtime; if (!string.IsNullOrEmpty(s)) return s; } catch { }
        try { var s = a.itemName; if (!string.IsNullOrEmpty(s)) return s; } catch { }
        return "item_" + a.itemId;
    }

    private void RebuildList()
    {
        if (content == null) return;
        for (int i = content.childCount - 1; i >= 0; i--) {
            var ch = content.GetChild(i);
            if (ch != null) UnityEngine.Object.Destroy(ch.gameObject);
        }
        if (cache == null) EnsureCache();
        if (cache == null) return;
        string q = lastSearch == "@@init@@" ? string.Empty : (lastSearch ?? string.Empty).Trim().ToLower();
        int shown = 0;
        foreach (var a in cache) {
            if (shown >= 150) break;
            int t = (int)a.itemType;
            if (catIndex != 0 && t != catIndex - 1) continue;
            string name = DispName(a);
            string hay = (name + " " + a.itemId).ToLower();
            if (q.Length > 0 && hay.Contains(q) == false) continue;
            shown++;
            int id = a.itemId;
            string label = name + " x" + (a.stackNumber > 0 ? a.stackNumber : 1);
            var row = NewGO("Row");
            var rrt = RT(row, content, 590, 26);
            var rle = row.AddComponent<LayoutElement>();
            rle.preferredHeight = 26;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            var txt = MkText(row.transform, label, 14, 500);
            var le = txt.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            var add = MkButton(row.transform, "[+]", 64, 26, () => AddItem(id));
            var ale = add.gameObject.AddComponent<LayoutElement>();
            ale.preferredWidth = 64;
            ale.minWidth = 64;
            ale.preferredHeight = 26;
            ale.minHeight = 26;
        }
        try {
            if (content.childCount > 0) {
                var r0 = content.GetChild(0);
                string info = "rows=" + content.childCount + " r0kids=" + r0.childCount;
                for (int k = 0; k < r0.childCount; k++) {
                    var c = r0.GetChild(k);
                    var rr = c.GetComponent(Il2CppType.Of<RectTransform>()).TryCast<RectTransform>();
                    info += " [" + c.name + "@" + rr.anchoredPosition + " " + rr.sizeDelta + (c.gameObject.activeSelf ? "" : " OFF") + "]";
                }
                Log("listdiag " + info);
            } else Log("listdiag EMPTY");
        } catch (Exception e) { Log("listdiag FAIL " + e.GetType().Name); }
    }

    private void CensusWeapons()
    {
        try {
            var gc = GameController.instance;
            if (gc == null || gc.gameData == null || gc.gameData.playerData == null) return;
            var inv = gc.gameData.playerData.inventoryData;
            if (inv == null) return;
            foreach (var wt in new ItemType[] { ItemType.RangedWeapon, ItemType.MeleeWeapon }) {
                var list = inv.GetItemListByType(wt);
                if (list == null) continue;
                foreach (var it in list) {
                    string nm = "?";
                    ItemAttr aa = null;
                    try { aa = ItemManager.instance.GetItemAttrById(it.itemId); if (aa != null) nm = aa.ItemName_EN; } catch { }
                    string extra2 = string.Empty;
                    try {
                        var rwx = aa.TryCast<ItemAttr_RangedWeapon>();
                        if (rwx != null) {
                            float price = -1f;
                            try { price = it.GetInstalledGunPartsPrice(); } catch { }
                            extra2 = " attrhp=" + rwx.hp + " partdur=" + ItemAttr_RangedWeapon.GetGunPartDurability(it) + " partsprice=" + price;
                        }
                    } catch (Exception e2) { extra2 = " partdur FAIL " + e2.GetType().Name; }
                    Log("invdump " + wt + " id=" + it.itemId + " dur=" + it.durability + " num=" + it.itemNumberFloat + " " + nm + extra2);
                }
            }
        } catch (Exception e) { Log("invdump FAIL " + e.GetType().Name); }
    }

    private void ClearMalfunction()
    {
        try {
            var gc = GameController.instance;
            if (gc == null) { if (statusText != null) statusText.text = "先读档"; return; }
            var pc = gc.playerCharacter;
            if (pc == null) { if (statusText != null) statusText.text = "没找到玩家角色"; return; }
            var gun = pc.rangedWeapon;
            Log("clearbtn gun=" + (gun == null ? "null" : ("malf=" + gun.isMalfunction)));
            if (gun == null) { if (statusText != null) statusText.text = "手上没拿枪（先装备）"; return; }
            if (!gun.isMalfunction) { if (statusText != null) statusText.text = "枪没卡壳"; return; }
            pc.ClearWeaponMalfunction(gun);
            if (statusText != null) statusText.text = "已发送清除卡壳";
            Log("clearbtn called");
        } catch (Exception e) {
            if (statusText != null) statusText.text = "Error: " + e.GetType().Name;
            Log("clearbtn FAIL " + e);
        }
    }

    private void AddItem(int itemId)
    {
        Log("AddItem clicked id=" + itemId);
        CharacterStatusData st = null;
        InventoryData inv = null;
        GameController gc = null;
        try {
            gc = GameController.instance;
            if (gc == null || gc.gameData == null || gc.gameData.playerData == null) {
                if (statusText != null) statusText.text = "先读档再添加";
                return;
            }
            st = gc.gameData.playerData.characterStatusData;
            inv = gc.gameData.playerData.inventoryData;
        } catch { }
        if (st == null || inv == null || gc == null) {
            if (statusText != null) statusText.text = "先读档再添加";
            return;
        }
        try {
            var data = new ItemData();
            data.itemId = itemId;
            ItemAttr attr0 = null;
            try {
                attr0 = ItemManager.instance.GetItemAttrById(itemId);
                data.itemNumberFloat = (attr0 != null && attr0.stackNumber > 0) ? attr0.stackNumber : 1;
            } catch { data.itemNumberFloat = 1; }
            string extra = string.Empty;
            Log("wdiag attr=" + (attr0 == null ? "null" : attr0.GetType().FullName));
            if (attr0 != null) {
                try {
                    var rw = attr0.TryCast<ItemAttr_RangedWeapon>();
                    Log("wdiag TryCast Ranged=" + (rw == null ? "null" : "ok"));
                    if (rw != null) {
                        // durability is PERCENT 0-100 (all natural guns census at dur=100
                        // regardless of attrhp 150/350/360). Writing attr.hp (e.g. 120)
                        // overflows it -> displays 0 + malfunction.
                        Log("wdiag rw.hp=" + rw.hp + " durBefore=" + data.durability);
                        data.durability = 100;
                        ItemManager.EnsureRangedWeaponProperties(data);
                        extra = " dur=100";
                        try {
                            if (rw.defaultMagazineId > 0) {
                                var mag = new ItemData();
                                mag.itemId = rw.defaultMagazineId;
                                mag.itemNumberFloat = rw.magazineSize > 0 ? rw.magazineSize : 1;
                                var old = ItemAttr_RangedWeapon.SwapMagazineIntoWeapon(data, mag);
                                Log("wdiag magswap magId=" + rw.defaultMagazineId + " old=" + (old == null ? "null" : ("id" + old.itemId)));
                                extra += " +mag";
                            }
                        } catch (Exception e2) { Log("wdiag magswap FAIL " + e2.GetType().Name); }
                        if (rw.ammoId > 0 && rw.magazineSize > 0) {
                            try {
                                var ammoAttr = ItemManager.instance.GetItemAttrById(rw.ammoId);
                                var ammo = new ItemData();
                                ammo.itemId = rw.ammoId;
                                int give = rw.magazineSize;
                                if (ammoAttr != null && ammoAttr.stackNumber > 0 && ammoAttr.stackNumber < give)
                                    give = ammoAttr.stackNumber;
                                ammo.itemNumberFloat = give;
                                if (gc.AddItemToPlayer(ammo, true)) extra += " +ammo" + give;
                            } catch { }
                        }
                    } else {
                        var mw = attr0.TryCast<ItemAttr_MeleeWeapon>();
                        Log("wdiag TryCast Melee=" + (mw == null ? "null" : "ok"));
                        if (mw != null) { data.durability = 100; extra = " dur=100"; }
                    }
                } catch { }
            }
            string via = string.Empty;
            bool ok = false;
            try { ok = gc.AddItemToPlayer(data, true); via = "AddItemToPlayer"; } catch { }
            if (ok && (data.durability > 0)) {
                try {
                    var got = inv.GetItemListById(itemId);
                    ItemData last = null;
                    if (got != null) { foreach (var it in got) { last = it; } }
                    Log("wdiag storedCount=" + (got == null ? -1 : got.Count) + " storedDur=" + (last == null ? -1f : last.durability));
                    if (last != null && last.durability <= 0) {
                        last.durability = data.durability;
                        Log("wdiag forced storedDur=" + last.durability);
                        extra += " fixed";
                    }
                } catch (Exception e) { Log("wdiag stored FAIL " + e.GetType().Name); }
                try {
                    var invs = new Il2CppSystem.Collections.Generic.List<InventoryData>();
                    invs.Add(inv);
                    float restored = InventoryData.RestoreItemDurability(itemId, 99999f, invs);
                    Log("wdiag restorecall ret=" + restored);
                    extra += " rep" + restored;
                } catch (Exception e) { Log("wdiag restorecall FAIL " + e.GetType().Name); }
            }
            if (ok) { CensusWeapons(); }
            if (!ok && inv != null) {
                try {
                    var data2 = new ItemData();
                    data2.itemId = itemId;
                    var attr = ItemManager.instance.GetItemAttrById(itemId);
                    data2.itemNumberFloat = (attr != null && attr.stackNumber > 0) ? attr.stackNumber : 1;
                    var t = (attr != null) ? (int)attr.itemType : -1;
                    if (t == (int)ItemType.RangedWeapon || t == (int)ItemType.Magazine)
                        ItemManager.EnsureRangedWeaponProperties(data2);
                    ok = inv.TryAddItem(data2, true, true);
                    via = "TryAddItem(T,T)";
                    data.itemNumberFloat = data2.itemNumberFloat;
                } catch { }
            }
            if (statusText != null)
                statusText.text = ok ? ("已添加 x" + data.itemNumberFloat + extra + " [" + via + "]") : "背包满/添加失败";
            Log("AddItem id=" + itemId + " ok=" + ok + " via=" + via);
        } catch (Exception e) {
            if (statusText != null) statusText.text = "Error: " + e.GetType().Name;
            Log("AddItem FAIL " + e);
        }
    }
}
