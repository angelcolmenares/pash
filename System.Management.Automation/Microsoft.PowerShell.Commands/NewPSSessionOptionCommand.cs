namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    [OutputType(new Type[] { typeof(PSSessionOption) }), Cmdlet("New", "PSSessionOption", HelpUri="http://go.microsoft.com/fwlink/?LinkID=144305", RemotingCapability=RemotingCapability.None)]
    public sealed class NewPSSessionOptionCommand : PSCmdlet
    {
        private System.Management.Automation.Remoting.ProxyAccessType _proxyacesstype;
        private PSCredential _proxycredential;
        private PSPrimitiveDictionary applicationArguments;
        private int? cancelTimeout;
        private CultureInfo culture;
        private int? idleTimeout;
        private bool includePortInSPN;
        private int? maximumRedirection;
        private int? maxRecvdDataSizePerCommand;
        private int? maxRecvdObjectSize;
        private SwitchParameter noCompression;
        private bool noencryption;
        private SwitchParameter noMachineProfile;
        private int? openTimeout;
        private int? operationtimeout;
        private System.Management.Automation.Runspaces.OutputBufferingMode outputBufferingMode;
        private AuthenticationMechanism proxyauthentication = AuthenticationMechanism.Negotiate;
        private bool skipcacheck;
        private bool skipcncheck;
        private bool skiprevocationcheck;
        private CultureInfo uiCulture;
        private bool useutf16;

        protected override void BeginProcessing()
        {
            PSSessionOption sendToPipeline = new PSSessionOption {
                ProxyAccessType = this.ProxyAccessType,
                ProxyAuthentication = this.ProxyAuthentication,
                ProxyCredential = this.ProxyCredential,
                SkipCACheck = (bool) this.SkipCACheck,
                SkipCNCheck = (bool) this.SkipCNCheck,
                SkipRevocationCheck = (bool) this.SkipRevocationCheck
            };
            if (this.operationtimeout.HasValue)
            {
                sendToPipeline.OperationTimeout = TimeSpan.FromMilliseconds((double) this.operationtimeout.Value);
            }
            sendToPipeline.NoEncryption = (bool) this.NoEncryption;
            sendToPipeline.UseUTF16 = (bool) this.UseUTF16;
            sendToPipeline.IncludePortInSPN = (bool) this.IncludePortInSPN;
            if (this.maximumRedirection.HasValue)
            {
                sendToPipeline.MaximumConnectionRedirectionCount = this.MaximumRedirection;
            }
            sendToPipeline.NoCompression = this.NoCompression.IsPresent;
            sendToPipeline.NoMachineProfile = this.NoMachineProfile.IsPresent;
            sendToPipeline.MaximumReceivedDataSizePerCommand = this.maxRecvdDataSizePerCommand;
            sendToPipeline.MaximumReceivedObjectSize = this.maxRecvdObjectSize;
            if (this.Culture != null)
            {
                sendToPipeline.Culture = this.Culture;
            }
            if (this.UICulture != null)
            {
                sendToPipeline.UICulture = this.UICulture;
            }
            if (this.openTimeout.HasValue)
            {
                sendToPipeline.OpenTimeout = TimeSpan.FromMilliseconds((double) this.openTimeout.Value);
            }
            if (this.cancelTimeout.HasValue)
            {
                sendToPipeline.CancelTimeout = TimeSpan.FromMilliseconds((double) this.cancelTimeout.Value);
            }
            if (this.idleTimeout.HasValue)
            {
                sendToPipeline.IdleTimeout = TimeSpan.FromMilliseconds((double) this.idleTimeout.Value);
            }
            sendToPipeline.OutputBufferingMode = this.outputBufferingMode;
            if (this.ApplicationArguments != null)
            {
                sendToPipeline.ApplicationArguments = this.ApplicationArguments;
            }
            base.WriteObject(sendToPipeline);
        }

        [Parameter, ValidateNotNull]
        public PSPrimitiveDictionary ApplicationArguments
        {
            get
            {
                return this.applicationArguments;
            }
            set
            {
                this.applicationArguments = value;
            }
        }

        [Parameter, Alias(new string[] { "CancelTimeoutMSec" }), ValidateRange(0, 0x7fffffff)]
        public int CancelTimeout
        {
            get
            {
                if (!this.cancelTimeout.HasValue)
                {
                    return 0xea60;
                }
                return this.cancelTimeout.Value;
            }
            set
            {
                this.cancelTimeout = new int?(value);
            }
        }

        [ValidateNotNull, Parameter]
        public CultureInfo Culture
        {
            get
            {
                return this.culture;
            }
            set
            {
                this.culture = value;
            }
        }

        [Parameter, ValidateRange(-1, 0x7fffffff), Alias(new string[] { "IdleTimeoutMSec" })]
        public int IdleTimeout
        {
            get
            {
                if (!this.idleTimeout.HasValue)
                {
                    return -1;
                }
                return this.idleTimeout.Value;
            }
            set
            {
                this.idleTimeout = new int?(value);
            }
        }

        [Parameter]
        public SwitchParameter IncludePortInSPN
        {
            get
            {
                return this.includePortInSPN;
            }
            set
            {
                this.includePortInSPN = (bool) value;
            }
        }

        [Parameter]
        public int MaximumReceivedDataSizePerCommand
        {
            get
            {
                return this.maxRecvdDataSizePerCommand.Value;
            }
            set
            {
                this.maxRecvdDataSizePerCommand = new int?(value);
            }
        }

        [Parameter]
        public int MaximumReceivedObjectSize
        {
            get
            {
                return this.maxRecvdObjectSize.Value;
            }
            set
            {
                this.maxRecvdObjectSize = new int?(value);
            }
        }

        [Parameter]
        public int MaximumRedirection
        {
            get
            {
                return this.maximumRedirection.Value;
            }
            set
            {
                this.maximumRedirection = new int?(value);
            }
        }

        [Parameter]
        public SwitchParameter NoCompression
        {
            get
            {
                return this.noCompression;
            }
            set
            {
                this.noCompression = value;
            }
        }

        [Parameter]
        public SwitchParameter NoEncryption
        {
            get
            {
                return this.noencryption;
            }
            set
            {
                this.noencryption = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter NoMachineProfile
        {
            get
            {
                return this.noMachineProfile;
            }
            set
            {
                this.noMachineProfile = value;
            }
        }

        [Parameter, Alias(new string[] { "OpenTimeoutMSec" }), ValidateRange(0, 0x7fffffff)]
        public int OpenTimeout
        {
            get
            {
                if (!this.openTimeout.HasValue)
                {
                    return 0x2bf20;
                }
                return this.openTimeout.Value;
            }
            set
            {
                this.openTimeout = new int?(value);
            }
        }

        [ValidateRange(0, 0x7fffffff), Parameter, Alias(new string[] { "OperationTimeoutMSec" })]
        public int OperationTimeout
        {
            get
            {
                if (!this.operationtimeout.HasValue)
                {
                    return 0x2bf20;
                }
                return this.operationtimeout.Value;
            }
            set
            {
                this.operationtimeout = new int?(value);
            }
        }

        [Parameter]
        public System.Management.Automation.Runspaces.OutputBufferingMode OutputBufferingMode
        {
            get
            {
                return this.outputBufferingMode;
            }
            set
            {
                this.outputBufferingMode = value;
            }
        }

        [Parameter, ValidateNotNullOrEmpty]
        public System.Management.Automation.Remoting.ProxyAccessType ProxyAccessType
        {
            get
            {
                return this._proxyacesstype;
            }
            set
            {
                this._proxyacesstype = value;
            }
        }

        [Parameter]
        public AuthenticationMechanism ProxyAuthentication
        {
            get
            {
                return this.proxyauthentication;
            }
            set
            {
                this.proxyauthentication = value;
            }
        }

        [Credential, Parameter, ValidateNotNullOrEmpty]
        public PSCredential ProxyCredential
        {
            get
            {
                return this._proxycredential;
            }
            set
            {
                this._proxycredential = value;
            }
        }

        [Parameter]
        public SwitchParameter SkipCACheck
        {
            get
            {
                return this.skipcacheck;
            }
            set
            {
                this.skipcacheck = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter SkipCNCheck
        {
            get
            {
                return this.skipcncheck;
            }
            set
            {
                this.skipcncheck = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter SkipRevocationCheck
        {
            get
            {
                return this.skiprevocationcheck;
            }
            set
            {
                this.skiprevocationcheck = (bool) value;
            }
        }

        [ValidateNotNull, Parameter]
        public CultureInfo UICulture
        {
            get
            {
                return this.uiCulture;
            }
            set
            {
                this.uiCulture = value;
            }
        }

        [Parameter]
        public SwitchParameter UseUTF16
        {
            get
            {
                return this.useutf16;
            }
            set
            {
                this.useutf16 = (bool) value;
            }
        }
    }
}

