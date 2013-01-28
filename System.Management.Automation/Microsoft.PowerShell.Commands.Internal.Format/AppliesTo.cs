namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class AppliesTo
    {
        internal List<TypeOrGroupReference> referenceList = new List<TypeOrGroupReference>();

        internal void AddAppliesToType(string typeName)
        {
            TypeReference item = new TypeReference {
                name = typeName
            };
            this.referenceList.Add(item);
        }
    }
}

