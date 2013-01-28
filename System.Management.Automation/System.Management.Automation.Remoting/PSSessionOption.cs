namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    public sealed class PSSessionOption
    {
        private PSPrimitiveDictionary applicationArguments;
        private TimeSpan cancelTimeout = TimeSpan.FromMilliseconds(60000.0);
        private CultureInfo culture;
        private TimeSpan idleTimeout = TimeSpan.FromMilliseconds(-1.0);
        private bool includePortInSPN;
        private int maximumConnectionRedirectionCount = 5;
        private int? maxRecvdDataSizePerCommand;
        private int? maxRecvdObjectSize = 0xc800000;
        private bool noCompression;
        private bool noEncryption;
        private bool noMachineProfile;
        private TimeSpan openTimeout = TimeSpan.FromMilliseconds(180000.0);
        private TimeSpan operationTimeout = TimeSpan.FromMilliseconds(180000.0);
        private System.Management.Automation.Runspaces.OutputBufferingMode outputBufferingMode;
        private System.Management.Automation.Remoting.ProxyAccessType proxyAcessType;
        private AuthenticationMechanism proxyAuthentication = AuthenticationMechanism.Negotiate;
        private PSCredential proxyCredential;
        private bool skipCACheck;
        private bool skipCNCheck;
        private bool skipRevocationCheck;
        private CultureInfo uiCulture;
        private bool useUtf16;

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

        public TimeSpan CancelTimeout
        {
            get
            {
                return this.cancelTimeout;
            }
            set
            {
                this.cancelTimeout = value;
            }
        }

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

        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
            set
            {
                this.idleTimeout = value;
            }
        }

        public bool IncludePortInSPN
        {
            get
            {
                return this.includePortInSPN;
            }
            set
            {
                this.includePortInSPN = value;
            }
        }

        public int MaximumConnectionRedirectionCount
        {
            get
            {
                return this.maximumConnectionRedirectionCount;
            }
            set
            {
                this.maximumConnectionRedirectionCount = value;
            }
        }

        public int? MaximumReceivedDataSizePerCommand
        {
            get
            {
                return this.maxRecvdDataSizePerCommand;
            }
            set
            {
                this.maxRecvdDataSizePerCommand = value;
            }
        }

        public int? MaximumReceivedObjectSize
        {
            get
            {
                return this.maxRecvdObjectSize;
            }
            set
            {
                this.maxRecvdObjectSize = value;
            }
        }

        public bool NoCompression
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

        public bool NoEncryption
        {
            get
            {
                return this.noEncryption;
            }
            set
            {
                this.noEncryption = value;
            }
        }

        public bool NoMachineProfile
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

        public TimeSpan OpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
            set
            {
                this.openTimeout = value;
            }
        }

        public TimeSpan OperationTimeout
        {
            get
            {
                return this.operationTimeout;
            }
            set
            {
                this.operationTimeout = value;
            }
        }

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

        public System.Management.Automation.Remoting.ProxyAccessType ProxyAccessType
        {
            get
            {
                return this.proxyAcessType;
            }
            set
            {
                this.proxyAcessType = value;
            }
        }

        public AuthenticationMechanism ProxyAuthentication
        {
            get
            {
                return this.proxyAuthentication;
            }
            set
            {
                switch (value)
                {
                    case AuthenticationMechanism.Basic:
                    case AuthenticationMechanism.Negotiate:
                    case AuthenticationMechanism.Digest:
                        this.proxyAuthentication = value;
                        return;
                }
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ProxyAmbiguosAuthentication, new object[] { value, AuthenticationMechanism.Basic.ToString(), AuthenticationMechanism.Negotiate.ToString(), AuthenticationMechanism.Digest.ToString() }));
            }
        }

        public PSCredential ProxyCredential
        {
            get
            {
                return this.proxyCredential;
            }
            set
            {
                this.proxyCredential = value;
            }
        }

        public bool SkipCACheck
        {
            get
            {
                return this.skipCACheck;
            }
            set
            {
                this.skipCACheck = value;
            }
        }

        public bool SkipCNCheck
        {
            get
            {
                return this.skipCNCheck;
            }
            set
            {
                this.skipCNCheck = value;
            }
        }

        public bool SkipRevocationCheck
        {
            get
            {
                return this.skipRevocationCheck;
            }
            set
            {
                this.skipRevocationCheck = value;
            }
        }

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

        public bool UseUTF16
        {
            get
            {
                return this.useUtf16;
            }
            set
            {
                this.useUtf16 = value;
            }
        }
    }
}

