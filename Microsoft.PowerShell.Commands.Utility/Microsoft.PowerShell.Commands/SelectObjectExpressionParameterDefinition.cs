namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;

    internal class SelectObjectExpressionParameterDefinition : CommandParameterDefinition
    {
        protected override void SetEntries()
        {
            base.hashEntries.Add(new ExpressionEntryDefinition());
            base.hashEntries.Add(new NameEntryDefinition());
        }
    }
}

