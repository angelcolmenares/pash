namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    public class JobRepository : Repository<Job>
    {
        internal JobRepository() : base("job")
        {
        }

        public Job GetJob(Guid instanceId)
        {
            return base.GetItem(instanceId);
        }

        protected override Guid GetKey(Job item)
        {
            if (item != null)
            {
                return item.InstanceId;
            }
            return Guid.Empty;
        }

        public List<Job> Jobs
        {
            get
            {
                return base.Items;
            }
        }
    }
}

