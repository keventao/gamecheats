namespace HumanicaCheats.Core
{
    // VillageData 是游戏世界根单例。非 null 表示游戏世界已加载。
    // 类名 VillageData 来自 Assembly-CSharp.dll 代理,静态属性 VillageData.VillageData 返回实例。
    public static class GameRefs
    {
        public static bool IsReady
        {
            get
            {
                try { return VillageData.VillageData != null; }
                catch { return false; }
            }
        }
    }
}
