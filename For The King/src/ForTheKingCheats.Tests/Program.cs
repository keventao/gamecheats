using System;
using ForTheKingCheats.Modules;

namespace ForTheKingCheats.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            ShouldKeepCurrentHealthWhenLockEnabledAndDamageWouldLowerHealth();
            ShouldAllowHealingWhenLockEnabled();
            ShouldAllowDamageWhenLockDisabled();
            return 0;
        }

        private static void ShouldKeepCurrentHealthWhenLockEnabledAndDamageWouldLowerHealth()
        {
            var result = PlayerHealthPolicy.GetProtectedHealth(true, 12, 7, 30);
            AssertEqual(12, result, "lock enabled keeps current health when incoming value is lower");
        }

        private static void ShouldAllowHealingWhenLockEnabled()
        {
            var result = PlayerHealthPolicy.GetProtectedHealth(true, 12, 18, 30);
            AssertEqual(18, result, "lock enabled allows healing");
        }

        private static void ShouldAllowDamageWhenLockDisabled()
        {
            var result = PlayerHealthPolicy.GetProtectedHealth(false, 12, 7, 30);
            AssertEqual(7, result, "lock disabled allows damage");
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
