namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class ComplexControlEntryDefinition
    {
        internal AppliesTo appliesTo;
        internal ComplexControlItemDefinition itemDefinition = new ComplexControlItemDefinition();
    }
}

