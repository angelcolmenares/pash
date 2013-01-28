namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class FormatParameterDefinitionBase : CommandParameterDefinition
    {
        protected override void SetEntries()
        {
            base.hashEntries.Add(new ExpressionEntryDefinition());
            base.hashEntries.Add(new FormatStringDefinition());
        }
    }
}

