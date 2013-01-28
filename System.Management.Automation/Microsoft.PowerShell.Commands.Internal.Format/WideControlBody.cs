namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class WideControlBody : ControlBody
    {
        internal int alignment;
        internal int columns;
        internal WideControlEntryDefinition defaultEntryDefinition;
        internal List<WideControlEntryDefinition> optionalEntryList = new List<WideControlEntryDefinition>();
    }
}

