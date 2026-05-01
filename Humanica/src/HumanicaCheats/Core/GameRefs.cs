namespace HumanicaCheats.Core
{
    // VillageData 是游戏世界根单例。非 null 表示游戏世界已加载。
    // 通过服务定位器 Il2Cpp.S 访问：Il2Cpp.S.VillageData（已通过反射确认）。
    // 原始假设 VillageData.VillageData 已废弃（VillageData 类本身无静态单例 getter）。
    public static class GameRefs
    {
        public static bool IsReady
        {
            get
            {
                try { return Il2Cpp.S.VillageData != null; }
                catch { return false; }
            }
        }
    }
}
