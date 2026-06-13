using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SatisfactoryTrainer
{
    internal static class Program
    {
        // Virtual-key codes
        private const int VK_F1 = 0x70;
        private const int VK_F2 = 0x71;
        private const int VK_F10 = 0x79;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private static int Main()
        {
            Console.Title = $"Satisfactory Trainer (build {Offsets.BuildId})";
            Banner();

            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("This trainer must run on Windows (it reads the game's memory).");
                return 2;
            }

            Process? game = FindGameProcess();
            if (game == null)
            {
                Console.WriteLine("Satisfactory is not running. Start the game, load a save, then relaunch the trainer.");
                return 1;
            }

            Console.WriteLine($"Attached to process {game.ProcessName} (PID {game.Id}).");

            using var mem = new ProcessMemory(game);

            long coreUObject = mem.GetModuleBase(Offsets.ModuleCoreUObject);
            long core = mem.GetModuleBase(Offsets.ModuleCore);
            if (coreUObject == 0 || core == 0)
            {
                Console.WriteLine("Could not locate engine modules. Is this really Satisfactory?");
                return 1;
            }

            var ue = new UnrealRuntime(mem, coreUObject, core);
            if (!ue.Initialize(out string initError))
            {
                Console.WriteLine($"Init failed: {initError}");
                Console.WriteLine($"These offsets target build {Offsets.BuildId}. If the game updated, re-extract (see refs/).");
                return 1;
            }

            Console.WriteLine($"UE runtime OK — {ue.ObjectCount:N0} objects.");

            var cheats = new CheatEngine(mem, ue);
            if (!cheats.ResolveClasses(out string classError))
            {
                Console.WriteLine($"Warning: {classError}");
            }

            PrintHelp();
            RunLoop(mem, cheats);
            return 0;
        }

        private static void RunLoop(ProcessMemory mem, CheatEngine cheats)
        {
            bool prevF1 = false, prevF2 = false, prevF10 = false;

            while (mem.IsAlive)
            {
                bool f1 = KeyDown(VK_F1);
                bool f2 = KeyDown(VK_F2);
                bool f10 = KeyDown(VK_F10);

                if (f1 && !prevF1)
                {
                    cheats.AchievementsEnabled = !cheats.AchievementsEnabled;
                    Status(cheats);
                }

                if (f2 && !prevF2)
                {
                    cheats.InstantCraftEnabled = !cheats.InstantCraftEnabled;
                    Status(cheats);
                }

                if (f10 && !prevF10)
                {
                    Console.WriteLine("Exiting (cheats left as-is in the running game).");
                    return;
                }

                prevF1 = f1;
                prevF2 = f2;
                prevF10 = f10;

                try
                {
                    cheats.Tick();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Tick error: {ex.Message}");
                }

                Thread.Sleep(50); // ~20 Hz
            }

            Console.WriteLine("Game process exited.");
        }

        private static bool KeyDown(int vKey) => (GetAsyncKeyState(vKey) & 0x8000) != 0;

        private static void Status(CheatEngine c)
        {
            Console.WriteLine(
                $"[Achievements: {OnOff(c.AchievementsEnabled)} ({c.BackendCount})]  " +
                $"[Instant Craft: {OnOff(c.InstantCraftEnabled)} ({c.WorkBenchCount})]");
        }

        private static string OnOff(bool b) => b ? "ON " : "off";

        private static Process? FindGameProcess()
        {
            // Try likely names first, then fall back to scanning every process for
            // the FactoryGame module (robust regardless of the exe name).
            string[] likely = { "FactoryGameSteam-Win64-Shipping", "FactoryGame", "FactoryGameSteam" };
            foreach (string name in likely)
            {
                foreach (Process p in Process.GetProcessesByName(name))
                {
                    if (HasFactoryModule(p))
                    {
                        return p;
                    }
                }
            }

            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (HasFactoryModule(p))
                    {
                        return p;
                    }
                }
                catch
                {
                    // access denied / exited — skip
                }
            }

            return null;
        }

        private static bool HasFactoryModule(Process p)
        {
            try
            {
                foreach (ProcessModule? m in p.Modules)
                {
                    if (m != null &&
                        string.Equals(m.ModuleName, Offsets.ModuleFactoryGame, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // 32/64-bit mismatch or access denied
            }

            return false;
        }

        private static void Banner()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("  Satisfactory Trainer  —  external, read/write");
            Console.WriteLine($"  Target build: {Offsets.BuildId}  (UE5.3)");
            Console.WriteLine("==================================================");
        }

        private static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine("  F1  — toggle Achievement Enable (re-enable with Advanced Game Settings)");
            Console.WriteLine("  F2  — toggle Instant Manual Craft (Craft Bench / Equipment Workshop)");
            Console.WriteLine("  F10 — quit trainer");
            Console.WriteLine();
            Console.WriteLine("  Both cheats start OFF. Load a save before toggling.");
            Console.WriteLine();
        }
    }
}
