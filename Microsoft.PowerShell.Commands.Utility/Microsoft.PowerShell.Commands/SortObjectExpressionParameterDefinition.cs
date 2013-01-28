namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;

    internal class SortObjectExpressionParameterDefinition : CommandParameterDefinition
    {
        protected override void SetEntries()
        {
            base.hashEntries.Add(new ExpressionEntryDefinition(false));
            base.hashEntries.Add(new BooleanEntryDefinition("ascending"));
            base.hashEntries.Add(new BooleanEntryDefinition("descending"));
        }
    }
}

