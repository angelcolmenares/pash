namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;
    using System.Threading;

    [Cmdlet("Unregister", "Event", SupportsShouldProcess=true, DefaultParameterSetName="BySource", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135269")]
    public class UnregisterEventCommand : PSCmdlet
    {
        private SwitchParameter force;
        private bool foundMatch;
        private WildcardPattern matchPattern;
        private string sourceIdentifier;
        private int subscriptionId = -1;

        protected override void ProcessRecord()
        {
            foreach (PSEventSubscriber subscriber in base.Events.Subscribers)
            {
                if ((((this.sourceIdentifier != null) && this.matchPattern.IsMatch(subscriber.SourceIdentifier)) || ((this.SubscriptionId >= 0) && (subscriber.SubscriptionId == this.SubscriptionId))) && (!subscriber.SupportEvent || (this.Force != 0)))
                {
                    this.foundMatch = true;
                    if (base.ShouldProcess(string.Format(Thread.CurrentThread.CurrentCulture, EventingStrings.EventSubscription, new object[] { subscriber.SourceIdentifier }), EventingStrings.Unsubscribe))
                    {
                        base.Events.UnsubscribeEvent(subscriber);
                    }
                }
            }
            if (((this.sourceIdentifier != null) && !WildcardPattern.ContainsWildcardCharacters(this.sourceIdentifier)) && !this.foundMatch)
            {
                ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, EventingStrings.EventSubscriptionNotFound, new object[] { this.sourceIdentifier })), "INVALID_SOURCE_IDENTIFIER", ErrorCategory.InvalidArgument, null);
                base.WriteError(errorRecord);
            }
            else if ((this.SubscriptionId >= 0) && !this.foundMatch)
            {
                ErrorRecord record2 = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, EventingStrings.EventSubscriptionNotFound, new object[] { this.SubscriptionId })), "INVALID_SUBSCRIPTION_IDENTIFIER", ErrorCategory.InvalidArgument, null);
                base.WriteError(record2);
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="BySource")]
        public string SourceIdentifier
        {
            get
            {
                return this.sourceIdentifier;
            }
            set
            {
                this.sourceIdentifier = value;
                if (value != null)
                {
                    this.matchPattern = new WildcardPattern(value, WildcardOptions.IgnoreCase);
                }
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ById")]
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
    }
}

