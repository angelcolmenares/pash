namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal class PSRemoteEventManager : PSEventManager
    {
        private string computerName;
        private Guid runspaceId;

        internal override event EventHandler<PSEventArgs> ForwardEvent;

        internal PSRemoteEventManager(string computerName, Guid runspaceId)
        {
            this.computerName = computerName;
            this.runspaceId = runspaceId;
        }

        internal override void AddForwardedEvent(PSEventArgs forwardedEvent)
        {
            forwardedEvent.EventIdentifier = base.GetNextEventId();
            forwardedEvent.ForwardEvent = false;
            if ((forwardedEvent.ComputerName == null) || (forwardedEvent.ComputerName.Length == 0))
            {
                forwardedEvent.ComputerName = this.computerName;
                forwardedEvent.RunspaceId = this.runspaceId;
            }
            this.ProcessNewEvent(forwardedEvent, false);
        }

        protected override PSEventArgs CreateEvent(string sourceIdentifier, object sender, object[] args, PSObject extraData)
        {
            return new PSEventArgs(null, this.runspaceId, base.GetNextEventId(), sourceIdentifier, sender, args, extraData);
        }

        public override IEnumerable<PSEventSubscriber> GetEventSubscribers(string sourceIdentifier)
        {
            throw new NotSupportedException(EventingResources.RemoteOperationNotSupported);
        }

        protected virtual void OnForwardEvent(PSEventArgs e)
        {
            EventHandler<PSEventArgs> forwardEvent = this.ForwardEvent;
            if (forwardEvent != null)
            {
                forwardEvent(this, e);
            }
        }

        protected override void ProcessNewEvent(PSEventArgs newEvent, bool processInCurrentThread)
        {
            this.ProcessNewEvent(newEvent, processInCurrentThread, false);
        }

        protected internal override void ProcessNewEvent(PSEventArgs newEvent, bool processInCurrentThread, bool waitForCompletionInCurrentThread)
        {
            lock (base.ReceivedEvents.SyncRoot)
            {
                if (newEvent.ForwardEvent)
                {
                    this.OnForwardEvent(newEvent);
                }
                else
                {
                    base.ReceivedEvents.Add(newEvent);
                }
            }
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent)
        {
            throw new NotSupportedException(EventingResources.RemoteOperationNotSupported);
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, ScriptBlock action, bool supportEvent, bool forwardEvent)
        {
            throw new NotSupportedException(EventingResources.RemoteOperationNotSupported);
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent, int maxTriggerCount)
        {
            throw new NotSupportedException(EventingResources.RemoteOperationNotSupported);
        }

        public override PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, ScriptBlock action, bool supportEvent, bool forwardEvent, int maxTriggerCount)
        {
            throw new NotSupportedException(EventingResources.RemoteOperationNotSupported);
        }

        public override void UnsubscribeEvent(PSEventSubscriber subscriber)
        {
            throw new NotSupportedException(EventingResources.RemoteOperationNotSupported);
        }

        public override List<PSEventSubscriber> Subscribers
        {
            get
            {
                throw new NotSupportedException(EventingResources.RemoteOperationNotSupported);
            }
        }
    }
}

