namespace LordsAndVilleinsCheats.Core
{
    /// <summary>
    /// Static cache of game-side singleton references.
    /// Populated by Harmony patches during game bootstrap; reset on returning to main menu.
    /// </summary>
    public static class GameRefs
    {
        public static bool IsReady;
        public static object? Settlement;

        public static void Reset()
        {
            IsReady = false;
            Settlement = null;
        }
    }
}
