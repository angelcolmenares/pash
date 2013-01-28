namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class TableRowItemDefinition
    {
        internal int alignment;
        internal List<FormatToken> formatTokenList = new List<FormatToken>();
    }
}

