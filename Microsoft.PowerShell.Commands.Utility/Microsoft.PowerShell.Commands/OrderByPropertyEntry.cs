namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal sealed class OrderByPropertyEntry
    {
        internal PSObject inputObject;
        internal List<ObjectCommandPropertyValue> orderValues = new List<ObjectCommandPropertyValue>();
    }
}

