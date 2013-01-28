namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class LabelEntryDefinition : HashtableEntryDefinition
    {
        internal LabelEntryDefinition() : base("label", new string[] { "name" }, new Type[] { typeof(string) }, false)
        {
        }
    }
}

