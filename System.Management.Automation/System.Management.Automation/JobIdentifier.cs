namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class JobIdentifier
    {
        internal JobIdentifier(int id, Guid instanceId)
        {
            if (id <= 0)
            {
                PSTraceSource.NewArgumentException("id", "remotingerroridstrings", "JobSessionIdLessThanOne", new object[] { id });
            }
            this.Id = id;
            this.InstanceId = instanceId;
        }

        internal int Id { get; private set; }

        internal Guid InstanceId { get; private set; }
    }
}

