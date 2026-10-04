using UnityEngine;

namespace ZedZoneHpLock;

// v0.2.0: lock stamina. energy pinned to maxEnergy (same pattern as HpLock).
// fatigue pinned to 0 on the assumption lower = more rested (WeMod lists
// "Unlimited Energy" and "No Fatigue" as separate cheats); if sprinting still
// gets you tired in-game, flip the line to maxFatigue.
public class StaminaLock : MonoBehaviour
{
    public StaminaLock(System.IntPtr ptr) : base(ptr) { }

    private void Update()
    {
        if (!Toggles.Stamina) return;
        var gc = GameController.instance;
        if (gc == null) return;
        var gd = gc.gameData;
        if (gd == null) return;
        var pd = gd.playerData;
        if (pd == null) return;
        var st = pd.characterStatusData;
        if (st == null) return;
        if (st.energy < st.maxEnergy) st.energy = st.maxEnergy;
        if (st.fatigue != 0f) st.fatigue = 0f;
    }
}
