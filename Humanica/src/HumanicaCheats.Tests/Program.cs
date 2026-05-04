using System;
using HumanicaCheats.Modules;

namespace HumanicaCheats.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            ShouldKeepOriginalSpeedWhenToggleDisabled();
            ShouldKeepOriginalSpeedForWildPlants();
            ShouldKeepOriginalSpeedForNonCropResources();
            ShouldMultiplySelfPlantedCropSpeedWhenEnabled();
            ShouldMultiplyOwnVillagerMoveSpeedByTwo();
            ShouldMultiplyOwnVillagerMoveSpeedByFive();
            ShouldKeepOriginalMoveSpeedForNonOwnVillagers();
            ShouldAutoApplyWarehouseExpansionWhenMultiplierSelected();
            ShouldNotAutoApplyWarehouseExpansionForX1();
            ShouldNotRepeatWarehouseAutoExpansionForSameLoadedWorld();
            ShouldNotAutoApplyWarehouseExpansionWhenSelectionChangesAfterWorldLoad();
            ShouldTrustCurrentPackCountWhenSavedBaselineDoesNotMatchLoadedWarehouse();
            ShouldKeepSavedBaselineWhenCurrentPackCountMatchesPreviousExpansion();
            ShouldRestoreWarehouseSnapshotWhenMultiplierEnabled();
            ShouldNotRestoreWarehouseSnapshotTwice();
            ShouldCalculateMissingWarehouseSnapshotAmount();
            ShouldKeepHigherWarehouseSnapshotAmount();
            ShouldRaiseWarehouseSnapshotAmountWhenCurrentIsHigher();
            ShouldRoundTripWarehouseSnapshot();
            return 0;
        }

        private static void ShouldKeepOriginalSpeedWhenToggleDisabled()
        {
            var result = CropGrowthPolicy.ApplyMultiplier(2.5f, enabled: false, hasPlantGrowingTrigger: true, isPlayerPlanted: true);
            AssertEqual(2.5f, result, "toggle disabled keeps original crop speed");
        }

        private static void ShouldKeepOriginalSpeedForWildPlants()
        {
            var result = CropGrowthPolicy.ApplyMultiplier(2.5f, enabled: true, hasPlantGrowingTrigger: true, isPlayerPlanted: false);
            AssertEqual(2.5f, result, "wild plants are not accelerated");
        }

        private static void ShouldKeepOriginalSpeedForNonCropResources()
        {
            var result = CropGrowthPolicy.ApplyMultiplier(2.5f, enabled: true, hasPlantGrowingTrigger: false, isPlayerPlanted: true);
            AssertEqual(2.5f, result, "non-crop resources are not accelerated");
        }

        private static void ShouldMultiplySelfPlantedCropSpeedWhenEnabled()
        {
            var result = CropGrowthPolicy.ApplyMultiplier(2.5f, enabled: true, hasPlantGrowingTrigger: true, isPlayerPlanted: true);
            AssertEqual(25f, result, "self-planted crop speed is multiplied by 10");
        }

        private static void ShouldMultiplyOwnVillagerMoveSpeedByTwo()
        {
            var result = VillagerMoveSpeedPolicy.ApplyMultiplier(1.5f, 2, isPlayerVillager: true);
            AssertEqual(3f, result, "own villager move speed is multiplied by 2");
        }

        private static void ShouldMultiplyOwnVillagerMoveSpeedByFive()
        {
            var result = VillagerMoveSpeedPolicy.ApplyMultiplier(1.5f, 5, isPlayerVillager: true);
            AssertEqual(7.5f, result, "own villager move speed is multiplied by 5");
        }

        private static void ShouldKeepOriginalMoveSpeedForNonOwnVillagers()
        {
            var result = VillagerMoveSpeedPolicy.ApplyMultiplier(1.5f, 5, isPlayerVillager: false);
            AssertEqual(1.5f, result, "non-own villager move speed is not multiplied");
        }

        private static void ShouldAutoApplyWarehouseExpansionWhenMultiplierSelected()
        {
            if (!WarehouseAutoExpansionPolicy.ShouldApply(5, loadedWorldMultiplier: 5, alreadyAppliedForLoadedWorld: false))
            {
                throw new InvalidOperationException("warehouse expansion should auto apply when x5 is selected");
            }
        }

        private static void ShouldNotAutoApplyWarehouseExpansionForX1()
        {
            if (WarehouseAutoExpansionPolicy.ShouldApply(1, loadedWorldMultiplier: 1, alreadyAppliedForLoadedWorld: false))
            {
                throw new InvalidOperationException("warehouse expansion should not auto apply for x1");
            }
        }

        private static void ShouldNotRepeatWarehouseAutoExpansionForSameLoadedWorld()
        {
            if (WarehouseAutoExpansionPolicy.ShouldApply(5, loadedWorldMultiplier: 5, alreadyAppliedForLoadedWorld: true))
            {
                throw new InvalidOperationException("warehouse expansion should not repeat for the same loaded world");
            }
        }

        private static void ShouldNotAutoApplyWarehouseExpansionWhenSelectionChangesAfterWorldLoad()
        {
            if (WarehouseAutoExpansionPolicy.ShouldApply(5, loadedWorldMultiplier: 1, alreadyAppliedForLoadedWorld: false))
            {
                throw new InvalidOperationException("warehouse expansion should not auto apply when x5 is selected after world load");
            }
        }

        private static void ShouldTrustCurrentPackCountWhenSavedBaselineDoesNotMatchLoadedWarehouse()
        {
            AssertEqual(16, WarehouseCapacityBaselinePolicy.InferBaseline(12, currentPacks: 16, previousMultiplier: 10), "mismatched saved baseline must not shrink a 16 pack warehouse to 60");
        }

        private static void ShouldKeepSavedBaselineWhenCurrentPackCountMatchesPreviousExpansion()
        {
            AssertEqual(16, WarehouseCapacityBaselinePolicy.InferBaseline(16, currentPacks: 160, previousMultiplier: 10), "matching saved baseline should describe an already expanded warehouse");
        }

        private static void ShouldRestoreWarehouseSnapshotWhenMultiplierEnabled()
        {
            if (!WarehouseResourceSnapshotPolicy.ShouldRestore(5, alreadyAttempted: false))
            {
                throw new InvalidOperationException("warehouse snapshot should restore when warehouse multiplier is enabled");
            }
        }

        private static void ShouldNotRestoreWarehouseSnapshotTwice()
        {
            if (WarehouseResourceSnapshotPolicy.ShouldRestore(5, alreadyAttempted: true))
            {
                throw new InvalidOperationException("warehouse snapshot should not restore twice for one loaded world");
            }
        }

        private static void ShouldCalculateMissingWarehouseSnapshotAmount()
        {
            AssertEqual(30, WarehouseResourceSnapshotPolicy.MissingAmount(80, 50), "missing warehouse snapshot amount");
            AssertEqual(0, WarehouseResourceSnapshotPolicy.MissingAmount(50, 80), "no missing amount when current is higher");
        }

        private static void ShouldKeepHigherWarehouseSnapshotAmount()
        {
            var saved = new System.Collections.Generic.Dictionary<int, int> { [32] = 100 };
            var current = new System.Collections.Generic.Dictionary<int, int> { [32] = 60 };
            var merged = WarehouseResourceSnapshotPolicy.MergeHighWater(saved, current);
            AssertEqual(100, merged[32], "snapshot high-water should not decrease");
        }

        private static void ShouldRaiseWarehouseSnapshotAmountWhenCurrentIsHigher()
        {
            var saved = new System.Collections.Generic.Dictionary<int, int> { [32] = 100 };
            var current = new System.Collections.Generic.Dictionary<int, int> { [32] = 120 };
            var merged = WarehouseResourceSnapshotPolicy.MergeHighWater(saved, current);
            AssertEqual(120, merged[32], "snapshot high-water should increase");
        }

        private static void ShouldRoundTripWarehouseSnapshot()
        {
            var values = new System.Collections.Generic.Dictionary<int, int>
            {
                [32] = 120,
                [1] = 50
            };
            var parsed = WarehouseResourceSnapshotPolicy.Parse(WarehouseResourceSnapshotPolicy.Format(values));
            AssertEqual(50, parsed[1], "snapshot round-trip sticks");
            AssertEqual(120, parsed[32], "snapshot round-trip bread");
        }

        private static void AssertEqual(float expected, float actual, string message)
        {
            if (Math.Abs(expected - actual) > 0.0001f)
            {
                throw new InvalidOperationException(message + ": expected " + expected + ", got " + actual);
            }
        }

        private static void AssertEqual(int expected, int actual, string message)
        {
            if (expected != actual)
            {
                throw new InvalidOperationException(message + ": expected " + expected + ", got " + actual);
            }
        }
    }
}
