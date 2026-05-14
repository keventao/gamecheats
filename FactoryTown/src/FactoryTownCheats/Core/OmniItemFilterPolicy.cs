using System;

namespace FactoryTownCheats.Core
{
    public static class OmniItemFilterPolicy
    {
        public const int RecipeIdBase = 50000;

        public static bool IsCandidateOutput(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return false;
            }

            if (string.Equals(itemName, "None", StringComparison.Ordinal) ||
                string.Equals(itemName, "Invalid", StringComparison.Ordinal))
            {
                return false;
            }

            if (itemName.StartsWith("Filter", StringComparison.Ordinal) ||
                itemName.StartsWith("Utility", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public static int RecipeIdForOutput(int outputItemValue)
        {
            return RecipeIdBase + outputItemValue;
        }
    }
}
