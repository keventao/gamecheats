using System;
using FM26Trainer;
using Xunit;

namespace FM26Trainer.Tests
{
    public sealed class HexTests
    {
        [Fact]
        public void ParseAddressAcceptsHex()
        {
            Assert.Equal(0x1234UL, Hex.ParseAddress("0x1234"));
            Assert.Equal(0xABCDEFUL, Hex.ParseAddress("ABCDEF"));
        }

        [Fact]
        public void ParseAddressAcceptsDecimal()
        {
            Assert.Equal(1234UL, Hex.ParseAddress("1234"));
        }

        [Fact]
        public void ParseBytesAcceptsCommonForms()
        {
            byte[] bytes = Hex.ParseBytes(new[] { "00", "0x7F", "aa,BB", "cc dd" });

            Assert.Equal(new byte[] { 0x00, 0x7F, 0xAA, 0xBB, 0xCC, 0xDD }, bytes);
        }

        [Fact]
        public void FormatBytesUsesUppercasePairs()
        {
            Assert.Equal("00 7F AA", Hex.FormatBytes(new byte[] { 0x00, 0x7F, 0xAA }));
        }

        [Fact]
        public void ParseBytesRejectsWideByteToken()
        {
            Assert.Throws<FormatException>(() => Hex.ParseBytes(new[] { "123" }));
        }
    }
}

