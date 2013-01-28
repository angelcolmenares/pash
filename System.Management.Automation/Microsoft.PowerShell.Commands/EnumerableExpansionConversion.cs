namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Runtime.InteropServices;

    internal static class EnumerableExpansionConversion
    {
        internal const string BothString = "Both";
        internal const string CoreOnlyString = "CoreOnly";
        internal const string EnumOnlyString = "EnumOnly";

        internal static bool Convert(string expansionString, out EnumerableExpansion expansion)
        {
            expansion = EnumerableExpansion.EnumOnly;
            if (string.Equals(expansionString, "CoreOnly", StringComparison.OrdinalIgnoreCase))
            {
                expansion = EnumerableExpansion.CoreOnly;
                return true;
            }
            if (string.Equals(expansionString, "EnumOnly", StringComparison.OrdinalIgnoreCase))
            {
                expansion = EnumerableExpansion.EnumOnly;
                return true;
            }
            if (string.Equals(expansionString, "Both", StringComparison.OrdinalIgnoreCase))
            {
                expansion = EnumerableExpansion.Both;
                return true;
            }
            return false;
        }
    }
}

