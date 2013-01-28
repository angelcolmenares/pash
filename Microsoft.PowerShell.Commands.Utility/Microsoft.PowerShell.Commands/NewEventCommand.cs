namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(PSEventArgs) }), Cmdlet("New", "Event", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135234")]
    public class NewEventCommand : PSCmdlet
    {
        private PSObject[] eventArguments = new PSObject[0];
        private PSObject messageData;
        private PSObject sender;
        private string sourceIdentifier;

        protected override void EndProcessing()
        {
            object[] args = null;
            if (this.eventArguments != null)
            {
                args = new object[this.eventArguments.Length];
                int index = 0;
                foreach (PSObject obj2 in this.eventArguments)
                {
                    if (obj2 != null)
                    {
                        args[index] = obj2.BaseObject;
                    }
                    index++;
                }
            }
            object sender = null;
            if (this.sender != null)
            {
                sender = this.sender.BaseObject;
            }
            base.WriteObject(base.Events.GenerateEvent(this.sourceIdentifier, sender, args, this.messageData, true, false));
        }

        [Parameter(Position=2)]
        public PSObject[] EventArguments
        {
            get
            {
                return this.eventArguments;
            }
            set
            {
                if (this.eventArguments != null)
                {
                    this.eventArguments = value;
                }
            }
        }

        [Parameter(Position=3)]
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

        [Parameter(Position=1)]
        public PSObject Sender
        {
            get
            {
                return this.sender;
            }
            set
            {
                this.sender = value;
            }
        }

        [Parameter(Position=0, Mandatory=true)]
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
    }
}

