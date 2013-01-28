namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    [OutputType(new Type[] { typeof(PSSession) }), Cmdlet("Get", "PSSession", DefaultParameterSetName="Name", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135219", RemotingCapability=RemotingCapability.OwnedByCommand)]
    public class GetPSSessionCommand : PSRunspaceCmdlet, IDisposable
    {
        private bool allowRedirection;
        private string appName;
        private AuthenticationMechanism authentication;
        private const string ComputerInstanceIdParameterSet = "ComputerInstanceId";
        private string[] computerNames;
        private const string ConnectionUriInstanceIdParameterSet = "ConnectionUriInstanceId";
        private const string ConnectionUriParameterSet = "ConnectionUri";
        private SessionFilterState filterState;
        private int port;
        private PSCredential psCredential;
        private QueryRunspaces queryRunspaces = new QueryRunspaces();
        private PSSessionOption sessionOption;
        private string shell;
        private ObjectStream stream = new ObjectStream();
        private int throttleLimit;
        private string thumbprint;
        private Uri[] uris;
        private SwitchParameter useSSL;

        public void Dispose()
        {
            this.stream.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override void EndProcessing()
        {
            this.stream.ObjectWriter.Close();
        }

        private Collection<WSManConnectionInfo> GetConnectionObjects()
        {
            Collection<WSManConnectionInfo> collection = new Collection<WSManConnectionInfo>();
            if ((base.ParameterSetName == "ComputerName") || (base.ParameterSetName == "ComputerInstanceId"))
            {
                string str = this.UseSSL.IsPresent ? "https" : "http";
                foreach (string str2 in this.ComputerName)
                {
                    WSManConnectionInfo connectionInfo = new WSManConnectionInfo {
                        Scheme = str,
                        ComputerName = base.ResolveComputerName(str2),
                        AppName = this.ApplicationName,
                        ShellUri = this.ConfigurationName,
                        Port = this.Port
                    };
                    if (this.CertificateThumbprint != null)
                    {
                        connectionInfo.CertificateThumbprint = this.CertificateThumbprint;
                    }
                    else
                    {
                        connectionInfo.Credential = this.Credential;
                    }
                    connectionInfo.AuthenticationMechanism = this.Authentication;
                    this.UpdateConnectionInfo(connectionInfo);
                    collection.Add(connectionInfo);
                }
                return collection;
            }
            if ((base.ParameterSetName == "ConnectionUri") || (base.ParameterSetName == "ConnectionUriInstanceId"))
            {
                foreach (Uri uri in this.ConnectionUri)
                {
                    WSManConnectionInfo info2 = new WSManConnectionInfo {
                        ConnectionUri = uri,
                        ShellUri = this.ConfigurationName
                    };
                    if (this.CertificateThumbprint != null)
                    {
                        info2.CertificateThumbprint = this.CertificateThumbprint;
                    }
                    else
                    {
                        info2.Credential = this.Credential;
                    }
                    info2.AuthenticationMechanism = this.Authentication;
                    this.UpdateConnectionInfo(info2);
                    collection.Add(info2);
                }
            }
            return collection;
        }

        protected override void ProcessRecord()
        {
            if ((base.ParameterSetName == "Name") && ((this.Name == null) || (this.Name.Length == 0)))
            {
                base.GetAllRunspaces(true, true);
            }
            else if (((base.ParameterSetName == "ComputerName") || (base.ParameterSetName == "ComputerInstanceId")) || ((base.ParameterSetName == "ConnectionUri") || (base.ParameterSetName == "ConnectionUriInstanceId")))
            {
                this.QueryForRemoteSessions();
            }
            else
            {
                base.GetMatchingRunspaces(true, true);
            }
        }

        private void QueryForRemoteSessions()
        {
            Collection<WSManConnectionInfo> connectionObjects = this.GetConnectionObjects();
            Collection<PSSession> collection2 = this.queryRunspaces.GetDisconnectedSessions(connectionObjects, base.Host, this.stream, base.RunspaceRepository, this.throttleLimit, this.filterState, this.InstanceId, this.Name, this.ConfigurationName);
            foreach (object obj2 in this.stream.ObjectReader.NonBlockingRead())
            {
                if (base.IsStopping)
                {
                    break;
                }
                base.WriteStreamObject((Action<Cmdlet>) obj2);
            }
            foreach (PSSession session in collection2)
            {
                if (base.IsStopping)
                {
                    break;
                }
                base.WriteObject(session);
            }
        }

        protected override void StopProcessing()
        {
            this.queryRunspaces.StopAllOperations();
        }

        private void UpdateConnectionInfo(WSManConnectionInfo connectionInfo)
        {
            if ((base.ParameterSetName != "ConnectionUri") && (base.ParameterSetName != "ConnectionUriInstanceId"))
            {
                connectionInfo.MaximumConnectionRedirectionCount = 0;
            }
            if (!this.allowRedirection)
            {
                connectionInfo.MaximumConnectionRedirectionCount = 0;
            }
            if (this.SessionOption != null)
            {
                connectionInfo.SetSessionOptions(this.SessionOption);
            }
        }

        [Parameter(ParameterSetName="ConnectionUriInstanceId"), Parameter(ParameterSetName="ConnectionUri")]
        public SwitchParameter AllowRedirection
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

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerInstanceId"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName")]
        public string ApplicationName
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

        [Parameter(ParameterSetName="ConnectionUri"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public AuthenticationMechanism Authentication
        {
            get
            {
                return this.authentication;
            }
            set
            {
                this.authentication = value;
                PSRemotingBaseCmdlet.ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUri"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public string CertificateThumbprint
        {
            get
            {
                return this.thumbprint;
            }
            set
            {
                this.thumbprint = value;
                PSRemotingBaseCmdlet.ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerInstanceId"), ValidateNotNullOrEmpty, Alias(new string[] { "Cn" })]
        public override string[] ComputerName
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

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerInstanceId"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUri"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUriInstanceId")]
        public string ConfigurationName
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

        [Alias(new string[] { "URI", "CU" }), Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUriInstanceId"), ValidateNotNullOrEmpty, Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ConnectionUri")]
        public Uri[] ConnectionUri
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

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUri"), Parameter(ParameterSetName="ConnectionUriInstanceId"), Credential]
        public PSCredential Credential
        {
            get
            {
                return this.psCredential;
            }
            set
            {
                this.psCredential = value;
                PSRemotingBaseCmdlet.ValidateSpecifiedAuthentication(this.Credential, this.CertificateThumbprint, this.Authentication);
            }
        }

        [Parameter(ParameterSetName="ConnectionUriInstanceId", Mandatory=true), Parameter(ParameterSetName="ComputerInstanceId", Mandatory=true), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="InstanceId"), ValidateNotNull]
        public override Guid[] InstanceId
        {
            get
            {
                return base.InstanceId;
            }
            set
            {
                base.InstanceId = value;
            }
        }

        [Parameter(ParameterSetName="ConnectionUri"), Parameter(ParameterSetName="ComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="Name"), ValidateNotNullOrEmpty]
        public override string[] Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        [Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ComputerName"), ValidateRange(1, 0xffff)]
        public int Port
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

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUri"), Parameter(ParameterSetName="ConnectionUriInstanceId")]
        public PSSessionOption SessionOption
        {
            get
            {
                return this.sessionOption;
            }
            set
            {
                this.sessionOption = value;
            }
        }

        [Parameter(ParameterSetName="ConnectionUriInstanceId"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUri")]
        public SessionFilterState State
        {
            get
            {
                return this.filterState;
            }
            set
            {
                this.filterState = value;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="ConnectionUriInstanceId"), Parameter(ParameterSetName="ComputerInstanceId"), Parameter(ParameterSetName="ConnectionUri")]
        public int ThrottleLimit
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

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="ComputerInstanceId")]
        public SwitchParameter UseSSL
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

