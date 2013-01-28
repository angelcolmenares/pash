namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    public abstract class PSRemotingBaseCmdlet : PSRemotingCmdlet
    {
        private bool allowRedirection;
        private string appName;
        private AuthenticationMechanism authMechanism;
        private string[] computerNames;
        internal const string DEFAULT_SESSION_OPTION = "PSSessionOption";
        private int port;
        private PSCredential pscredential;
        private PSSession[] remoteRunspaceInfos;
        private string[] resolvedComputerNames;
        private PSSessionOption sessionOption;
        private string shell;
        private int throttleLimit;
        private string thumbPrint;
        protected const string UriParameterSet = "Uri";
        private Uri[] uris;
        private SwitchParameter useSSL;

        protected PSRemotingBaseCmdlet()
        {
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            int totalMilliseconds = (int) this.SessionOption.IdleTimeout.TotalMilliseconds;
            if ((totalMilliseconds != -1) && (totalMilliseconds < 0xea60))
            {
                throw new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.InvalidIdleTimeoutOption, totalMilliseconds / 0x3e8, 60));
            }
            if (string.IsNullOrEmpty(this.shell))
            {
                this.shell = base.ResolveShell(null);
            }
            if (string.IsNullOrEmpty(this.appName))
            {
                this.appName = base.ResolveAppName(null);
            }
        }

        internal void UpdateConnectionInfo(WSManConnectionInfo connectionInfo)
        {
            connectionInfo.SetSessionOptions(this.SessionOption);
            if (!base.ParameterSetName.Equals("Uri", StringComparison.OrdinalIgnoreCase))
            {
                connectionInfo.MaximumConnectionRedirectionCount = 0;
            }
            if (!this.allowRedirection)
            {
                connectionInfo.MaximumConnectionRedirectionCount = 0;
            }
        }

        protected void ValidateComputerName(string[] computerNames)
        {
            foreach (string str in computerNames)
            {
                UriHostNameType type = Uri.CheckHostName(str);
                if (((type != UriHostNameType.Dns) && (type != UriHostNameType.IPv4)) && (type != UriHostNameType.IPv6))
                {
                    base.ThrowTerminatingError(new ErrorRecord(new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.InvalidComputerName, new object[0])), "PSSessionInvalidComputerName", ErrorCategory.InvalidArgument, computerNames));
                }
            }
        }

        protected void ValidateRemoteRunspacesSpecified()
        {
            if (RemotingCommandUtil.HasRepeatingRunspaces(this.Session))
            {
                base.ThrowTerminatingError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.RemoteRunspaceInfoHasDuplicates)), PSRemotingErrorId.RemoteRunspaceInfoHasDuplicates.ToString(), ErrorCategory.InvalidArgument, this.Session));
            }
            if (RemotingCommandUtil.ExceedMaximumAllowableRunspaces(this.Session))
            {
                base.ThrowTerminatingError(new ErrorRecord(new ArgumentException(base.GetMessage(RemotingErrorIdStrings.RemoteRunspaceInfoLimitExceeded)), PSRemotingErrorId.RemoteRunspaceInfoLimitExceeded.ToString(), ErrorCategory.InvalidArgument, this.Session));
            }
        }

        internal static void ValidateSpecifiedAuthentication(PSCredential credential, string thumbprint, AuthenticationMechanism authentication)
        {
            if ((credential != null) && (thumbprint != null))
            {
                throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.NewRunspaceAmbiguosAuthentication, new object[] { "CertificateThumbPrint", "Credential" }));
            }
            if ((authentication != AuthenticationMechanism.Default) && (thumbprint != null))
            {
                throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.NewRunspaceAmbiguosAuthentication, new object[] { "CertificateThumbPrint", authentication.ToString() }));
            }
            if ((authentication == AuthenticationMechanism.NegotiateWithImplicitCredential) && (credential != null))
            {
                throw new InvalidOperationException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.NewRunspaceAmbiguosAuthentication, new object[] { "Credential", authentication.ToString() }));
            }
        }

        [Parameter(ParameterSetName="Uri")]
        public virtual SwitchParameter AllowRedirection
        {
            get
            {
                return this.allowRedirection;
            }
            set
            {
                this.allowRedirection = (bool) value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
        public virtual string ApplicationName
        {
            get
            {
                return this.appName;
            }
            set
            {
                this.appName = base.ResolveAppName(value);
            }
        }

        [Parameter(ParameterSetName="Uri"), Parameter(ParameterSetName="ComputerName")]
        public virtual AuthenticationMechanism Authentication
        {
            get
            {
                return this.authMechanism;
            }
            set
            {
                this.authMechanism = value;
                ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Uri")]
        public virtual string CertificateThumbprint
        {
            get
            {
                return this.thumbPrint;
            }
            set
            {
                this.thumbPrint = value;
                ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Alias(new string[] { "Cn" })]
        public virtual string[] ComputerName
        {
            get
            {
                return this.computerNames;
            }
            set
            {
                this.computerNames = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Uri"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
        public virtual string ConfigurationName
        {
            get
            {
                return this.shell;
            }
            set
            {
                this.shell = base.ResolveShell(value);
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Uri"), ValidateNotNullOrEmpty, Alias(new string[] { "URI", "CU" })]
        public virtual Uri[] ConnectionUri
        {
            get
            {
                return this.uris;
            }
            set
            {
                this.uris = value;
            }
        }

        [Credential, Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Uri"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
        public virtual PSCredential Credential
        {
            get
            {
                return this.pscredential;
            }
            set
            {
                this.pscredential = value;
                ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(ParameterSetName="ComputerName"), ValidateRange(1, 0xffff)]
        public virtual int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        protected string[] ResolvedComputerNames
        {
            get
            {
                return this.resolvedComputerNames;
            }
            set
            {
                this.resolvedComputerNames = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="Session")]
        public virtual PSSession[] Session
        {
            get
            {
                return this.remoteRunspaceInfos;
            }
            set
            {
                this.remoteRunspaceInfos = value;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Uri"), ValidateNotNull]
        public virtual PSSessionOption SessionOption
        {
            get
            {
                if (this.sessionOption == null)
                {
                    object valueToConvert = base.SessionState.PSVariable.GetValue("PSSessionOption");
                    if ((valueToConvert == null) || !LanguagePrimitives.TryConvertTo<PSSessionOption>(valueToConvert, out this.sessionOption))
                    {
                        this.sessionOption = new PSSessionOption();
                    }
                }
                return this.sessionOption;
            }
            set
            {
                this.sessionOption = value;
            }
        }

        [Parameter(ParameterSetName="Session"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="Uri")]
        public virtual int ThrottleLimit
        {
            get
            {
                return this.throttleLimit;
            }
            set
            {
                this.throttleLimit = value;
            }
        }

        [Parameter(ParameterSetName="ComputerName")]
        public virtual SwitchParameter UseSSL
        {
            get
            {
                return this.useSSL;
            }
            set
            {
                this.useSSL = value;
            }
        }
    }
}

