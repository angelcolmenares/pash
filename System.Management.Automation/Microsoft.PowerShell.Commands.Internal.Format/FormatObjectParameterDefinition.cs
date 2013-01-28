namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class FormatObjectParameterDefinition : CommandParameterDefinition
    {
        protected override void SetEntries()
        {
            base.hashEntries.Add(new ExpressionEntryDefinition());
            base.hashEntries.Add(new HashtableEntryDefinition("depth", new Type[] { typeof(int) }));
        }
    }
}

