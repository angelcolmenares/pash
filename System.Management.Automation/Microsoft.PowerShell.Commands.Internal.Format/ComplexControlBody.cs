namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System.Collections.Generic;

    internal sealed class ComplexControlBody : ControlBody
    {
        internal ComplexControlEntryDefinition defaultEntry;
        internal List<ComplexControlEntryDefinition> optionalEntryList = new List<ComplexControlEntryDefinition>();
    }
}

