namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Threading;

    [Cmdlet("Get", "EventSubscriber", DefaultParameterSetName="BySource", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135155"), OutputType(new Type[] { typeof(PSEventSubscriber) })]
    public class GetEventSubscriberCommand : PSCmdlet
    {
        private SwitchParameter force;
        private WildcardPattern matchPattern;
        private string sourceIdentifier;
        private int subscriptionId = -1;

        protected override void ProcessRecord()
        {
            bool flag = false;
            List<PSEventSubscriber> list = new List<PSEventSubscriber>(base.Events.Subscribers);
            foreach (PSEventSubscriber subscriber in list)
            {
                if ((((this.sourceIdentifier == null) || this.matchPattern.IsMatch(subscriber.SourceIdentifier)) && ((this.subscriptionId < 0) || (subscriber.SubscriptionId == this.subscriptionId))) && (!subscriber.SupportEvent || (this.Force != 0)))
                {
                    base.WriteObject(subscriber);
                    flag = true;
                }
            }
            if (!flag)
            {
                bool flag2 = (this.sourceIdentifier != null) && !WildcardPattern.ContainsWildcardCharacters(this.sourceIdentifier);
                bool flag3 = this.subscriptionId >= 0;
                if (flag2 || flag3)
                {
                    object sourceIdentifier = null;
                    string format = null;
                    if (flag2)
                    {
                        sourceIdentifier = this.sourceIdentifier;
                        format = EventingStrings.EventSubscriptionSourceNotFound;
                    }
                    else if (flag3)
                    {
                        sourceIdentifier = this.subscriptionId;
                        format = EventingStrings.EventSubscriptionNotFound;
                    }
                    ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, format, new object[] { sourceIdentifier })), "INVALID_SOURCE_IDENTIFIER", ErrorCategory.InvalidArgument, null);
                    base.WriteError(errorRecord);
                }
            }
        }

        [Parameter(Position=1)]
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

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="BySource")]
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

        [Alias(new string[] { "Id" }), Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ById")]
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

