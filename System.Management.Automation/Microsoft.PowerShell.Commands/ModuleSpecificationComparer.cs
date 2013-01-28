namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;

    internal class ModuleSpecificationComparer : IEqualityComparer<ModuleSpecification>
    {
        public bool Equals(ModuleSpecification x, ModuleSpecification y)
        {
            bool flag = false;
            if ((x == null) && (y == null))
            {
                return true;
            }
            if ((x != null) && (y != null))
            {
                if ((x.Name != null) && (y.Name != null))
                {
                    flag = x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    flag = true;
                }
                if ((flag && x.Guid.HasValue) && y.Guid.HasValue)
                {
                    flag = x.Guid.Equals(y.Guid);
                }
                if ((flag && (x.Version != null)) && (y.Version != null))
                {
                    flag = x.Version.Equals(y.Version);
                }
            }
            return flag;
        }

        public int GetHashCode(ModuleSpecification obj)
        {
            int num = 0;
            if (obj != null)
            {
                if (obj.Name != null)
                {
                    num ^= obj.Name.GetHashCode();
                }
                if (obj.Guid.HasValue)
                {
                    num ^= obj.Guid.GetHashCode();
                }
                if (obj.Version != null)
                {
                    num ^= obj.Version.GetHashCode();
                }
            }
            return num;
        }
    }
}

