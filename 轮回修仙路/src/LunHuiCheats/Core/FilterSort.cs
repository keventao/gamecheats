using System;
using System.Collections.Generic;
using System.Linq;

namespace LunHuiCheats.Core
{
    public enum SortKey { Name, Category, Recent }

    public static class FilterSort
    {
        public static List<T> Apply<T>(
            IEnumerable<T> items, string query, SortKey key,
            Func<T, string> nameOf, Func<T, string> catOf, Func<T, int> recentOf)
        {
            IEnumerable<T> seq = items ?? Enumerable.Empty<T>();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim();
                seq = seq.Where(x => (nameOf(x) ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            seq = key switch
            {
                SortKey.Category => seq.OrderBy(catOf, StringComparer.Ordinal).ThenBy(nameOf, StringComparer.Ordinal),
                SortKey.Recent   => seq.OrderByDescending(recentOf).ThenBy(nameOf, StringComparer.Ordinal),
                _                => seq.OrderBy(nameOf, StringComparer.Ordinal),
            };

            return seq.ToList();
        }
    }
}
