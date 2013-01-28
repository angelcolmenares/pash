namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class TypeGroupDefinition
    {
        internal string name;
        internal List<TypeReference> typeReferenceList = new List<TypeReference>();
    }
}

