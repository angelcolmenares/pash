namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class TypeOrGroupReference
    {
        internal ExpressionToken conditionToken;
        internal string name;

        protected TypeOrGroupReference()
        {
        }
    }
}

