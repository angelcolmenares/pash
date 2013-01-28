namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;

    internal class GroupObjectExpressionParameterDefinition : CommandParameterDefinition
    {
        protected override void SetEntries()
        {
            base.hashEntries.Add(new ExpressionEntryDefinition(true));
        }
    }
}

