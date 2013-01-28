namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal sealed class PSModuleInfoComparer : IEqualityComparer<PSModuleInfo>
    {
        public bool Equals(PSModuleInfo x, PSModuleInfo y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return false;
            }
            return ((string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) && (x.Guid == y.Guid)) && (x.Version == y.Version));
        }

        public int GetHashCode(PSModuleInfo obj)
        {
            int num = 0;
            if (obj != null)
            {
                num = 0x17;
                if (obj.Name != null)
                {
                    num = (num * 0x11) + obj.Name.GetHashCode();
                }
                if (obj.Guid != Guid.Empty)
                {
                    num = (num * 0x11) + obj.Guid.GetHashCode();
                }
                if (obj.Version != null)
                {
                    num = (num * 0x11) + obj.Version.GetHashCode();
                }
            }
            return num;
        }
    }
}

