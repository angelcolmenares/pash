namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Runspaces;

    public class RunspaceRepository : Repository<PSSession>
    {
        internal RunspaceRepository() : base("runspace")
        {
        }

        internal void AddOrReplace(PSSession item)
        {
            if (base.Dictionary.ContainsKey(this.GetKey(item)))
            {
                base.Dictionary.Remove(this.GetKey(item));
            }
            base.Dictionary.Add(item.InstanceId, item);
        }

        protected override Guid GetKey(PSSession item)
        {
            if (item != null)
            {
                return item.InstanceId;
            }
            return Guid.Empty;
        }

        public List<PSSession> Runspaces
        {
            get
            {
                return base.Items;
            }
        }
    }
}

