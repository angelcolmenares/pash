namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    internal class ToStringColumnInfo : ColumnInfo
    {
        private OutGridViewCommand parentCmdlet;

        internal ToStringColumnInfo(string staleObjectPropertyName, string displayName, OutGridViewCommand parentCmdlet) : base(staleObjectPropertyName, displayName)
        {
            this.parentCmdlet = parentCmdlet;
        }

        internal override object GetValue(PSObject liveObject)
        {
            return ColumnInfo.LimitString(this.parentCmdlet.ConvertToString(liveObject));
        }
    }
}

