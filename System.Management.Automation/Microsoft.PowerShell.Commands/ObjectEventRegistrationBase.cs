namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Threading;

    public abstract class ObjectEventRegistrationBase : PSCmdlet
    {
        private int _maxTriggerCount;
        private ScriptBlock action;
        private SwitchParameter forward = new SwitchParameter();
        private PSObject messageData;
        private PSEventSubscriber newSubscriber;
        private string sourceIdentifier = Guid.NewGuid().ToString();
        private SwitchParameter supportEvent = new SwitchParameter();

        protected ObjectEventRegistrationBase()
        {
        }

        protected override void BeginProcessing()
        {
            if ((this.forward != false) && (this.action != null))
            {
                base.ThrowTerminatingError(new ErrorRecord(new ArgumentException(EventingResources.ActionAndForwardNotSupported), "ACTION_AND_FORWARD_NOT_SUPPORTED", ErrorCategory.InvalidOperation, null));
            }
        }

        protected override void EndProcessing()
        {
            object targetObject = PSObject.Base(this.GetSourceObject());
            string sourceObjectEventName = this.GetSourceObjectEventName();
            try
            {
                if (((targetObject != null) || (sourceObjectEventName != null)) && base.Events.GetEventSubscribers(this.sourceIdentifier).GetEnumerator().MoveNext())
                {
                    ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, EventingResources.SubscriberExists, new object[] { this.sourceIdentifier })), "SUBSCRIBER_EXISTS", ErrorCategory.InvalidArgument, targetObject);
                    base.WriteError(errorRecord);
                }
                else
                {
                    this.newSubscriber = base.Events.SubscribeEvent(targetObject, sourceObjectEventName, this.sourceIdentifier, this.messageData, this.action, (bool) this.supportEvent, (bool) this.forward, this._maxTriggerCount);
                    if ((this.action != null) && (this.supportEvent == false))
                    {
                        base.WriteObject(this.newSubscriber.Action);
                    }
                }
            }
            catch (ArgumentException exception)
            {
                ErrorRecord record2 = new ErrorRecord(exception, "INVALID_REGISTRATION", ErrorCategory.InvalidArgument, targetObject);
                base.WriteError(record2);
            }
            catch (InvalidOperationException exception2)
            {
                ErrorRecord record3 = new ErrorRecord(exception2, "INVALID_REGISTRATION", ErrorCategory.InvalidOperation, targetObject);
                base.WriteError(record3);
            }
        }

        protected abstract object GetSourceObject();
        protected abstract string GetSourceObjectEventName();

        [Parameter(Position=0x65)]
        public ScriptBlock Action
        {
            get
            {
                return this.action;
            }
            set
            {
                this.action = value;
            }
        }

        [Parameter]
        public SwitchParameter Forward
        {
            get
            {
                return this.forward;
            }
            set
            {
                this.forward = value;
            }
        }

        [Parameter]
        public int MaxTriggerCount
        {
            get
            {
                return this._maxTriggerCount;
            }
            set
            {
                this._maxTriggerCount = (value <= 0) ? 0 : value;
            }
        }

        [Parameter]
        public PSObject MessageData
        {
            get
            {
                return this.messageData;
            }
            set
            {
                this.messageData = value;
            }
        }

        protected PSEventSubscriber NewSubscriber
        {
            get
            {
                return this.newSubscriber;
            }
        }

        [Parameter(Position=100)]
        public string SourceIdentifier
        {
            get
            {
                return this.sourceIdentifier;
            }
            set
            {
                this.sourceIdentifier = value;
            }
        }

        [Parameter]
        public SwitchParameter SupportEvent
        {
            get
            {
                return this.supportEvent;
            }
            set
            {
                this.supportEvent = value;
            }
        }
    }
}

