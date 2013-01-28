namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Tracing;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    public sealed class WSManConnectionInfo : RunspaceConnectionInfo
    {
        private bool _allowImplicitCredForNegotiate;
        private string _appName;
        private System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism _authMechanism;
        private string _computerName;
        private Uri _connectionUri;
        private PSCredential _credential;
        private bool _enableNetworkAccess;
        private bool _includePortInSPN;
        private int? _maxRecvdDataSizePerCommand;
        private int? _maxRecvdObjectSize;
        private int _maxUriRedirectionCount;
        private bool _noEncryption;
        private bool _noMachineProfile;
        private System.Management.Automation.Runspaces.OutputBufferingMode _outputBufferingMode;
        private int _port;
        private System.Management.Automation.Remoting.ProxyAccessType _proxyAcessType;
        private System.Management.Automation.Runspaces.AuthenticationMechanism _proxyAuthentication;
        private PSCredential _proxyCredential;
        private string _scheme;
        private string _shellUri;
        private bool _skipCaCheck;
        private bool _skipCnCheck;
        private bool _skipRevocationCheck;
        private string _thumbPrint;
        private bool _useCompression;
        private bool _useDefaultWSManPort;
        private bool _useUtf16;
        private static readonly string DefaultAppName = "/wsman";
        private const string DefaultComputerName = "localhost";
        private const PSCredential DefaultCredential = null;
        private const string DefaultM3PEndpoint = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell.Workflow";
        private const string DefaultM3PShellName = "Microsoft.PowerShell.Workflow";
        internal const int defaultMaximumConnectionRedirectionCount = 5;
        internal const System.Management.Automation.Runspaces.OutputBufferingMode DefaultOutputBufferingMode = System.Management.Automation.Runspaces.OutputBufferingMode.None;
        private const int DefaultPort = 0;
        private const int DefaultPortHttp = 80;
        private const int DefaultPortHttps = 0x1bb;
        private const string DefaultScheme = "http";
        private const string DefaultShellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
        private const string DefaultSslScheme = "https";
        public const string HttpScheme = "http";
        public const string HttpsScheme = "https";
        private const string LocalHostUriString = "http://localhost/wsman";
        private const int MaxPort = 0xffff;
        private const int MinPort = 0;

        public WSManConnectionInfo()
        {
            this._scheme = "http";
            this._computerName = "localhost";
            this._appName = DefaultAppName;
            this._port = -1;
            this._connectionUri = new Uri("http://localhost/wsman");
            this._shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
            this._useCompression = true;
            this.UseDefaultWSManPort = true;
        }

        public WSManConnectionInfo(PSSessionType configurationType) : this()
        {
            this.ComputerName = string.Empty;
            switch (configurationType)
            {
                case PSSessionType.DefaultRemoteShell:
                    break;

                case PSSessionType.Workflow:
                    this.ShellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell.Workflow";
                    break;

                default:
                    return;
            }
        }

        public WSManConnectionInfo(Uri uri) : this(uri, "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", (PSCredential) null)
        {
        }

        public WSManConnectionInfo(Uri uri, string shellUri, PSCredential credential)
		{
            this._scheme = "http";
            this._computerName = "localhost";
            this._appName = DefaultAppName;
            this._port = -1;
            this._connectionUri = new Uri("http://localhost/wsman");
            this._shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
            this._useCompression = true;
            if (uri == null)
            {
                this.ShellUri = shellUri;
                this.Credential = credential;
                this.UseDefaultWSManPort = true;
            }
            else
            {
                if (!uri.IsAbsoluteUri)
                {
                    throw new NotSupportedException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RelativeUriForRunspacePathNotSupported, new object[0]));
                }
                if ((uri.AbsolutePath.Equals("/", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(uri.Query)) && string.IsNullOrEmpty(uri.Fragment))
                {
                    this.ConstructUri(uri.Scheme, uri.Host, new int?(uri.Port), DefaultAppName);
                }
                else
                {
                    this.ConnectionUri = uri;
                }
                this.ShellUri = shellUri;
                this.Credential = credential;
            }
        }

        public WSManConnectionInfo(Uri uri, string shellUri, string certificateThumbprint) : this(uri, shellUri, (PSCredential) null)
        {
            this._thumbPrint = certificateThumbprint;
        }

        public WSManConnectionInfo(bool useSsl, string computerName, int port, string appName, string shellUri, PSCredential credential) : this(useSsl ? "https" : "http", computerName, port, appName, shellUri, credential)
        {
        }

        public WSManConnectionInfo(string scheme, string computerName, int port, string appName, string shellUri, PSCredential credential) : this(scheme, computerName, port, appName, shellUri, credential, 0x2bf20)
        {
        }

        public WSManConnectionInfo(bool useSsl, string computerName, int port, string appName, string shellUri, PSCredential credential, int openTimeout) : this(useSsl ? "https" : "http", computerName, port, appName, shellUri, credential, openTimeout)
        {
        }

        public WSManConnectionInfo(string scheme, string computerName, int port, string appName, string shellUri, PSCredential credential, int openTimeout)
        {
            this._scheme = "http";
            this._computerName = "localhost";
            this._appName = DefaultAppName;
            this._port = -1;
            this._connectionUri = new Uri("http://localhost/wsman");
            this._shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
            this._useCompression = true;
            this.Scheme = scheme;
            this.ComputerName = computerName;
            this.Port = port;
            this.AppName = appName;
            this.ShellUri = shellUri;
            this.Credential = credential;
            base.OpenTimeout = openTimeout;
        }

        internal void ConstructUri(string scheme, string computerName, int? port, string appName)
        {
            this._scheme = scheme;
            if (string.IsNullOrEmpty(this._scheme))
            {
                this._scheme = "http";
            }
            if ((!this._scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && !this._scheme.Equals("https", StringComparison.OrdinalIgnoreCase)) && !this._scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
            {
                ArgumentException exception = new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.InvalidSchemeValue, new object[] { this._scheme }));
                throw exception;
            }
            if (string.IsNullOrEmpty(computerName) || string.Equals(computerName, ".", StringComparison.OrdinalIgnoreCase))
            {
                this._computerName = "localhost";
            }
            else
            {
                this._computerName = computerName.Trim();
                IPAddress address = null;
                if ((IPAddress.TryParse(this._computerName, out address) && (address.AddressFamily == AddressFamily.InterNetworkV6)) && ((this._computerName.Length == 0) || (this._computerName[0] != '[')))
                {
                    this._computerName = "[" + this._computerName + "]";
                }
            }
            PSEtwLog.LogAnalyticVerbose(PSEventId.ComputerName, PSOpcode.Method, PSTask.CreateRunspace, PSKeyword.Runspace | PSKeyword.UseAlwaysAnalytic, new object[] { this._computerName });
            if (port.HasValue)
            {
                if (port.Value == 0)
                {
                    this._port = -1;
                    this.UseDefaultWSManPort = true;
                }
                else if ((port.Value == 80) || (port.Value == 0x1bb))
                {
                    this._port = port.Value;
                    this.UseDefaultWSManPort = false;
                }
                else
                {
                    if ((port.Value < 0) || (port.Value > 0xffff))
                    {
                        ArgumentException exception2 = new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.PortIsOutOfRange, new object[] { port }));
                        throw exception2;
                    }
                    this._port = port.Value;
                    this.UseDefaultWSManPort = false;
                }
            }
            this._appName = appName;
            if (string.IsNullOrEmpty(this._appName))
            {
                this._appName = DefaultAppName;
            }
            UriBuilder builder = new UriBuilder(this._scheme, this._computerName, this._port, this._appName);
            this._connectionUri = builder.Uri;
        }

        public WSManConnectionInfo Copy()
        {
            var w = new WSManConnectionInfo { 
                _connectionUri = this._connectionUri, _computerName = this._computerName, _scheme = this._scheme, _port = this._port, _appName = this._appName, _shellUri = this._shellUri, _credential = this._credential, UseDefaultWSManPort = this.UseDefaultWSManPort, _authMechanism = this._authMechanism, _maxUriRedirectionCount = this._maxUriRedirectionCount, _maxRecvdDataSizePerCommand = this._maxRecvdDataSizePerCommand, _maxRecvdObjectSize = this._maxRecvdObjectSize, OpenTimeout = base.OpenTimeout, IdleTimeout = base.IdleTimeout, MaxIdleTimeout = base.MaxIdleTimeout, CancelTimeout = base.CancelTimeout, 
                OperationTimeout = base.OperationTimeout, Culture = base.Culture, UICulture = base.UICulture, _thumbPrint = this._thumbPrint, _allowImplicitCredForNegotiate = this._allowImplicitCredForNegotiate, UseCompression = this._useCompression, NoMachineProfile = this._noMachineProfile, _proxyAcessType = this.ProxyAccessType, _proxyAuthentication = this.ProxyAuthentication, _proxyCredential = this.ProxyCredential, _skipCaCheck = this.SkipCACheck, _skipCnCheck = this.SkipCNCheck, _skipRevocationCheck = this.SkipRevocationCheck, _noEncryption = this.NoEncryption, _useUtf16 = this.UseUTF16, _includePortInSPN = this.IncludePortInSPN, 
                _enableNetworkAccess = this.EnableNetworkAccess, _outputBufferingMode = this._outputBufferingMode
             };
            w.UseDefaultWSManPort = this.UseDefaultWSManPort;
            return w;
        }

        internal static T ExtractPropertyAsWsManConnectionInfo<T>(RunspaceConnectionInfo rsCI, string property, T defaultValue)
        {
            WSManConnectionInfo info = rsCI as WSManConnectionInfo;
            if (info == null)
            {
                return defaultValue;
            }
            return (T) typeof(WSManConnectionInfo).GetProperty(property, typeof(T)).GetValue(info, null);
        }

        internal static string GetConnectionString(Uri connectionUri, out bool isSSLSpecified)
        {
            isSSLSpecified = connectionUri.Scheme.Equals("https");
            string str = connectionUri.OriginalString.TrimStart(new char[0]);
            if (isSSLSpecified)
            {
                return str.Substring("https".Length + 3);
            }
            return str.Substring("http".Length + 3);
        }

        private string ResolveShellUri(string shell)
        {
            string str = shell;
            if (string.IsNullOrEmpty(str))
            {
                str = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
            }
            if (str.IndexOf("http://schemas.microsoft.com/powershell/", StringComparison.OrdinalIgnoreCase) == -1)
            {
                str = "http://schemas.microsoft.com/powershell/" + str;
            }
            return str;
        }

        internal void SetConnectionUri(Uri newUri)
        {
            this._connectionUri = newUri;
        }

        public override void SetSessionOptions(PSSessionOption options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if ((options.ProxyAccessType == System.Management.Automation.Remoting.ProxyAccessType.None) && (options.ProxyCredential != null))
            {
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ProxyCredentialWithoutAccess, new object[] { System.Management.Automation.Remoting.ProxyAccessType.None }));
            }
            base.SetSessionOptions(options);
            this.MaximumConnectionRedirectionCount = (options.MaximumConnectionRedirectionCount >= 0) ? options.MaximumConnectionRedirectionCount : 0x7fffffff;
            this.MaximumReceivedDataSizePerCommand = options.MaximumReceivedDataSizePerCommand;
            this.MaximumReceivedObjectSize = options.MaximumReceivedObjectSize;
            this.UseCompression = !options.NoCompression;
            this.NoMachineProfile = options.NoMachineProfile;
            this._proxyAcessType = options.ProxyAccessType;
            this._proxyAuthentication = options.ProxyAuthentication;
            this._proxyCredential = options.ProxyCredential;
            this._skipCaCheck = options.SkipCACheck;
            this._skipCnCheck = options.SkipCNCheck;
            this._skipRevocationCheck = options.SkipRevocationCheck;
            this._noEncryption = options.NoEncryption;
            this._useUtf16 = options.UseUTF16;
            this._includePortInSPN = options.IncludePortInSPN;
            this._outputBufferingMode = options.OutputBufferingMode;
        }

        private void UpdateUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new NotSupportedException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RelativeUriForRunspacePathNotSupported, new object[0]));
            }
            if (uri.OriginalString.LastIndexOf(":", StringComparison.OrdinalIgnoreCase) > uri.AbsoluteUri.IndexOf("//", StringComparison.OrdinalIgnoreCase))
            {
                this._useDefaultWSManPort = false;
            }
            if ((uri.AbsolutePath.Equals("/", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(uri.Query)) && string.IsNullOrEmpty(uri.Fragment))
            {
                string defaultAppName = DefaultAppName;
                this.ConstructUri(uri.Scheme, uri.Host, new int?(uri.Port), defaultAppName);
            }
            else
            {
                this._connectionUri = uri;
                this._scheme = uri.Scheme;
                this._appName = uri.AbsolutePath;
                this._port = uri.Port;
                this._computerName = uri.Host;
                this._useDefaultWSManPort = false;
            }
        }

        private void ValidateSpecifiedAuthentication()
        {
            if ((this._authMechanism != System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION) && (this._thumbPrint != null))
            {
                throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", PSRemotingErrorId.NewRunspaceAmbiguosAuthentication.ToString(), new object[] { "CertificateThumbPrint", this.AuthenticationMechanism.ToString() });
            }
        }

        internal bool AllowImplicitCredentialForNegotiate
        {
            get
            {
                return this._allowImplicitCredForNegotiate;
            }
        }

        public string AppName
        {
            get
            {
                return this._appName;
            }
            set
            {
                this.ConstructUri(this._scheme, this._computerName, null, value);
            }
        }

        public override System.Management.Automation.Runspaces.AuthenticationMechanism AuthenticationMechanism
        {
            get
            {
                switch (this._authMechanism)
                {
                    case System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION:
                        return System.Management.Automation.Runspaces.AuthenticationMechanism.Default;

                    case System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST:
                        return System.Management.Automation.Runspaces.AuthenticationMechanism.Digest;

                    case System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE:
                        if (!this._allowImplicitCredForNegotiate)
                        {
                            return System.Management.Automation.Runspaces.AuthenticationMechanism.Negotiate;
                        }
                        return System.Management.Automation.Runspaces.AuthenticationMechanism.NegotiateWithImplicitCredential;

                    case System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC:
                        return System.Management.Automation.Runspaces.AuthenticationMechanism.Basic;

                    case System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_KERBEROS:
                        return System.Management.Automation.Runspaces.AuthenticationMechanism.Kerberos;

                    case System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CREDSSP:
                        return System.Management.Automation.Runspaces.AuthenticationMechanism.Credssp;
                }
                return System.Management.Automation.Runspaces.AuthenticationMechanism.Default;
            }
            set
            {
                switch (value)
                {
                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Default:
                        this._authMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION;
                        break;

                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Basic:
                        this._authMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_BASIC;
                        break;

                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Negotiate:
                        this._authMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
                        break;

                    case System.Management.Automation.Runspaces.AuthenticationMechanism.NegotiateWithImplicitCredential:
                        this._authMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_NEGOTIATE;
                        this._allowImplicitCredForNegotiate = true;
                        break;

                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Credssp:
                        this._authMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CREDSSP;
                        break;

                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Digest:
                        this._authMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_DIGEST;
                        break;

                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Kerberos:
                        this._authMechanism = System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_KERBEROS;
                        break;

                    default:
                        throw new PSNotSupportedException();
                }
                this.ValidateSpecifiedAuthentication();
            }
        }

        public override string CertificateThumbprint
        {
            get
            {
                return this._thumbPrint;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this._thumbPrint = value;
            }
        }

        public override string ComputerName
        {
            get
            {
                return this._computerName;
            }
            set
            {
                this.ConstructUri(this._scheme, value, null, this._appName);
            }
        }

        public Uri ConnectionUri
        {
            get
            {
                return this._connectionUri;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this.UpdateUri(value);
            }
        }

        public override PSCredential Credential
        {
            get
            {
                return this._credential;
            }
            set
            {
                this._credential = value;
            }
        }

        public bool EnableNetworkAccess
        {
            get
            {
                return this._enableNetworkAccess;
            }
            set
            {
                this._enableNetworkAccess = value;
            }
        }

        public bool IncludePortInSPN
        {
            get
            {
                return this._includePortInSPN;
            }
            set
            {
                this._includePortInSPN = value;
            }
        }

        internal bool IsLocalhostAndNetworkAccess
        {
            get
            {
                if (!this.EnableNetworkAccess)
                {
                    return false;
                }
                if (this.Credential != null)
                {
                    return false;
                }
                if (!this.ComputerName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    return (this.ComputerName.IndexOf('.') == -1);
                }
                return true;
            }
        }

        public int MaximumConnectionRedirectionCount
        {
            get
            {
                return this._maxUriRedirectionCount;
            }
            set
            {
                this._maxUriRedirectionCount = value;
            }
        }

        public int? MaximumReceivedDataSizePerCommand
        {
            get
            {
                return this._maxRecvdDataSizePerCommand;
            }
            set
            {
                this._maxRecvdDataSizePerCommand = value;
            }
        }

        public int? MaximumReceivedObjectSize
        {
            get
            {
                return this._maxRecvdObjectSize;
            }
            set
            {
                this._maxRecvdObjectSize = value;
            }
        }

        public bool NoEncryption
        {
            get
            {
                return this._noEncryption;
            }
            set
            {
                this._noEncryption = value;
            }
        }

        public bool NoMachineProfile
        {
            get
            {
                return this._noMachineProfile;
            }
            set
            {
                this._noMachineProfile = value;
            }
        }

        public System.Management.Automation.Runspaces.OutputBufferingMode OutputBufferingMode
        {
            get
            {
                return this._outputBufferingMode;
            }
            set
            {
                this._outputBufferingMode = value;
            }
        }

        public int Port
        {
            get
            {
                return this.ConnectionUri.Port;
            }
            set
            {
                this.ConstructUri(this._scheme, this._computerName, new int?(value), this._appName);
            }
        }

        internal int PortSetting
        {
            get
            {
                return this._port;
            }
        }

        public System.Management.Automation.Remoting.ProxyAccessType ProxyAccessType
        {
            get
            {
                return this._proxyAcessType;
            }
            set
            {
                this._proxyAcessType = value;
            }
        }

        public System.Management.Automation.Runspaces.AuthenticationMechanism ProxyAuthentication
        {
            get
            {
                return this._proxyAuthentication;
            }
            set
            {
                switch (value)
                {
                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Basic:
                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Negotiate:
                    case System.Management.Automation.Runspaces.AuthenticationMechanism.Digest:
                        this._proxyAuthentication = value;
                        return;
                }
                throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ProxyAmbiguosAuthentication, new object[] { value, System.Management.Automation.Runspaces.AuthenticationMechanism.Basic.ToString(), System.Management.Automation.Runspaces.AuthenticationMechanism.Negotiate.ToString(), System.Management.Automation.Runspaces.AuthenticationMechanism.Digest.ToString() }));
            }
        }

        public PSCredential ProxyCredential
        {
            get
            {
                return this._proxyCredential;
            }
            set
            {
                if (this._proxyAcessType == System.Management.Automation.Remoting.ProxyAccessType.None)
                {
                    throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.ProxyCredentialWithoutAccess, new object[] { System.Management.Automation.Remoting.ProxyAccessType.None }));
                }
                this._proxyCredential = value;
            }
        }

        public string Scheme
        {
            get
            {
                return this._scheme;
            }
            set
            {
                this.ConstructUri(value, this._computerName, null, this._appName);
            }
        }

        public string ShellUri
        {
            get
            {
                return this._shellUri;
            }
            set
            {
                this._shellUri = this.ResolveShellUri(value);
            }
        }

        public bool SkipCACheck
        {
            get
            {
                return this._skipCaCheck;
            }
            set
            {
                this._skipCaCheck = value;
            }
        }

        public bool SkipCNCheck
        {
            get
            {
                return this._skipCnCheck;
            }
            set
            {
                this._skipCnCheck = value;
            }
        }

        public bool SkipRevocationCheck
        {
            get
            {
                return this._skipRevocationCheck;
            }
            set
            {
                this._skipRevocationCheck = value;
            }
        }

        public bool UseCompression
        {
            get
            {
                return this._useCompression;
            }
            set
            {
                this._useCompression = value;
            }
        }

        internal bool UseDefaultWSManPort
        {
            get
            {
                return this._useDefaultWSManPort;
            }
            set
            {
                this._useDefaultWSManPort = value;
            }
        }

        public bool UseUTF16
        {
            get
            {
                return this._useUtf16;
            }
            set
            {
                this._useUtf16 = value;
            }
        }

        internal System.Management.Automation.Remoting.Client.WSManNativeApi.WSManAuthenticationMechanism WSManAuthenticationMechanism
        {
            get
            {
                return this._authMechanism;
            }
        }

		public override PSObject ToPSObjectForRemoting ()
		{
			var obj = RemotingEncoder.CreateEmptyPSObject ();
			AddPSNoteProperty(obj, this._allowImplicitCredForNegotiate, "AllowImplicitCredForNegotiate");
			AddPSNoteProperty(obj, this._appName, "ApplicationName");
			AddPSNoteProperty(obj, this._authMechanism, "AuthenticationMechanism");
			AddPSNoteProperty(obj, this._computerName, "ComputerName");
			AddPSNoteProperty(obj, this._connectionUri, "ConnectionUri");
			AddPSNoteProperty(obj, this._credential, "Credential");
			AddPSNoteProperty(obj, this._maxRecvdDataSizePerCommand, "MaxRecvdDataSizePerCommand");
			AddPSNoteProperty(obj, this._enableNetworkAccess, "EnableNetworkAccess");
			AddPSNoteProperty(obj, this._includePortInSPN, "IncludePortInSPN");
			AddPSNoteProperty(obj, this._maxRecvdObjectSize, "MaxRecvdObjectSize");
            AddPSNoteProperty(obj, this._maxUriRedirectionCount,"MaxUriRedirectionCount");
            AddPSNoteProperty(obj, this._noEncryption, "NoEncryption");
            AddPSNoteProperty(obj, this._noMachineProfile, "NoMachineProfile");
            AddPSNoteProperty(obj, this._outputBufferingMode, "OutputBufferingMode");
            AddPSNoteProperty(obj, this._port, "Port");
            AddPSNoteProperty(obj, this._proxyAcessType, "ProxyAccessType");
            AddPSNoteProperty(obj, this._proxyAuthentication, "ProxyAuthenticationMechanism");
            AddPSNoteProperty(obj, this._proxyCredential, "ProxyCredential");
            AddPSNoteProperty(obj, this._thumbPrint, "ThumbPrint");
            AddPSNoteProperty(obj, this._scheme, "Scheme");
            AddPSNoteProperty(obj, this._shellUri, "ShellUri");
            AddPSNoteProperty(obj, this._skipCaCheck, "SkipCaCheck");
            AddPSNoteProperty(obj, this._skipCnCheck, "SkipCnCheck");
            AddPSNoteProperty(obj, this._skipRevocationCheck, "SkipRevocationCheck");
            AddPSNoteProperty(obj, this._useCompression, "UseCompression");
            AddPSNoteProperty(obj, this._useDefaultWSManPort, "UseDefaultWSManPort");
            AddPSNoteProperty(obj, this._useUtf16 ,  "UseUtf16");
			AddPSNoteProperty(obj, this.IdleTimeout ,  "IdleTimeout");
			AddPSNoteProperty(obj, this.MaxIdleTimeout ,  "MaxIdleTimeout");
			AddPSNoteProperty(obj, this.CancelTimeout ,  "CancelTimeout");
			AddPSNoteProperty(obj, this.OpenTimeout ,  "OpenTimeout");
			AddPSNoteProperty(obj, this.OperationTimeout ,  "OperationTimeout");
			AddPSNoteProperty(obj, this.Culture.Name ,  "Culture");
			AddPSNoteProperty(obj, this.UICulture.Name ,  "UICulture");
			return obj;
		}

		private void AddPSNoteProperty (PSObject obj, object value, string name)
		{
			obj.AddOrSetProperty (new PSNoteProperty(name, value));
		}

		public static RunspaceConnectionInfo FromPSObjectForRemoting (PSObject obj)
		{
			var connection = new WSManConnectionInfo(PSSessionType.DefaultRemoteShell);
			connection._allowImplicitCredForNegotiate = RemotingDecoder.GetPropertyValue<bool>(obj, "AllowImplicitCredForNegotiate");
			connection._appName = RemotingDecoder.GetPropertyValue<string>(obj, "ApplicationName");
			connection._authMechanism = RemotingDecoder.GetPropertyValue<WSManNativeApi.WSManAuthenticationMechanism>(obj, "AuthenticationMechanism");
			connection._computerName =  RemotingDecoder.GetPropertyValue<string>(obj, "ComputerName");
			connection._connectionUri =  RemotingDecoder.GetPropertyValue<Uri>(obj, "ConnectionUri");
			connection._credential =  RemotingDecoder.GetPropertyValue<PSCredential>(obj, "Credential");
			connection._maxRecvdDataSizePerCommand = RemotingDecoder.GetPropertyValue<int?>(obj, "MaxRecvdDataSizePerCommand");
			connection._enableNetworkAccess = RemotingDecoder.GetPropertyValue<bool>(obj, "EnableNetworkAccess");
			connection._includePortInSPN = RemotingDecoder.GetPropertyValue<bool>(obj, "IncludePortInSPN");
			connection._maxRecvdObjectSize = RemotingDecoder.GetPropertyValue<int?>(obj, "MaxRecvdObjectSize");
			connection._maxUriRedirectionCount = RemotingDecoder.GetPropertyValue<int>(obj, "MaxUriRedirectionCount");
			connection._noEncryption = RemotingDecoder.GetPropertyValue<bool>(obj, "NoEncryption");
			connection._noMachineProfile = RemotingDecoder.GetPropertyValue<bool>(obj, "NoMachineProfile");
			connection._outputBufferingMode = RemotingDecoder.GetPropertyValue<OutputBufferingMode>(obj, "OutputBufferingMode");
			connection._port = RemotingDecoder.GetPropertyValue<int>(obj, "Port");
			connection._proxyAcessType = RemotingDecoder.GetPropertyValue<ProxyAccessType>(obj, "ProxyAccessType");
			connection._proxyAuthentication = RemotingDecoder.GetPropertyValue<AuthenticationMechanism>(obj, "ProxyAuthenticationMechanism");
			connection._proxyCredential =  RemotingDecoder.GetPropertyValue<PSCredential>(obj, "ProxyCredential");
			connection._thumbPrint = RemotingDecoder.GetPropertyValue<string>(obj, "ThumbPrint");
			connection._scheme = RemotingDecoder.GetPropertyValue<string>(obj, "Scheme");
			connection._shellUri = RemotingDecoder.GetPropertyValue<string>(obj, "ShellUri");
			connection._skipCaCheck = RemotingDecoder.GetPropertyValue<bool>(obj, "SkipCaCheck");
			connection._skipCnCheck = RemotingDecoder.GetPropertyValue<bool>(obj, "SkipCnCheck");
			connection._skipRevocationCheck = RemotingDecoder.GetPropertyValue<bool>(obj, "SkipRevocationCheck");
			connection._useCompression = RemotingDecoder.GetPropertyValue<bool>(obj, "UseCompression");
			connection._useDefaultWSManPort = RemotingDecoder.GetPropertyValue<bool>(obj, "UseDefaultWSManPort");
			connection._useUtf16 = RemotingDecoder.GetPropertyValue<bool>(obj, "UseUtf16");
			connection.IdleTimeout = RemotingDecoder.GetPropertyValue<int>(obj, "IdleTimeout");
			connection.MaxIdleTimeout = RemotingDecoder.GetPropertyValue<int>(obj, "MaxIdleTimeout");
			connection.CancelTimeout = RemotingDecoder.GetPropertyValue<int>(obj, "CancelTimeout");
			connection.OpenTimeout = RemotingDecoder.GetPropertyValue<int>(obj, "OpenTimeout");
			connection.OperationTimeout = RemotingDecoder.GetPropertyValue<int>(obj, "OperationTimeout");
			connection.Culture = new System.Globalization.CultureInfo(RemotingDecoder.GetPropertyValue<string>(obj, "Culture"));
			connection.UICulture = new System.Globalization.CultureInfo(RemotingDecoder.GetPropertyValue<string>(obj, "UICulture"));
			return connection;
		}
    }
}

