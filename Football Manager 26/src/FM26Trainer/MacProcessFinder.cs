using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FM26Trainer
{
    public sealed class MacProcessInfo
    {
        public MacProcessInfo(Process process, string path)
        {
            Process = process;
            Path = path;
        }

        public Process Process { get; }

        public string Path { get; }
    }

    public static class MacProcessFinder
    {
        private const uint ProcPidPathInfoMaxSize = 4096;
        private const string BundleNeedle = "/Football Manager 26/fm.app/Contents/MacOS/";

        [DllImport("/usr/lib/libproc.dylib", EntryPoint = "proc_pidpath")]
        private static extern int ProcPidPath(int pid, byte[] buffer, uint bufferSize);

        public static MacProcessInfo? FindFootballManager26()
        {
            foreach (Process process in Process.GetProcesses())
            {
                string path = TryGetPath(process);
                if (IsLikelyFm26(process.ProcessName, path))
                {
                    return new MacProcessInfo(process, path);
                }

                process.Dispose();
            }

            return null;
        }

        private static bool IsLikelyFm26(string processName, string path)
        {
            if (string.Equals(processName, "fm", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(processName, "Football Manager 26", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return path.IndexOf(BundleNeedle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string TryGetPath(Process process)
        {
            try
            {
                string? path = process.MainModule?.FileName;
                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }
            catch
            {
                // Fall through to libproc. MainModule often fails for protected processes.
            }

            byte[] buffer = new byte[ProcPidPathInfoMaxSize];
            int length = ProcPidPath(process.Id, buffer, ProcPidPathInfoMaxSize);
            if (length <= 0)
            {
                return string.Empty;
            }

            int nul = Array.IndexOf(buffer, (byte)0, 0, length);
            if (nul >= 0)
            {
                length = nul;
            }

            return Encoding.UTF8.GetString(buffer, 0, length);
        }
    }
}

