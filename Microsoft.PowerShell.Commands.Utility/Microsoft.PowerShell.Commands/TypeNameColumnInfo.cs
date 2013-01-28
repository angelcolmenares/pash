namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    internal class TypeNameColumnInfo : ColumnInfo
    {
        internal TypeNameColumnInfo(string staleObjectPropertyName, string displayName) : base(staleObjectPropertyName, displayName)
        {
        }

        internal override object GetValue(PSObject liveObject)
        {
            return liveObject.BaseObject.GetType().FullName;
        }
    }
}

