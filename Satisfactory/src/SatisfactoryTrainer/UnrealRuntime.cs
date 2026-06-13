using System;
using System.Collections.Generic;
using System.Text;

namespace SatisfactoryTrainer
{
    /// <summary>
    /// Reads the live UE5.3 object graph of the attached game: iterates
    /// GUObjectArray, decodes FNames, and finds objects by class name.
    /// </summary>
    public sealed class UnrealRuntime
    {
        private readonly ProcessMemory _mem;
        private readonly long _coreUObjectBase;
        private readonly long _coreBase;

        private long _gObjectArray;
        private long _nameBlocksBase;     // *GNameBlocksDebug  (Blocks[] base)
        private int _nameLenShift = 6;    // verified on init

        public UnrealRuntime(ProcessMemory mem, long coreUObjectBase, long coreBase)
        {
            _mem = mem;
            _coreUObjectBase = coreUObjectBase;
            _coreBase = coreBase;
        }

        public int ObjectCount { get; private set; }

        /// <summary>
        /// Resolve globals and self-verify FName decoding. Returns false (with a
        /// reason) if the layout looks wrong — e.g. after a game update.
        /// </summary>
        public bool Initialize(out string error)
        {
            _gObjectArray = _coreUObjectBase + Offsets.RvaGUObjectArray;
            _nameBlocksBase = _mem.ReadPtr(_coreBase + Offsets.RvaGNameBlocksDebug);

            long chunked = _gObjectArray + Offsets.FUObjectArray_ObjObjects;
            int numElements = _mem.ReadInt(chunked + Offsets.Chunked_NumElements);
            int maxElements = _mem.ReadInt(chunked + Offsets.Chunked_MaxElements);
            int maxChunks = _mem.ReadInt(chunked + Offsets.Chunked_MaxChunks);

            if (numElements <= 0 || numElements > 50_000_000 || maxChunks <= 0 || maxElements < numElements)
            {
                error = $"GUObjectArray looks wrong (Num={numElements}, Max={maxElements}, Chunks={maxChunks}). " +
                        "RVAs likely stale for this build.";
                return false;
            }

            ObjectCount = numElements;

            // Self-verify FName decode: walk a few objects and check we can read a
            // printable class name. If shift 6 yields garbage, try the
            // case-preserving layout (shift 1).
            foreach (int shift in new[] { 6, 1 })
            {
                _nameLenShift = shift;
                if (LooksLikeValidNames())
                {
                    error = string.Empty;
                    return true;
                }
            }

            error = "FName decode self-check failed (name pool layout changed?).";
            return false;
        }

        private bool LooksLikeValidNames()
        {
            int ok = 0, tried = 0;
            foreach ((long obj, _) in EnumerateObjects())
            {
                if (tried++ >= 200)
                {
                    break;
                }

                string name = GetObjectName(obj);
                if (name.Length is > 0 and <= 128 && IsPrintableAscii(name))
                {
                    ok++;
                }
            }

            return tried > 0 && ok >= tried * 0.8; // 80% printable => layout correct
        }

        private static bool IsPrintableAscii(string s)
        {
            foreach (char c in s)
            {
                if (c < 0x20 || c > 0x7E)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Enumerate live (obj address, internal index). Skips null slots.</summary>
        public IEnumerable<(long obj, int index)> EnumerateObjects()
        {
            long chunked = _gObjectArray + Offsets.FUObjectArray_ObjObjects;
            long objectsTable = _mem.ReadPtr(chunked + Offsets.Chunked_Objects);
            int numElements = _mem.ReadInt(chunked + Offsets.Chunked_NumElements);
            int maxElements = _mem.ReadInt(chunked + Offsets.Chunked_MaxElements);
            int maxChunks = _mem.ReadInt(chunked + Offsets.Chunked_MaxChunks);
            int elementsPerChunk = maxElements / maxChunks;

            long cachedChunkBase = 0;
            int cachedChunk = -1;

            for (int i = 0; i < numElements; i++)
            {
                (int chunk, int inChunk) = UnrealMath.ChunkIndex(i, elementsPerChunk);

                if (chunk != cachedChunk)
                {
                    if (!_mem.TryReadPtr(objectsTable + (long)chunk * 8, out cachedChunkBase) ||
                        cachedChunkBase == 0)
                    {
                        cachedChunk = chunk;
                        cachedChunkBase = 0;
                        continue;
                    }

                    cachedChunk = chunk;
                }

                if (cachedChunkBase == 0)
                {
                    continue;
                }

                long item = UnrealMath.ItemAddress(cachedChunkBase, inChunk);
                if (!_mem.TryReadPtr(item + Offsets.FUObjectItem_Object, out long obj) || obj == 0)
                {
                    continue;
                }

                yield return (obj, i);
            }
        }

        public long GetClass(long obj)
        {
            return _mem.TryReadPtr(obj + Offsets.UObject_ClassPrivate, out long c) ? c : 0;
        }

        /// <summary>Decode the FName at obj.NamePrivate into a string.</summary>
        public string GetObjectName(long obj)
        {
            if (!_mem.TryReadBytes(obj + Offsets.UObject_NamePrivate, 4, out byte[] idBytes))
            {
                return string.Empty;
            }

            uint id = BitConverter.ToUInt32(idBytes, 0);
            (int block, int byteOffset) = UnrealMath.FNameParts(id);

            if (!_mem.TryReadPtr(_nameBlocksBase + (long)block * 8, out long blockPtr) || blockPtr == 0)
            {
                return string.Empty;
            }

            long entry = blockPtr + byteOffset;
            if (!_mem.TryReadBytes(entry, 2, out byte[] headerBytes))
            {
                return string.Empty;
            }

            ushort header = BitConverter.ToUInt16(headerBytes, 0);
            (int len, bool isWide) = UnrealMath.DecodeNameHeader(header, _nameLenShift);
            if (len <= 0 || len > 1024)
            {
                return string.Empty;
            }

            int byteLen = isWide ? len * 2 : len;
            if (!_mem.TryReadBytes(entry + 2, byteLen, out byte[] str))
            {
                return string.Empty;
            }

            return isWide ? Encoding.Unicode.GetString(str) : Encoding.ASCII.GetString(str);
        }

        /// <summary>
        /// Find the UClass object whose name == <paramref name="className"/>.
        /// Cache the result; class pointers are stable for the session.
        /// </summary>
        public long FindClass(string className)
        {
            foreach ((long obj, _) in EnumerateObjects())
            {
                if (GetObjectName(obj) == className)
                {
                    return obj;
                }
            }

            return 0;
        }

        /// <summary>All live instances whose ClassPrivate == <paramref name="classPtr"/>.</summary>
        public List<long> FindInstancesOfClass(long classPtr)
        {
            var result = new List<long>();
            foreach ((long obj, _) in EnumerateObjects())
            {
                if (GetClass(obj) == classPtr)
                {
                    result.Add(obj);
                }
            }

            return result;
        }
    }
}
