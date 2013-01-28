namespace System.Management.Automation.Tracing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Eventing;

    public class EtwEventCorrelator : IEtwEventCorrelator
    {
        private readonly EventDescriptor _transferEvent;
        private readonly EventProvider _transferProvider;

        public EtwEventCorrelator(EventProvider transferProvider, EventDescriptor transferEvent)
        {
            if (transferProvider == null)
            {
                throw new ArgumentNullException("transferProvider");
            }
            this._transferProvider = transferProvider;
            this._transferEvent = transferEvent;
        }

        public IEtwActivityReverter StartActivity()
        {
            return this.StartActivity(this.CurrentActivityId);
        }

        public IEtwActivityReverter StartActivity(Guid relatedActivityId)
        {
            EtwActivityReverter reverter = new EtwActivityReverter(this, this.CurrentActivityId);
            this.CurrentActivityId = EventProvider.CreateActivityId();
            if (relatedActivityId != Guid.Empty)
            {
                EventDescriptor eventDescriptor = this._transferEvent;
                this._transferProvider.WriteTransferEvent(ref eventDescriptor, relatedActivityId, new object[0]);
            }
            return reverter;
        }

        public Guid CurrentActivityId
        {
            get
            {
                return Trace.CorrelationManager.ActivityId;
            }
            set
            {
                EventProvider.SetActivityId(ref value);
            }
        }
    }
}

