namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class PropertyTokenBase : FormatToken
    {
        internal ExpressionToken conditionToken;
        internal bool enumerateCollection;
        internal ExpressionToken expression = new ExpressionToken();

        protected PropertyTokenBase()
        {
        }
    }
}

