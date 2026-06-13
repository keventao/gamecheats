namespace SatisfactoryTrainer
{
    /// <summary>
    /// Pure address arithmetic for the UE5.3 object/name layout. No process or
    /// platform dependency — fully unit-testable off-game. See
    /// <c>refs/RE-notes.md</c> for the source of every formula.
    /// </summary>
    public static class UnrealMath
    {
        /// <summary>
        /// Chunk + in-chunk index for a flat object index in a
        /// FChunkedFixedUObjectArray. <paramref name="elementsPerChunk"/> is
        /// derived at runtime as MaxElements / MaxChunks.
        /// </summary>
        public static (int chunk, int inChunk) ChunkIndex(int flatIndex, int elementsPerChunk)
        {
            if (elementsPerChunk <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(elementsPerChunk));
            }

            return (flatIndex / elementsPerChunk, flatIndex % elementsPerChunk);
        }

        /// <summary>Address of the FUObjectItem for a given chunk/in-chunk index.</summary>
        public static long ItemAddress(long chunkBase, int inChunk)
        {
            return chunkBase + (long)inChunk * Offsets.FUObjectItem_Stride;
        }

        /// <summary>Split an FName ComparisonIndex into (block, byteOffsetInBlock).</summary>
        public static (int block, int byteOffset) FNameParts(uint comparisonIndex)
        {
            int block = (int)(comparisonIndex >> 16);
            int byteOffset = (int)(comparisonIndex & 0xFFFF) * 2; // stride = 2
            return (block, byteOffset);
        }

        /// <summary>
        /// Decode an FNameEntryHeader. Default (case-preserving OFF) layout puts
        /// the length in the top 10 bits (<paramref name="lenShift"/> = 6); the
        /// case-preserving layout uses shift 1. <c>bIsWide</c> is bit 0.
        /// </summary>
        public static (int len, bool isWide) DecodeNameHeader(ushort header, int lenShift)
        {
            bool isWide = (header & 1) != 0;
            int len = header >> lenShift;
            return (len, isWide);
        }
    }
}
