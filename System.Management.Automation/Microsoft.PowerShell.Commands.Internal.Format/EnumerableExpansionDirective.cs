namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class EnumerableExpansionDirective
    {
        internal AppliesTo appliesTo;
        internal EnumerableExpansion enumerableExpansion = EnumerableExpansion.EnumOnly;
    }
}

