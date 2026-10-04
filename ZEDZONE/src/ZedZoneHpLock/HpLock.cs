using UnityEngine;

namespace ZedZoneHpLock;

// v0.1.0: lock CURRENT hp only. maxhp is never written, so level-ups and
// attribute points that raise the cap keep working; hp just follows the cap.
// Chain (all real names from BepInEx interop dump):
//   GameController.instance -> gameData -> playerData -> characterStatusData -> hp/maxhp
public class HpLock : MonoBehaviour
{
    public HpLock(System.IntPtr ptr) : base(ptr) { }

    private void Update()
    {
        if (!Toggles.Hp) return;
        var gc = GameController.instance;
        if (gc == null) return;
        var gd = gc.gameData;
        if (gd == null) return;
        var pd = gd.playerData;
        if (pd == null) return;
        var st = pd.characterStatusData;
        if (st == null) return;
        if (st.hp < st.maxhp) st.hp = st.maxhp;
    }
}
