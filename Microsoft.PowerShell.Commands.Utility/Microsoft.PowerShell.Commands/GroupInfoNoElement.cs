namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    public sealed class GroupInfoNoElement : GroupInfo
    {
        internal GroupInfoNoElement(OrderByPropertyEntry groupValue) : base(groupValue)
        {
        }

        internal override void Add(PSObject groupValue)
        {
            base.count++;
        }
    }
}

