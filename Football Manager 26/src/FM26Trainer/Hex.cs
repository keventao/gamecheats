using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FM26Trainer
{
    public static class Hex
    {
        public static ulong ParseAddress(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Address is empty.");
            }

            string value = text.Trim();
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ulong.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            if (ContainsHexLetter(value))
            {
                return ulong.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static bool ContainsHexLetter(string value)
        {
            foreach (char c in value)
            {
                if ((c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                {
                    return true;
                }
            }

            return false;
        }

        public static byte[] ParseBytes(IEnumerable<string> tokens)
        {
            var bytes = new List<byte>();

            foreach (string token in tokens)
            {
                string[] parts = token.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    string value = part.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                        ? part.Substring(2)
                        : part;

                    if (value.Length == 0 || value.Length > 2)
                    {
                        throw new FormatException($"Invalid byte token '{part}'.");
                    }

                    bytes.Add(byte.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                }
            }

            return bytes.ToArray();
        }

        public static string FormatBytes(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(bytes.Length * 3);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(bytes[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
