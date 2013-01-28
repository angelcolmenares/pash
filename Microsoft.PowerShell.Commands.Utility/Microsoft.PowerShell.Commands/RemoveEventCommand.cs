namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;
    using System.Threading;

    [Cmdlet("Remove", "Event", SupportsShouldProcess=true, DefaultParameterSetName="BySource", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135247")]
    public class RemoveEventCommand : PSCmdlet
    {
        private int eventIdentifier = -1;
        private WildcardPattern matchPattern;
        private string sourceIdentifier;

        protected override void ProcessRecord()
        {
            bool flag = false;
            lock (base.Events.ReceivedEvents.SyncRoot)
            {
                PSEventArgsCollection receivedEvents = base.Events.ReceivedEvents;
                for (int i = receivedEvents.Count; i > 0; i--)
                {
                    PSEventArgs args = receivedEvents[i - 1];
                    if (((this.sourceIdentifier == null) || this.matchPattern.IsMatch(args.SourceIdentifier)) && ((this.eventIdentifier < 0) || (args.EventIdentifier == this.eventIdentifier)))
                    {
                        flag = true;
                        if (base.ShouldProcess(string.Format(Thread.CurrentThread.CurrentCulture, EventingStrings.EventResource, new object[] { args.SourceIdentifier }), EventingStrings.Remove))
                        {
                            receivedEvents.RemoveAt(i - 1);
                        }
                    }
                }
            }
            if (((this.sourceIdentifier != null) && !WildcardPattern.ContainsWildcardCharacters(this.sourceIdentifier)) && !flag)
            {
                ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, EventingStrings.SourceIdentifierNotFound, new object[] { this.sourceIdentifier })), "INVALID_SOURCE_IDENTIFIER", ErrorCategory.InvalidArgument, null);
                base.WriteError(errorRecord);
            }
            else if ((this.eventIdentifier >= 0) && !flag)
            {
                ErrorRecord record2 = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, EventingStrings.EventIdentifierNotFound, new object[] { this.eventIdentifier })), "INVALID_EVENT_IDENTIFIER", ErrorCategory.InvalidArgument, null);
                base.WriteError(record2);
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ByIdentifier")]
        public int EventIdentifier
        {
            get
            {
                return this.eventIdentifier;
            }
            set
            {
                this.eventIdentifier = value;
            }
        }

        [Parameter(Mandatory=true, Position=0, ParameterSetName="BySource")]
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
    }
}

