namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class PSEventSubscriber : IEquatable<PSEventSubscriber>
    {
        private PSEventJob action;
        private System.Management.Automation.ExecutionContext context;
        private string eventName;
        private bool forwardEvent;
        private PSEventReceivedEventHandler handlerDelegate;
        private bool shouldProcessInExecutionThread;
        private string sourceIdentifier;
        private object sourceObject;
        private int subscriptionId;
        private bool supportEvent;

        public event PSEventUnsubscribedEventHandler Unsubscribed;

        internal PSEventSubscriber(System.Management.Automation.ExecutionContext context, int id, object source, string eventName, string sourceIdentifier, bool supportEvent, bool forwardEvent, int maxTriggerCount)
        {
            this.context = context;
            this.subscriptionId = id;
            this.sourceObject = source;
            this.eventName = eventName;
            this.sourceIdentifier = sourceIdentifier;
            this.supportEvent = supportEvent;
            this.forwardEvent = forwardEvent;
            this.IsBeingUnsubscribed = false;
            this.RemainingActionsToProcess = 0;
            if (maxTriggerCount <= 0)
            {
                this.AutoUnregister = false;
                this.RemainingTriggerCount = -1;
            }
            else
            {
                this.AutoUnregister = true;
                this.RemainingTriggerCount = maxTriggerCount;
            }
        }

        internal PSEventSubscriber(System.Management.Automation.ExecutionContext context, int id, object source, string eventName, string sourceIdentifier, PSEventReceivedEventHandler handlerDelegate, bool supportEvent, bool forwardEvent, int maxTriggerCount) : this(context, id, source, eventName, sourceIdentifier, supportEvent, forwardEvent, maxTriggerCount)
        {
            this.handlerDelegate = handlerDelegate;
        }

        internal PSEventSubscriber(System.Management.Automation.ExecutionContext context, int id, object source, string eventName, string sourceIdentifier, ScriptBlock action, bool supportEvent, bool forwardEvent, int maxTriggerCount) : this(context, id, source, eventName, sourceIdentifier, supportEvent, forwardEvent, maxTriggerCount)
        {
            if (action != null)
            {
                ScriptBlock block = this.CreateBoundScriptBlock(action);
                this.action = new PSEventJob(context.Events, this, block, sourceIdentifier);
            }
        }

        private ScriptBlock CreateBoundScriptBlock(ScriptBlock scriptAction)
        {
            ScriptBlock block = this.context.Modules.CreateBoundScriptBlock(this.context, scriptAction, true);
            PSVariable variable = new PSVariable("script:Error", new ArrayList(), ScopedItemOptions.Constant);
            SessionStateInternal sessionStateInternal = block.SessionStateInternal;
            sessionStateInternal.GetScopeByID("script").SetVariable(variable.Name, variable, false, true, sessionStateInternal, CommandOrigin.Internal, false);
            return block;
        }

        public bool Equals(PSEventSubscriber other)
        {
            if (other == null)
            {
                return false;
            }
            return object.Equals(this.SubscriptionId, other.SubscriptionId);
        }

        public override int GetHashCode()
        {
            return this.SubscriptionId;
        }

        internal void OnPSEventUnsubscribed(object sender, PSEventUnsubscribedEventArgs e)
        {
            if (this.Unsubscribed != null)
            {
                this.Unsubscribed(sender, e);
            }
        }

        internal void RegisterJob()
        {
            if (!this.supportEvent && (this.Action != null))
            {
                ((LocalRunspace) this.context.CurrentRunspace).JobRepository.Add(this.action);
            }
        }

        public PSEventJob Action
        {
            get
            {
                return this.action;
            }
        }

        internal bool AutoUnregister { get; private set; }

        public string EventName
        {
            get
            {
                return this.eventName;
            }
        }

        public bool ForwardEvent
        {
            get
            {
                return this.forwardEvent;
            }
        }

        public PSEventReceivedEventHandler HandlerDelegate
        {
            get
            {
                return this.handlerDelegate;
            }
        }

        internal bool IsBeingUnsubscribed { get; set; }

        internal int RemainingActionsToProcess { get; set; }

        internal int RemainingTriggerCount { get; set; }

        internal bool ShouldProcessInExecutionThread
        {
            get
            {
                return this.shouldProcessInExecutionThread;
            }
            set
            {
                this.shouldProcessInExecutionThread = value;
            }
        }

        public string SourceIdentifier
        {
            get
            {
                return this.sourceIdentifier;
            }
        }

        public object SourceObject
        {
            get
            {
                return this.sourceObject;
            }
        }

        public int SubscriptionId
        {
            get
            {
                return this.subscriptionId;
            }
            set
            {
                this.subscriptionId = value;
            }
        }

        public bool SupportEvent
        {
            get
            {
                return this.supportEvent;
            }
        }
    }
}

