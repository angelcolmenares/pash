namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;

    internal class MshParameter
    {
        internal Hashtable hash;

        internal object GetEntry(string key)
        {
            if (this.hash.ContainsKey(key))
            {
                return this.hash[key];
            }
            return AutomationNull.Value;
        }
    }
}

