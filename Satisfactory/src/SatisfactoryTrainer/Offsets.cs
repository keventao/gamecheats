namespace SatisfactoryTrainer
{
    /// <summary>
    /// All build-specific addresses and struct offsets for Satisfactory.
    /// Every value here is extracted from the shipping PDBs — see
    /// <c>refs/RE-notes.md</c>. RVAs change every game patch; struct offsets
    /// change only if Coffee Stain edits the class. Re-verify per build.
    /// </summary>
    public static class Offsets
    {
        /// <summary>BuildId these offsets were extracted against.</summary>
        public const string BuildId = "493833";

        // ---- Module file names (as loaded in the game process) ----
        public const string ModuleCoreUObject = "FactoryGameSteam-CoreUObject-Win64-Shipping.dll";
        public const string ModuleCore = "FactoryGameSteam-Core-Win64-Shipping.dll";
        public const string ModuleFactoryGame = "FactoryGameSteam-FactoryGame-Win64-Shipping.dll";

        // ---- Engine global RVAs (relative to owning module base) ----
        /// <summary>FUObjectArray, in CoreUObject DLL.</summary>
        public const long RvaGUObjectArray = 0x5A3620;

        /// <summary>uint8** to the FName pool Blocks[] base, in Core DLL.</summary>
        public const long RvaGNameBlocksDebug = 0x7B8500;

        // ---- FUObjectArray layout ----
        public const int FUObjectArray_ObjObjects = 16; // FChunkedFixedUObjectArray

        // ---- FChunkedFixedUObjectArray layout (at GUObjectArray+16) ----
        public const int Chunked_Objects = 0;      // FUObjectItem** (chunk table)
        public const int Chunked_MaxElements = 16;
        public const int Chunked_NumElements = 20;
        public const int Chunked_MaxChunks = 24;
        public const int Chunked_NumChunks = 28;

        // ---- FUObjectItem ----
        public const int FUObjectItem_Stride = 24;
        public const int FUObjectItem_Object = 0; // UObject*

        // ---- UObjectBase layout ----
        public const int UObject_ObjectFlags = 8;
        public const int UObject_InternalIndex = 12;
        public const int UObject_ClassPrivate = 16;  // UClass*
        public const int UObject_NamePrivate = 24;   // FName (ComparisonIndex @ +0)
        public const int UObject_OuterPrivate = 32;

        // ---- Cheat 1: achievement enable ----
        // UOnlineIntegrationBackend
        public const string ClassOnlineIntegrationBackend = "OnlineIntegrationBackend";
        public const int OnlineBackend_bSuppressAchievements = 320; // bool

        // ---- Cheat 2: instant manual craft ----
        // UFGWorkBench (component on the manual-craft station)
        public const string ClassWorkBench = "FGWorkBench";
        public const int WorkBench_mCurrentRecipe = 600;                 // ptr (null when idle)
        public const int WorkBench_mCurrentManufacturingProgress = 608;  // float 0..1
        public const int WorkBench_mManufacturingSpeed = 612;            // float
        public const int WorkBench_mIsProducing = 628;                   // bool
        public const int WorkBench_mRecipeDuration = 724;                // float
    }
}
