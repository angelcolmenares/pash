namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Threading;

    [Cmdlet("Get", "Event", DefaultParameterSetName="BySource", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113453"), OutputType(new Type[] { typeof(PSEventArgs) })]
    public class GetEventCommand : PSCmdlet
    {
        private int eventId = -1;
        private WildcardPattern matchPattern;
        private string sourceIdentifier;

        protected override void EndProcessing()
        {
            bool flag = false;
            List<PSEventArgs> list = new List<PSEventArgs>(base.Events.ReceivedEvents);
            foreach (PSEventArgs args in list)
            {
                if (((this.sourceIdentifier == null) || this.matchPattern.IsMatch(args.SourceIdentifier)) && ((this.eventId < 0) || (args.EventIdentifier == this.eventId)))
                {
                    base.WriteObject(args);
                    flag = true;
                }
            }
            if (!flag)
            {
                bool flag2 = (this.sourceIdentifier != null) && !WildcardPattern.ContainsWildcardCharacters(this.sourceIdentifier);
                bool flag3 = this.eventId >= 0;
                if (flag2 || flag3)
                {
                    object sourceIdentifier = null;
                    string format = null;
                    if (flag2)
                    {
                        sourceIdentifier = this.sourceIdentifier;
                        format = EventingStrings.SourceIdentifierNotFound;
                    }
                    else if (flag3)
                    {
                        sourceIdentifier = this.eventId;
                        format = EventingStrings.EventIdentifierNotFound;
                    }
                    ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, format, new object[] { sourceIdentifier })), "INVALID_SOURCE_IDENTIFIER", ErrorCategory.InvalidArgument, null);
                    base.WriteError(errorRecord);
                }
            }
        }

        [Alias(new string[] { "Id" }), Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ById")]
        public int EventIdentifier
        {
            get
            {
                return this.eventId;
            }
            set
            {
                this.eventId = value;
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
    }
}

