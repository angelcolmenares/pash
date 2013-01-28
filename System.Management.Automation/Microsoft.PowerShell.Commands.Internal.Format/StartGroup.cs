namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class StartGroup
    {
        internal ControlBase control;
        internal ExpressionToken expression;
        internal TextToken labelTextToken;
    }
}

