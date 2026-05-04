using System;
using System.Collections.Generic;

namespace HumanicaCheats.Modules
{
    internal static class WarehouseResourceSnapshotPolicy
    {
        public static bool ShouldRestore(int multiplier, bool alreadyAttempted)
        {
            return multiplier > 1 && !alreadyAttempted;
        }

        public static int MissingAmount(int snapshotAmount, int currentAmount)
        {
            return Math.Max(0, snapshotAmount - currentAmount);
        }

        public static Dictionary<int, int> MergeHighWater(Dictionary<int, int> saved, Dictionary<int, int> current)
        {
            var merged = new Dictionary<int, int>(saved);
            foreach (var pair in current)
            {
                if (pair.Key <= 0 || pair.Value <= 0)
                {
                    continue;
                }

                if (!merged.TryGetValue(pair.Key, out int oldAmount) || pair.Value > oldAmount)
                {
                    merged[pair.Key] = pair.Value;
                }
            }

            return merged;
        }

        public static Dictionary<int, int> Parse(string value)
        {
            var result = new Dictionary<int, int>();
            if (string.IsNullOrWhiteSpace(value)) return result;

            string[] pairs = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] parts = pairs[i].Split(':');
                if (parts.Length != 2) continue;
                if (!int.TryParse(parts[0], out int idx)) continue;
                if (!int.TryParse(parts[1], out int amount)) continue;
                if (idx > 0 && amount > 0)
                {
                    result[idx] = amount;
                }
            }

            return result;
        }

        public static string Format(Dictionary<int, int> values)
        {
            var parts = new List<string>();
            foreach (var pair in values)
            {
                if (pair.Key > 0 && pair.Value > 0)
                {
                    parts.Add(pair.Key + ":" + pair.Value);
                }
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join(";", parts);
        }
    }
}
