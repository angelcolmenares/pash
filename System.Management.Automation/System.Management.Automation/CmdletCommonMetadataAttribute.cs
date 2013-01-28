namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CmdletCommonMetadataAttribute : CmdletMetadataAttribute
    {
        private System.Management.Automation.ConfirmImpact confirmImpact = System.Management.Automation.ConfirmImpact.Medium;
        private string defaultParameterSetName;
        private string helpUri = string.Empty;
        private System.Management.Automation.RemotingCapability remotingCapability = System.Management.Automation.RemotingCapability.PowerShell;
        private bool supportsPaging;
        private bool supportsShouldProcess;
        private bool supportsTransactions;

        protected CmdletCommonMetadataAttribute()
        {
        }

        public System.Management.Automation.ConfirmImpact ConfirmImpact
        {
            get
            {
                return this.confirmImpact;
            }
            set
            {
                this.confirmImpact = value;
            }
        }

        public string DefaultParameterSetName
        {
            get
            {
                return this.defaultParameterSetName;
            }
            set
            {
                this.defaultParameterSetName = value;
            }
        }

        public string HelpUri
        {
            get
            {
                return this.helpUri;
            }
            set
            {
                this.helpUri = value;
            }
        }

        public System.Management.Automation.RemotingCapability RemotingCapability
        {
            get
            {
                return this.remotingCapability;
            }
            set
            {
                this.remotingCapability = value;
            }
        }

        public bool SupportsPaging
        {
            get
            {
                return this.supportsPaging;
            }
            set
            {
                this.supportsPaging = value;
            }
        }

        public bool SupportsShouldProcess
        {
            get
            {
                return this.supportsShouldProcess;
            }
            set
            {
                this.supportsShouldProcess = value;
            }
        }

        public bool SupportsTransactions
        {
            get
            {
                return this.supportsTransactions;
            }
            set
            {
                this.supportsTransactions = value;
            }
        }
    }
}

