namespace System.Management.Automation
{
    using System;
    using System.Runtime.InteropServices;

    internal class DriveScopeItemSearcher : ScopedItemSearcher<PSDriveInfo>
    {
        public DriveScopeItemSearcher(SessionStateInternal sessionState, VariablePath lookupPath) : base(sessionState, lookupPath)
        {
        }

        protected override bool GetScopeItem(SessionStateScope scope, VariablePath name, out PSDriveInfo drive)
        {
            bool flag = true;
            drive = scope.GetDrive(name.DriveName);
            if (drive == null)
            {
                flag = false;
            }
            return flag;
        }
    }
}

