namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class NameEntryDefinition : HashtableEntryDefinition
    {
        internal const string NameEntryKey = "name";

        internal NameEntryDefinition() : base("name", new string[] { "label" }, new Type[] { typeof(string) }, false)
        {
        }
    }
}

