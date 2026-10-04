using System;
using UnityEngine;

namespace ZedZoneHpLock;

// v0.4.0: thin controller. IMGUI is out (FlexibleSpace/ScrollView stripped,
// font setter stripped). ItemPanel builds a runtime uGUI window Jim97-style:
// Canvas + dark panel + title/close + toggles + search + categories + real
// ScrollRect list + add buttons, using the game's own SourceHanSans font.
public class Panel : MonoBehaviour
{
    public Panel(IntPtr ptr) : base(ptr) { }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) ItemPanel.Toggle();
    }
}
