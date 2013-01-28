namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;

    [Cmdlet("Write", "Progress", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113428", RemotingCapability=RemotingCapability.None)]
    public sealed class WriteProgressCommand : PSCmdlet
    {
        private bool _completed;
        private string activity;
        private int activityId;
        private string currentOperation;
        private const string HelpMessageBaseName = "WriteProgressResourceStrings";
        private int parentId = -1;
        private int percentComplete = -1;
        private int secondsRemaining = -1;
        private int sourceId;
        private string status = WriteProgressResourceStrings.Processing;

        protected override void ProcessRecord()
        {
            ProgressRecord progressRecord = new ProgressRecord(this.Id, this.Activity, this.Status) {
                ParentActivityId = this.ParentId,
                PercentComplete = this.PercentComplete,
                SecondsRemaining = this.SecondsRemaining,
                CurrentOperation = this.CurrentOperation,
                RecordType = (this.Completed != 0) ? ProgressRecordType.Completed : ProgressRecordType.Processing
            };
            base.WriteProgress((long) this.SourceId, progressRecord);
        }

        [Parameter(Position=0, Mandatory=true, HelpMessageBaseName="WriteProgressResourceStrings", HelpMessageResourceId="ActivityParameterHelpMessage")]
        public string Activity
        {
            get
            {
                return this.activity;
            }
            set
            {
                this.activity = value;
            }
        }

        [Parameter]
        public SwitchParameter Completed
        {
            get
            {
                return this._completed;
            }
            set
            {
                this._completed = (bool) value;
            }
        }

        [Parameter]
        public string CurrentOperation
        {
            get
            {
                return this.currentOperation;
            }
            set
            {
                this.currentOperation = value;
            }
        }

        [ValidateRange(0, 0x7fffffff), Parameter(Position=2)]
        public int Id
        {
            get
            {
                return this.activityId;
            }
            set
            {
                this.activityId = value;
            }
        }

        [ValidateRange(-1, 0x7fffffff), Parameter]
        public int ParentId
        {
            get
            {
                return this.parentId;
            }
            set
            {
                this.parentId = value;
            }
        }

        [Parameter, ValidateRange(-1, 100)]
        public int PercentComplete
        {
            get
            {
                return this.percentComplete;
            }
            set
            {
                this.percentComplete = value;
            }
        }

        [Parameter]
        public int SecondsRemaining
        {
            get
            {
                return this.secondsRemaining;
            }
            set
            {
                this.secondsRemaining = value;
            }
        }

        [Parameter]
        public int SourceId
        {
            get
            {
                return this.sourceId;
            }
            set
            {
                this.sourceId = value;
            }
        }

        [Parameter(Position=1, HelpMessageBaseName="WriteProgressResourceStrings", HelpMessageResourceId="StatusParameterHelpMessage"), ValidateNotNullOrEmpty]
        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }
        }
    }
}

