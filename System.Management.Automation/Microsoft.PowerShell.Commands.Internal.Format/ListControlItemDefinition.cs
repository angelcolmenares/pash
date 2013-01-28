namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class ListControlItemDefinition
    {
        internal ExpressionToken conditionToken;
        internal List<FormatToken> formatTokenList = new List<FormatToken>();
        internal TextToken label;
    }
}

