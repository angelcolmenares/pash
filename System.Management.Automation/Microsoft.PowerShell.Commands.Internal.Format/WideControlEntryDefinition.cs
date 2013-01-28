namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class WideControlEntryDefinition
    {
        internal AppliesTo appliesTo;
        internal List<FormatToken> formatTokenList = new List<FormatToken>();
    }
}

