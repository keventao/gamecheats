namespace ClanfolkCheats.Core
{
    public static class GameRefs
    {
        public static bool IsReady
        {
            get
            {
                // TODO: replace with actual game world singleton check after decompile
                // e.g. try { return Il2Cpp.S.WorldManager != null; } catch { return false; }
                return true;
            }
        }
    }
}
