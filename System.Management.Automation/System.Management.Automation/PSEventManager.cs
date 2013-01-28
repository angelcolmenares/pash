namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public abstract class PSEventManager
    {
        private int nextEventId = 1;
        private PSEventArgsCollection receivedEvents = new PSEventArgsCollection();

        internal abstract event EventHandler<PSEventArgs> ForwardEvent;

        protected PSEventManager()
        {
        }

        internal abstract void AddForwardedEvent(PSEventArgs forwardedEvent);
        protected abstract PSEventArgs CreateEvent(string sourceIdentifier, object sender, object[] args, PSObject extraData);
        public PSEventArgs GenerateEvent(string sourceIdentifier, object sender, object[] args, PSObject extraData)
        {
            return this.GenerateEvent(sourceIdentifier, sender, args, extraData, false, false);
        }

        internal PSEventArgs GenerateEvent(string sourceIdentifier, object sender, object[] args, PSObject extraData, bool processInCurrentThread, bool waitForCompletionInCurrentThread = false)
        {
            PSEventArgs newEvent = this.CreateEvent(sourceIdentifier, sender, args, extraData);
            this.ProcessNewEvent(newEvent, processInCurrentThread, waitForCompletionInCurrentThread);
            return newEvent;
        }

        public abstract IEnumerable<PSEventSubscriber> GetEventSubscribers(string sourceIdentifier);
        protected int GetNextEventId()
        {
            return this.nextEventId++;
        }

        protected abstract void ProcessNewEvent(PSEventArgs newEvent, bool processInCurrentThread);
        protected internal virtual void ProcessNewEvent(PSEventArgs newEvent, bool processInCurrentThread, bool waitForCompletionWhenInCurrentThread)
        {
            throw new NotImplementedException();
        }

        public abstract PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent);
        public abstract PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, ScriptBlock action, bool supportEvent, bool forwardEvent);
        public abstract PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent, int maxTriggerCount);
        public abstract PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, ScriptBlock action, bool supportEvent, bool forwardEvent, int maxTriggerCount);
        internal virtual PSEventSubscriber SubscribeEvent(object source, string eventName, string sourceIdentifier, PSObject data, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent, bool shouldQueueAndProcessInExecutionThread, int maxTriggerCount = 0)
        {
            return this.SubscribeEvent(source, eventName, sourceIdentifier, data, handlerDelegate, supportEvent, forwardEvent, maxTriggerCount);
        }

        public abstract void UnsubscribeEvent(PSEventSubscriber subscriber);

        public PSEventArgsCollection ReceivedEvents
        {
            get
            {
                return this.receivedEvents;
            }
        }

        public abstract List<PSEventSubscriber> Subscribers { get; }
    }
}

