namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class FormatListParameterDefinition : FormatParameterDefinitionBase
    {
        protected override void SetEntries()
        {
            base.SetEntries();
            base.hashEntries.Add(new LabelEntryDefinition());
        }
    }
}

