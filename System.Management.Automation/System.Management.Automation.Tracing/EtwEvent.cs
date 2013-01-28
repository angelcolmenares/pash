namespace System.Management.Automation.Tracing
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EtwEvent : Attribute
    {
        private long eventId;

        public EtwEvent(long eventId)
        {
            this.eventId = eventId;
        }

        public long EventId
        {
            get
            {
                return this.eventId;
            }
        }
    }
}

