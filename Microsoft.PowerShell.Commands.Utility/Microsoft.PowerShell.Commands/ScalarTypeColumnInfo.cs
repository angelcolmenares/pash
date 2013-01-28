namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    internal class ScalarTypeColumnInfo : ColumnInfo
    {
        private Type type;

        internal ScalarTypeColumnInfo(Type type) : base(type.Name, type.Name)
        {
            this.type = type;
        }

        internal override object GetValue(PSObject liveObject)
        {
            object baseObject = liveObject.BaseObject;
            if (baseObject.GetType().Equals(this.type))
            {
                return ColumnInfo.LimitString(baseObject);
            }
            return null;
        }
    }
}

