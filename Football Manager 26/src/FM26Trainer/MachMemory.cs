using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FM26Trainer
{
    public sealed unsafe class MachMemory : IDisposable
    {
        private const int KernSuccess = 0;

        private readonly Process _process;
        private uint _task;

        [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "mach_task_self")]
        private static extern uint MachTaskSelf();

        [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "task_for_pid")]
        private static extern int TaskForPid(uint targetTask, int pid, out uint task);

        [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "mach_vm_read_overwrite")]
        private static extern int MachVmReadOverwrite(
            uint targetTask,
            ulong address,
            ulong size,
            IntPtr data,
            out ulong outSize);

        [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "mach_vm_write")]
        private static extern int MachVmWrite(uint targetTask, ulong address, IntPtr data, uint dataCount);

        [DllImport("/usr/lib/libSystem.dylib", EntryPoint = "mach_port_deallocate")]
        private static extern int MachPortDeallocate(uint task, uint name);

        public MachMemory(Process process)
        {
            if (!OperatingSystem.IsMacOS())
            {
                throw new PlatformNotSupportedException("MachMemory is macOS-only.");
            }

            _process = process;

            int kr = TaskForPid(MachTaskSelf(), process.Id, out _task);
            if (kr != KernSuccess)
            {
                throw new InvalidOperationException(
                    $"task_for_pid failed for PID {process.Id} with kern_return_t {kr}. " +
                    "macOS may require elevated/debug permissions for this target.");
            }
        }

        public bool IsAlive => !_process.HasExited;

        public byte[] ReadBytes(ulong address, int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            byte[] buffer = new byte[size];
            fixed (byte* ptr = buffer)
            {
                int kr = MachVmReadOverwrite(_task, address, (ulong)size, new IntPtr(ptr), out ulong outSize);
                if (kr != KernSuccess || outSize != (ulong)size)
                {
                    throw new AccessViolationException(
                        $"mach_vm_read_overwrite failed at 0x{address:X} ({size} bytes), kr={kr}, out={outSize}.");
                }
            }

            return buffer;
        }

        public bool TryReadBytes(ulong address, int size, out byte[] buffer)
        {
            buffer = Array.Empty<byte>();
            if (size < 0)
            {
                return false;
            }

            byte[] local = new byte[size];
            fixed (byte* ptr = local)
            {
                int kr = MachVmReadOverwrite(_task, address, (ulong)size, new IntPtr(ptr), out ulong outSize);
                if (kr != KernSuccess || outSize != (ulong)size)
                {
                    return false;
                }
            }

            buffer = local;
            return true;
        }

        public void WriteBytes(ulong address, byte[] data)
        {
            if (data.Length == 0)
            {
                return;
            }

            fixed (byte* ptr = data)
            {
                int kr = MachVmWrite(_task, address, new IntPtr(ptr), checked((uint)data.Length));
                if (kr != KernSuccess)
                {
                    throw new AccessViolationException(
                        $"mach_vm_write failed at 0x{address:X} ({data.Length} bytes), kr={kr}.");
                }
            }
        }

        public ushort ReadUInt16(ulong address) => BitConverter.ToUInt16(ReadBytes(address, 2), 0);

        public byte ReadByte(ulong address) => ReadBytes(address, 1)[0];

        public void WriteUInt16(ulong address, ushort value) => WriteBytes(address, BitConverter.GetBytes(value));

        public void WriteByte(ulong address, byte value) => WriteBytes(address, new[] { value });

        public void Dispose()
        {
            if (_task != 0)
            {
                MachPortDeallocate(MachTaskSelf(), _task);
                _task = 0;
            }

            _process.Dispose();
        }
    }
}
