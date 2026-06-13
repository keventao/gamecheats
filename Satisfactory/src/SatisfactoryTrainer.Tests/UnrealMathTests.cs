using SatisfactoryTrainer;
using Xunit;

namespace SatisfactoryTrainer.Tests
{
    public class UnrealMathTests
    {
        [Theory]
        [InlineData(0, 65536, 0, 0)]
        [InlineData(1, 65536, 0, 1)]
        [InlineData(65535, 65536, 0, 65535)]
        [InlineData(65536, 65536, 1, 0)]
        [InlineData(131073, 65536, 2, 1)]
        public void ChunkIndex_splits_flat_index(int flat, int epc, int expChunk, int expIn)
        {
            (int chunk, int inChunk) = UnrealMath.ChunkIndex(flat, epc);
            Assert.Equal(expChunk, chunk);
            Assert.Equal(expIn, inChunk);
        }

        [Fact]
        public void ChunkIndex_rejects_nonpositive_chunk_size()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => UnrealMath.ChunkIndex(0, 0));
        }

        [Theory]
        [InlineData(0x1000, 0, 0x1000)]
        [InlineData(0x1000, 1, 0x1000 + 24)]
        [InlineData(0x1000, 10, 0x1000 + 240)]
        public void ItemAddress_uses_24_byte_stride(long chunkBase, int inChunk, long expected)
        {
            Assert.Equal(expected, UnrealMath.ItemAddress(chunkBase, inChunk));
        }

        [Theory]
        // id = (block << 16) | entryWithinBlock ; byteOffset = entryWithinBlock * 2
        [InlineData(0u, 0, 0)]
        [InlineData(5u, 0, 10)]
        [InlineData(0x0001_0000u, 1, 0)]
        [InlineData(0x0003_0008u, 3, 16)]
        public void FNameParts_splits_block_and_offset(uint id, int expBlock, int expByteOffset)
        {
            (int block, int byteOffset) = UnrealMath.FNameParts(id);
            Assert.Equal(expBlock, block);
            Assert.Equal(expByteOffset, byteOffset);
        }

        [Theory]
        // default layout: len in top 10 bits (shift 6), bIsWide bit 0
        [InlineData((ushort)(6 << 6), 6, 6, false)]        // len 6, ansi
        [InlineData((ushort)((6 << 6) | 1), 6, 6, true)]   // len 6, wide
        [InlineData((ushort)(1 << 6), 6, 1, false)]
        public void DecodeNameHeader_default_layout(ushort header, int shift, int expLen, bool expWide)
        {
            (int len, bool isWide) = UnrealMath.DecodeNameHeader(header, shift);
            Assert.Equal(expLen, len);
            Assert.Equal(expWide, isWide);
        }

        [Fact]
        public void DecodeNameHeader_casePreserving_layout_uses_shift1()
        {
            // shift 1: len in top 15 bits
            ushort header = (ushort)((10 << 1) | 1); // len 10, wide
            (int len, bool isWide) = UnrealMath.DecodeNameHeader(header, 1);
            Assert.Equal(10, len);
            Assert.True(isWide);
        }
    }

    public class OffsetsTests
    {
        [Fact]
        public void Engine_rvas_match_pdb_extraction()
        {
            // Guard against accidental edits — these are the values derived in
            // refs/RE-notes.md for build 493833.
            Assert.Equal(0x5A3620, Offsets.RvaGUObjectArray);
            Assert.Equal(0x7B8500, Offsets.RvaGNameBlocksDebug);
        }

        [Fact]
        public void Cheat_field_offsets_match_pdb()
        {
            Assert.Equal(320, Offsets.OnlineBackend_bSuppressAchievements);
            Assert.Equal(608, Offsets.WorkBench_mCurrentManufacturingProgress);
            Assert.Equal(628, Offsets.WorkBench_mIsProducing);
            Assert.Equal(600, Offsets.WorkBench_mCurrentRecipe);
        }

        [Fact]
        public void FUObjectItem_stride_is_24()
        {
            Assert.Equal(24, Offsets.FUObjectItem_Stride);
        }
    }
}
