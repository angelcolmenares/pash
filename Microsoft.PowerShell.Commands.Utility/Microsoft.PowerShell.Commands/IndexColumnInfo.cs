namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    internal class IndexColumnInfo : ColumnInfo
    {
        private int index;

        internal IndexColumnInfo(string staleObjectPropertyName, string displayName, int index) : base(staleObjectPropertyName, displayName)
        {
            this.index = index;
        }

        internal override object GetValue(PSObject liveObject)
        {
            return this.index++;
        }
    }
}

