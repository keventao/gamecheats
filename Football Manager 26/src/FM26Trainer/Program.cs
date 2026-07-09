using System;
using System.Linq;

namespace FM26Trainer
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (!OperatingSystem.IsMacOS())
            {
                Console.WriteLine("FM26Trainer is macOS-only.");
                return 2;
            }

            if (args.Length == 0 || args[0] == "help" || args[0] == "--help" || args[0] == "-h")
            {
                PrintHelp();
                return 0;
            }

            try
            {
                return args[0] switch
                {
                    "probe" => Probe(),
                    "targets" => Targets(),
                    "read" => Read(args),
                    "write" => Write(args),
                    _ => Unknown(args[0])
                };
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Input error: {ex.Message}");
                return 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private static int Probe()
        {
            MacProcessInfo? target = MacProcessFinder.FindFootballManager26();
            if (target == null)
            {
                Console.WriteLine("Football Manager 26 process not found. Launch the game and load a save first.");
                return 1;
            }

            Console.WriteLine($"Found PID {target.Process.Id}: {target.Process.ProcessName}");
            Console.WriteLine($"Path: {target.Path}");

            try
            {
                using var mem = new MachMemory(target.Process);
                Console.WriteLine("Attach OK.");
                Console.WriteLine($"Process alive: {mem.IsAlive}");
                return 0;
            }
            finally
            {
                target.Process.Dispose();
            }
        }

        private static int Targets()
        {
            CheatTargets.Print();
            return 0;
        }

        private static int Read(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: read <address> <size>");
                return 2;
            }

            ulong address = Hex.ParseAddress(args[1]);
            int size = int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
            if (size <= 0)
            {
                throw new FormatException("Size must be positive.");
            }

            using MachMemory mem = Attach();
            byte[] bytes = mem.ReadBytes(address, size);
            Console.WriteLine($"0x{address:X}: {Hex.FormatBytes(bytes)}");
            return 0;
        }

        private static int Write(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: write <address> <hex bytes...> --yes");
                return 2;
            }

            bool confirmed = args.Any(a => string.Equals(a, "--yes", StringComparison.OrdinalIgnoreCase));
            if (!confirmed)
            {
                Console.WriteLine("Refusing to write without --yes.");
                return 2;
            }

            ulong address = Hex.ParseAddress(args[1]);
            byte[] bytes = Hex.ParseBytes(args.Skip(2).Where(a => a != "--yes"));
            if (bytes.Length == 0)
            {
                throw new FormatException("No bytes provided.");
            }

            using MachMemory mem = Attach();
            mem.WriteBytes(address, bytes);
            Console.WriteLine($"Wrote {bytes.Length} bytes at 0x{address:X}: {Hex.FormatBytes(bytes)}");
            return 0;
        }

        private static MachMemory Attach()
        {
            MacProcessInfo? target = MacProcessFinder.FindFootballManager26();
            if (target == null)
            {
                throw new InvalidOperationException("Football Manager 26 process not found.");
            }

            Console.WriteLine($"Using PID {target.Process.Id}: {target.Process.ProcessName}");
            Console.WriteLine($"Path: {target.Path}");
            return new MachMemory(target.Process);
        }

        private static int Unknown(string command)
        {
            Console.WriteLine($"Unknown command: {command}");
            PrintHelp();
            return 2;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("FM26Trainer - Mac external realtime memory base");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  targets                         Print researched CE target fields");
            Console.WriteLine("  probe                           Find FM26 and try task_for_pid");
            Console.WriteLine("  read <address> <size>           Read explicit bytes");
            Console.WriteLine("  write <address> <bytes> --yes   Write explicit bytes");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run --project FM26Trainer/FM26Trainer.csproj -- probe");
            Console.WriteLine("  dotnet run --project FM26Trainer/FM26Trainer.csproj -- read 0x12345678 16");
        }
    }
}

