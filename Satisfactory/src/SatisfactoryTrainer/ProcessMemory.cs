using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SatisfactoryTrainer
{
    /// <summary>
    /// Thin read/write wrapper over a target process's address space.
    /// Windows-only at runtime (P/Invoke kernel32); the trainer is meant to run
    /// on the same Windows box as the game. Compiles cross-platform.
    /// </summary>
    public sealed class ProcessMemory : IDisposable
    {
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int access, bool inherit, int pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(
            IntPtr handle, IntPtr baseAddress, byte[] buffer, int size, out IntPtr read);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteProcessMemory(
            IntPtr handle, IntPtr baseAddress, byte[] buffer, int size, out IntPtr written);

        private readonly Process _process;
        private IntPtr _handle;

        public ProcessMemory(Process process)
        {
            _process = process;
            _handle = OpenProcess(
                PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION,
                false, process.Id);

            if (_handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    $"OpenProcess failed (Win32 {Marshal.GetLastWin32Error()}). Run the trainer as Administrator.");
            }
        }

        public bool IsAlive => !_process.HasExited;

        /// <summary>Base address of a loaded module by file name, or 0 if not found.</summary>
        public long GetModuleBase(string moduleName)
        {
            // Modules is refreshed on access; the game loads all engine DLLs at startup.
            foreach (ProcessModule? m in _process.Modules)
            {
                if (m != null &&
                    string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    return m.BaseAddress.ToInt64();
                }
            }

            return 0;
        }

        public byte[] ReadBytes(long address, int size)
        {
            byte[] buffer = new byte[size];
            if (!ReadProcessMemory(_handle, new IntPtr(address), buffer, size, out IntPtr read) ||
                read.ToInt64() != size)
            {
                throw new AccessViolationException($"ReadProcessMemory failed at 0x{address:X} ({size} bytes).");
            }

            return buffer;
        }

        /// <summary>Best-effort read; returns false instead of throwing on failure.</summary>
        public bool TryReadBytes(long address, int size, out byte[] buffer)
        {
            buffer = new byte[size];
            return ReadProcessMemory(_handle, new IntPtr(address), buffer, size, out IntPtr read)
                   && read.ToInt64() == size;
        }

        public long ReadPtr(long address)
        {
            return BitConverter.ToInt64(ReadBytes(address, 8), 0);
        }

        public bool TryReadPtr(long address, out long value)
        {
            value = 0;
            if (!TryReadBytes(address, 8, out byte[] b))
            {
                return false;
            }

            value = BitConverter.ToInt64(b, 0);
            return true;
        }

        public int ReadInt(long address) => BitConverter.ToInt32(ReadBytes(address, 4), 0);

        public uint ReadUInt(long address) => BitConverter.ToUInt32(ReadBytes(address, 4), 0);

        public ushort ReadUShort(long address) => BitConverter.ToUInt16(ReadBytes(address, 2), 0);

        public float ReadFloat(long address) => BitConverter.ToSingle(ReadBytes(address, 4), 0);

        public byte ReadByte(long address) => ReadBytes(address, 1)[0];

        public void WriteBytes(long address, byte[] data)
        {
            if (!WriteProcessMemory(_handle, new IntPtr(address), data, data.Length, out IntPtr written) ||
                written.ToInt64() != data.Length)
            {
                throw new AccessViolationException($"WriteProcessMemory failed at 0x{address:X} ({data.Length} bytes).");
            }
        }

        public void WriteFloat(long address, float value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteByte(long address, byte value) => WriteBytes(address, new[] { value });

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
        }
    }
}
