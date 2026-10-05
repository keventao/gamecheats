using UnityEngine;

namespace ZedZoneHpLock;

// v0.5.0: infinite reserve ammo. Every frame, top up Ammo/Arrow stacks in the
// whole player inventory (incl. backpack) to their stackNumber, so Reload
// always succeeds. Magazines/weapons untouched (their counts carry state).
// Combined with CanFire=true + malfunction suppression, the gun never runs dry.
public class AmmoLock : MonoBehaviour
{
    public AmmoLock(System.IntPtr ptr) : base(ptr) { }

    private void Update()
    {
        if (!Toggles.InfiniteAmmo) return;
        try {
            var gc = GameController.instance;
            if (gc == null || gc.gameData == null || gc.gameData.playerData == null) return;
            var inv = gc.gameData.playerData.inventoryData;
            if (inv == null) return;
            var all = inv.GetAllItemsIncludeBackpack();
            if (all == null) return;
            foreach (var it in all) {
                if (it == null) continue;
                ItemAttr attr = null;
                try { attr = ItemManager.instance.GetItemAttrById(it.itemId); } catch { continue; }
                if (attr == null) continue;
                int t = (int)attr.itemType;
                if (t != (int)ItemType.Ammo && t != (int)ItemType.Arrow) continue;
                if (attr.stackNumber > 0 && it.itemNumberFloat < attr.stackNumber)
                    it.itemNumberFloat = attr.stackNumber;
            }
        } catch { }
    }
}
