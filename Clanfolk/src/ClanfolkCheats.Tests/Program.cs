using System;

namespace ClanfolkCheats.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            ShouldLogTestStartup();
            // Add feature-specific policy tests here as modules are implemented.
            return 0;
        }

        private static void ShouldLogTestStartup()
        {
            Console.WriteLine("ClanfolkCheats.Tests: running...");
        }
    }
}
