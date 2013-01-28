namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class FormatTableParameterDefinition : FormatParameterDefinitionBase
    {
        protected override void SetEntries()
        {
            base.SetEntries();
            base.hashEntries.Add(new WidthEntryDefinition());
            base.hashEntries.Add(new AligmentEntryDefinition());
            base.hashEntries.Add(new LabelEntryDefinition());
        }
    }
}

