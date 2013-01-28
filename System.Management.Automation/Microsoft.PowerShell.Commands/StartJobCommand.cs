namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Security;

    [OutputType(new System.Type[] { typeof(PSRemotingJob) }), Cmdlet("Start", "Job", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113405")]
    public class StartJobCommand : PSExecutionCmdlet, IDisposable
    {
        private string _definitionName;
        private string _definitionPath;
        private string _definitionType;
        private const string DefinitionNameParameterSet = "DefinitionName";
        private bool firstProcessRecord = true;
        private System.Management.Automation.ScriptBlock initScript;
        private string name;
        private Version psVersion;
        private bool shouldRunAs32;
        private static readonly string StartJobType = "BackgroundJob";

        protected override void BeginProcessing()
        {
            if (base.ParameterSetName != "DefinitionName")
            {
                base.SkipWinRMCheck = true;
                base.BeginProcessing();
            }
        }

        protected override void CreateHelpersForSpecifiedComputerNames()
        {
            if (((base.Context.LanguageMode == PSLanguageMode.ConstrainedLanguage) && (SystemPolicy.GetSystemLockdownPolicy() != SystemEnforcementMode.Enforce)) && ((this.ScriptBlock != null) || (this.InitializationScript != null)))
            {
                base.ThrowTerminatingError(new ErrorRecord(new PSNotSupportedException(RemotingErrorIdStrings.CannotStartJobInconsistentLanguageMode), "CannotStartJobInconsistentLanguageMode", ErrorCategory.PermissionDenied, base.Context.LanguageMode));
            }
            NewProcessConnectionInfo connectionInfo = new NewProcessConnectionInfo(this.Credential) {
                RunAs32 = this.shouldRunAs32,
                InitializationScript = this.initScript,
                AuthenticationMechanism = this.Authentication,
                PSVersion = this.PSVersion
            };
            RemoteRunspace remoteRunspace = (RemoteRunspace) RunspaceFactory.CreateRunspace(connectionInfo, base.Host, Utils.GetTypeTableFromExecutionContextTLS());
            remoteRunspace.Events.ReceivedEvents.PSEventReceived += new PSEventReceivedEventHandler(this.OnRunspacePSEventReceived);
            Pipeline pipeline = base.CreatePipeline(remoteRunspace);
            IThrottleOperation item = new ExecutionCmdletHelperComputerName(remoteRunspace, pipeline, false);
            base.Operations.Add(item);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.CloseAllInputStreams();
            }
        }

        protected override void EndProcessing()
        {
            base.CloseAllInputStreams();
        }

        protected override void ProcessRecord()
        {
            if (base.ParameterSetName == "DefinitionName")
            {
                string definitionPath = null;
                if (!string.IsNullOrEmpty(this._definitionPath))
                {
                    ProviderInfo provider = null;
                    Collection<string> resolvedProviderPathFromPSPath = base.Context.SessionState.Path.GetResolvedProviderPathFromPSPath(this._definitionPath, out provider);
                    if (!provider.NameEquals(base.Context.ProviderNames.FileSystem))
                    {
                        string message = StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionPathInvalidNotFSProvider, new object[] { this._definitionName, this._definitionPath, provider.FullName });
                        base.WriteError(new ErrorRecord(new RuntimeException(message), "StartJobFromDefinitionNamePathInvalidNotFileSystemProvider", ErrorCategory.InvalidArgument, null));
                        return;
                    }
                    if (resolvedProviderPathFromPSPath.Count != 1)
                    {
                        string str3 = StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionPathInvalidNotSingle, this._definitionName, this._definitionPath);
                        base.WriteError(new ErrorRecord(new RuntimeException(str3), "StartJobFromDefinitionNamePathInvalidNotSingle", ErrorCategory.InvalidArgument, null));
                        return;
                    }
                    definitionPath = resolvedProviderPathFromPSPath[0];
                }
                List<Job2> list = base.JobManager.GetJobToStart(this._definitionName, definitionPath, this._definitionType, this, false);
                if (list.Count == 0)
                {
                    string str4 = (this._definitionType != null) ? StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionNotFound2, this._definitionType, this._definitionName) : StringUtil.Format(RemotingErrorIdStrings.StartJobDefinitionNotFound1, this._definitionName);
                    base.WriteError(new ErrorRecord(new RuntimeException(str4), "StartJobFromDefinitionNameNotFound", ErrorCategory.ObjectNotFound, null));
                }
                else if (list.Count > 1)
                {
                    string str5 = StringUtil.Format(RemotingErrorIdStrings.StartJobManyDefNameMatches, this._definitionName);
                    base.WriteError(new ErrorRecord(new RuntimeException(str5), "StartJobFromDefinitionNameMoreThanOneMatch", ErrorCategory.InvalidResult, null));
                }
                else
                {
                    Job2 sendToPipeline = list[0];
                    sendToPipeline.StartJob();
                    base.WriteObject(sendToPipeline);
                }
            }
            else
            {
                if (this.firstProcessRecord)
                {
                    this.firstProcessRecord = false;
                    PSRemotingJob item = new PSRemotingJob(base.ResolvedComputerNames, base.Operations, this.ScriptBlock.ToString(), this.ThrottleLimit, this.name) {
                        PSJobTypeName = StartJobType
                    };
                    base.JobRepository.Add(item);
                    base.WriteObject(item);
                }
                if (this.InputObject != AutomationNull.Value)
                {
                    foreach (IThrottleOperation operation in base.Operations)
                    {
                        ExecutionCmdletHelper helper = (ExecutionCmdletHelper) operation;
                        helper.Pipeline.Input.Write(this.InputObject);
                    }
                }
            }
        }

        public override SwitchParameter AllowRedirection
        {
            get
            {
                return false;
            }
        }

        public override string ApplicationName
        {
            get
            {
                return null;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Alias(new string[] { "Args" }), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="LiteralFilePathComputerName")]
        public override object[] ArgumentList
        {
            get
            {
                return base.ArgumentList;
            }
            set
            {
                base.ArgumentList = value;
            }
        }

        [Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="LiteralFilePathComputerName")]
        public override AuthenticationMechanism Authentication
        {
            get
            {
                return base.Authentication;
            }
            set
            {
                base.Authentication = value;
            }
        }

        public override string CertificateThumbprint
        {
            get
            {
                return base.CertificateThumbprint;
            }
            set
            {
                base.CertificateThumbprint = value;
            }
        }

        public override string[] ComputerName
        {
            get
            {
                return null;
            }
        }

        public override string ConfigurationName
        {
            get
            {
                return base.ConfigurationName;
            }
            set
            {
                base.ConfigurationName = value;
            }
        }

        public override Uri[] ConnectionUri
        {
            get
            {
                return null;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Credential, Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="LiteralFilePathComputerName")]
        public override PSCredential Credential
        {
            get
            {
                return base.Credential;
            }
            set
            {
                base.Credential = value;
            }
        }

        [Parameter(Position=0, Mandatory=true, ParameterSetName="DefinitionName"), ValidateNotNullOrEmpty]
        public string DefinitionName
        {
            get
            {
                return this._definitionName;
            }
            set
            {
                this._definitionName = value;
            }
        }

        [Parameter(Position=1, ParameterSetName="DefinitionName"), ValidateNotNullOrEmpty]
        public string DefinitionPath
        {
            get
            {
                return this._definitionPath;
            }
            set
            {
                this._definitionPath = value;
            }
        }

        public override SwitchParameter EnableNetworkAccess
        {
            get
            {
                return false;
            }
        }

        [Parameter(Position=0, Mandatory=true, ParameterSetName="FilePathComputerName")]
        public override string FilePath
        {
            get
            {
                return base.FilePath;
            }
            set
            {
                base.FilePath = value;
            }
        }

        [Parameter(Position=1, ParameterSetName="ComputerName"), Parameter(Position=1, ParameterSetName="FilePathComputerName"), Parameter(Position=1, ParameterSetName="LiteralFilePathComputerName")]
        public virtual System.Management.Automation.ScriptBlock InitializationScript
        {
            get
            {
                return this.initScript;
            }
            set
            {
                this.initScript = value;
            }
        }

        [Parameter(ValueFromPipeline=true, ParameterSetName="LiteralFilePathComputerName"), Parameter(ValueFromPipeline=true, ParameterSetName="FilePathComputerName"), Parameter(ValueFromPipeline=true, ParameterSetName="ComputerName")]
        public override PSObject InputObject
        {
            get
            {
                return base.InputObject;
            }
            set
            {
                base.InputObject = value;
            }
        }

        [Parameter(Mandatory=true, ParameterSetName="LiteralFilePathComputerName"), Alias(new string[] { "PSPath" })]
        public string LiteralPath
        {
            get
            {
                return base.FilePath;
            }
            set
            {
                base.FilePath = value;
                base.IsLiteralPath = true;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="ComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="LiteralFilePathComputerName"), Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="FilePathComputerName")]
        public virtual string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.name = value;
                }
            }
        }

        public override int Port
        {
            get
            {
                return 0;
            }
        }

        [Parameter(ParameterSetName="LiteralFilePathComputerName"), Parameter(ParameterSetName="FilePathComputerName"), Parameter(ParameterSetName="ComputerName"), ValidateNotNullOrEmpty]
        public virtual Version PSVersion
        {
            get
            {
                return this.psVersion;
            }
            set
            {
                if ((((value != null) && (value.Major != 2)) && (value.Major != 3)) || ((value != null) && (value.Minor != 0)))
                {
                    throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.PSVersionParameterOutOfRange, new object[] { value, "PSVersion" }));
                }
                PSSessionConfigurationCommandUtilities.CheckIfPowerShellVersionIsInstalled(value);
                this.psVersion = value;
            }
        }

        [Parameter(ParameterSetName="ComputerName"), Parameter(ParameterSetName="LiteralFilePathComputerName"), Parameter(ParameterSetName="FilePathComputerName")]
        public virtual SwitchParameter RunAs32
        {
            get
            {
                return this.shouldRunAs32;
            }
            set
            {
                this.shouldRunAs32 = (bool) value;
            }
        }

        [Alias(new string[] { "Command" }), Parameter(Position=0, Mandatory=true, ParameterSetName="ComputerName")]
        public override System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return base.ScriptBlock;
            }
            set
            {
                base.ScriptBlock = value;
            }
        }

        public override PSSession[] Session
        {
            get
            {
                return null;
            }
        }

        public override PSSessionOption SessionOption
        {
            get
            {
                return base.SessionOption;
            }
            set
            {
                base.SessionOption = value;
            }
        }

        public override int ThrottleLimit
        {
            get
            {
                return 0;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=2, ParameterSetName="DefinitionName")]
        public string Type
        {
            get
            {
                return this._definitionType;
            }
            set
            {
                this._definitionType = value;
            }
        }

        public override SwitchParameter UseSSL
        {
            get
            {
                return false;
            }
        }
    }
}

