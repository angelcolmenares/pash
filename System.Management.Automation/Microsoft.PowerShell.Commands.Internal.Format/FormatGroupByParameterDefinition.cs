namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class FormatGroupByParameterDefinition : CommandParameterDefinition
    {
        protected override void SetEntries()
        {
            base.hashEntries.Add(new ExpressionEntryDefinition());
            base.hashEntries.Add(new FormatStringDefinition());
            base.hashEntries.Add(new LabelEntryDefinition());
        }
    }
}

