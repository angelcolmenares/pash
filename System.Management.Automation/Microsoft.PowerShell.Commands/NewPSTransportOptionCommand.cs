namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("New", "PSTransportOption", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210608", RemotingCapability=RemotingCapability.None), OutputType(new Type[] { typeof(WSManConfigurationOption) })]
    public sealed class NewPSTransportOptionCommand : PSCmdlet
    {
        private WSManConfigurationOption option = new WSManConfigurationOption();

        protected override void ProcessRecord()
        {
            base.WriteObject(this.option);
        }

        [Parameter(ValueFromPipelineByPropertyName=true), ValidateRange(60, 0x20c49b)]
        public int? IdleTimeoutSec
        {
            get
            {
                return this.option.IdleTimeoutSec;
            }
            set
            {
                this.option.IdleTimeoutSec = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true), ValidateRange(1, 0x7fffffff)]
        public int? MaxConcurrentCommandsPerSession
        {
            get
            {
                return this.option.MaxConcurrentCommandsPerSession;
            }
            set
            {
                this.option.MaxConcurrentCommandsPerSession = value;
            }
        }

        [ValidateRange(1, 100), Parameter(ValueFromPipelineByPropertyName=true)]
        public int? MaxConcurrentUsers
        {
            get
            {
                return this.option.MaxConcurrentUsers;
            }
            set
            {
                this.option.MaxConcurrentUsers = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true), ValidateRange(60, 0x20c49b)]
        public int? MaxIdleTimeoutSec
        {
            get
            {
                return this.option.MaxIdleTimeoutSec;
            }
            set
            {
                this.option.MaxIdleTimeoutSec = value;
            }
        }

        [ValidateRange(5, 0x7fffffff), Parameter(ValueFromPipelineByPropertyName=true)]
        public int? MaxMemoryPerSessionMB
        {
            get
            {
                return this.option.MaxMemoryPerSessionMB;
            }
            set
            {
                this.option.MaxMemoryPerSessionMB = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true), ValidateRange(1, 0x7fffffff)]
        public int? MaxProcessesPerSession
        {
            get
            {
                return this.option.MaxProcessesPerSession;
            }
            set
            {
                this.option.MaxProcessesPerSession = value;
            }
        }

        [ValidateRange(1, 0x7fffffff), Parameter(ValueFromPipelineByPropertyName=true)]
        public int? MaxSessions
        {
            get
            {
                return this.option.MaxSessions;
            }
            set
            {
                this.option.MaxSessions = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true), ValidateRange(1, 0x7fffffff)]
        public int? MaxSessionsPerUser
        {
            get
            {
                return this.option.MaxSessionsPerUser;
            }
            set
            {
                this.option.MaxSessionsPerUser = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public System.Management.Automation.Runspaces.OutputBufferingMode? OutputBufferingMode
        {
            get
            {
                return this.option.OutputBufferingMode;
            }
            set
            {
                this.option.OutputBufferingMode = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true), ValidateRange(0, 0x127500)]
        public int? ProcessIdleTimeoutSec
        {
            get
            {
                return this.option.ProcessIdleTimeoutSec;
            }
            set
            {
                this.option.ProcessIdleTimeoutSec = value;
            }
        }
    }
}

