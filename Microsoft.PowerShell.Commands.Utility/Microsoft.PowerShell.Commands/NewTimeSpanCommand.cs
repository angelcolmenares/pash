namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(TimeSpan) }), Cmdlet("New", "TimeSpan", DefaultParameterSetName="Date", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113360", RemotingCapability=RemotingCapability.None)]
    public sealed class NewTimeSpanCommand : PSCmdlet
    {
        private int days;
        private DateTime end;
        private bool endSpecified;
        private int hours;
        private int minutes;
        private int seconds;
        private DateTime start;
        private bool startSpecified;

        protected override void ProcessRecord()
        {
            TimeSpan span;
            DateTime now = DateTime.Now;
            DateTime end = now;
            string parameterSetName = base.ParameterSetName;
            if (parameterSetName == null)
            {
                return;
            }
            if (!(parameterSetName == "Date"))
            {
                if (!(parameterSetName == "Time"))
                {
                    return;
                }
            }
            else
            {
                if (this.startSpecified)
                {
                    now = this.Start;
                }
                if (this.endSpecified)
                {
                    end = this.End;
                }
                span = end.Subtract(now);
                goto Label_0078;
            }
            span = new TimeSpan(this.Days, this.Hours, this.Minutes, this.Seconds);
        Label_0078:
            base.WriteObject(span);
        }

        [Parameter(ParameterSetName="Time")]
        public int Days
        {
            get
            {
                return this.days;
            }
            set
            {
                this.days = value;
            }
        }

        [Parameter(Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="Date")]
        public DateTime End
        {
            get
            {
                return this.end;
            }
            set
            {
                this.end = value;
                this.endSpecified = true;
            }
        }

        [Parameter(ParameterSetName="Time")]
        public int Hours
        {
            get
            {
                return this.hours;
            }
            set
            {
                this.hours = value;
            }
        }

        [Parameter(ParameterSetName="Time")]
        public int Minutes
        {
            get
            {
                return this.minutes;
            }
            set
            {
                this.minutes = value;
            }
        }

        [Parameter(ParameterSetName="Time")]
        public int Seconds
        {
            get
            {
                return this.seconds;
            }
            set
            {
                this.seconds = value;
            }
        }

        [Alias(new string[] { "LastWriteTime" }), Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Date")]
        public DateTime Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value;
                this.startSpecified = true;
            }
        }
    }
}

