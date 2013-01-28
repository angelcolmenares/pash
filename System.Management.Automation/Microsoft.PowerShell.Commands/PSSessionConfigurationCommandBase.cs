namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Security.AccessControl;
    using System.Threading;

    public class PSSessionConfigurationCommandBase : PSCmdlet
    {
        private PSSessionConfigurationAccessMode accessMode = PSSessionConfigurationAccessMode.Remote;
        internal bool accessModeSpecified;
        internal string applicationBase;
        internal string assemblyName;
        internal const string AssemblyNameParameterSetName = "AssemblyNameParameterSet";
        private string configPath;
        internal string configurationScript;
        internal string configurationTypeName;
        internal bool force;
        internal bool isApplicationBaseSpecified;
        internal bool isAssemblyNameSpecified;
        internal bool isConfigurationScriptSpecified;
        internal bool isConfigurationTypeNameSpecified;
        internal bool isMaxCommandSizeMBSpecified;
        internal bool isMaxObjectSizeMBSpecified;
        internal bool isPSVersionSpecified;
        internal bool isRunAsCredentialSpecified;
        internal bool isSddlSpecified;
        internal bool isUseSharedProcessSpecified;
        private const string localSDDL = "O:NSG:BAD:P(D;;GA;;;NU)(A;;GA;;;BA)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        private const string localSDDL_Win8 = "O:NSG:BAD:P(D;;GA;;;NU)(A;;GA;;;BA)(A;;GA;;;RM)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        internal double? maxCommandSizeMB;
        internal double? maxObjectSizeMB;
        internal Version MaxPSVersion;
        internal bool modulePathSpecified;
        internal string[] modulesToImport;
        internal const string NameParameterSetName = "NameParameterSet";
        internal bool noRestart;
        internal Version psVersion;
        internal const string RemoteManagementUsersSID = "S-1-5-32-580";
        private const string remoteSDDL = "O:NSG:BAD:P(A;;GA;;;BA)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        private const string remoteSDDL_Win8 = "O:NSG:BAD:P(A;;GA;;;BA)(A;;GA;;;RM)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        internal PSCredential runAsCredential;
        internal string sddl;
        internal const string SessionConfigurationFileParameterSetName = "SessionConfigurationFile";
        internal PSSessionTypeOption sessionTypeOption;
        internal string shellName;
        private bool showUI;
        internal bool showUISpecified;
        internal ApartmentState? threadAptState;
        internal PSThreadOptions? threadOptions;
        internal PSTransportOption transportOption;
        private bool useSharedProcess;

        internal PSSessionConfigurationCommandBase()
        {
        }

        internal static string GetLocalSddl()
        {
            if ((Environment.OSVersion.Version.Major >= 6) && (Environment.OSVersion.Version.Minor >= 2))
            {
                return "O:NSG:BAD:P(D;;GA;;;NU)(A;;GA;;;BA)(A;;GA;;;RM)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
            }
            return "O:NSG:BAD:P(D;;GA;;;NU)(A;;GA;;;BA)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        }

        internal static string GetRemoteSddl()
        {
            if ((Environment.OSVersion.Version.Major >= 6) && (Environment.OSVersion.Version.Minor >= 2))
            {
                return "O:NSG:BAD:P(A;;GA;;;BA)(A;;GA;;;RM)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
            }
            return "O:NSG:BAD:P(A;;GA;;;BA)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        }

        [Parameter]
        public PSSessionConfigurationAccessMode AccessMode
        {
            get
            {
                return this.accessMode;
            }
            set
            {
                this.accessMode = value;
                this.accessModeSpecified = true;
            }
        }

        [Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="AssemblyNameParameterSet")]
        public string ApplicationBase
        {
            get
            {
                return this.applicationBase;
            }
            set
            {
                this.applicationBase = value;
                this.isApplicationBaseSpecified = true;
            }
        }

        [Parameter(Position=1, Mandatory=true, ParameterSetName="AssemblyNameParameterSet")]
        public string AssemblyName
        {
            get
            {
                return this.assemblyName;
            }
            set
            {
                this.assemblyName = value;
                this.isAssemblyNameSpecified = true;
            }
        }

        [Parameter(Position=2, Mandatory=true, ParameterSetName="AssemblyNameParameterSet")]
        public string ConfigurationTypeName
        {
            get
            {
                return this.configurationTypeName;
            }
            set
            {
                this.configurationTypeName = value;
                this.isConfigurationTypeNameSpecified = true;
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return this.force;
            }
            set
            {
                this.force = (bool) value;
            }
        }

        [AllowNull, Parameter]
        public double? MaximumReceivedDataSizePerCommandMB
        {
            get
            {
                return this.maxCommandSizeMB;
            }
            set
            {
                if (value.HasValue && (value.Value < 0.0))
                {
                    throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.CSCDoubleParameterOutOfRange, new object[] { value.Value, "MaximumReceivedDataSizePerCommandMB" }));
                }
                this.maxCommandSizeMB = value;
                this.isMaxCommandSizeMBSpecified = true;
            }
        }

        [Parameter, AllowNull]
        public double? MaximumReceivedObjectSizeMB
        {
            get
            {
                return this.maxObjectSizeMB;
            }
            set
            {
                if (value.HasValue && (value.Value < 0.0))
                {
                    throw new ArgumentException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.CSCDoubleParameterOutOfRange, new object[] { value.Value, "MaximumReceivedObjectSizeMB" }));
                }
                this.maxObjectSizeMB = value;
                this.isMaxObjectSizeMBSpecified = true;
            }
        }

        [Parameter(ParameterSetName="NameParameterSet"), Parameter(ParameterSetName="AssemblyNameParameterSet")]
        public string[] ModulesToImport
        {
            get
            {
                return this.modulesToImport;
            }
            set
            {
                if (value != null)
                {
                    foreach (string str in value)
                    {
                        if (!string.IsNullOrEmpty(str.Trim()) && !Directory.Exists(str))
                        {
                            throw new ArgumentException(StringUtil.Format(RemotingErrorIdStrings.InvalidRegisterPSSessionConfigurationModulePath, str));
                        }
                    }
                }
                this.modulesToImport = value;
                this.modulePathSpecified = true;
            }
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="SessionConfigurationFile"), Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="AssemblyNameParameterSet"), Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true, ParameterSetName="NameParameterSet"), ValidateNotNullOrEmpty]
        public string Name
        {
            get
            {
                return this.shellName;
            }
            set
            {
                this.shellName = value;
            }
        }

        [Parameter]
        public SwitchParameter NoServiceRestart
        {
            get
            {
                return this.noRestart;
            }
            set
            {
                this.noRestart = (bool) value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Mandatory=true, ParameterSetName="SessionConfigurationFile")]
        public string Path
        {
            get
            {
                return this.configPath;
            }
            set
            {
                this.configPath = value;
            }
        }

        [Alias(new string[] { "PowerShellVersion" }), Parameter(ParameterSetName="AssemblyNameParameterSet"), ValidateNotNullOrEmpty, Parameter(ParameterSetName="NameParameterSet")]
        public Version PSVersion
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
                this.isPSVersionSpecified = true;
            }
        }

        [Credential, Parameter]
        public PSCredential RunAsCredential
        {
            get
            {
                return this.runAsCredential;
            }
            set
            {
                this.runAsCredential = value;
                this.isRunAsCredentialSpecified = true;
            }
        }

        [Parameter]
        public string SecurityDescriptorSddl
        {
            get
            {
                return this.sddl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && (new CommonSecurityDescriptor(false, false, value) == null))
                {
                    throw new NotSupportedException();
                }
                this.sddl = value;
                this.isSddlSpecified = true;
            }
        }

        [Parameter(ParameterSetName="AssemblyNameParameterSet"), Parameter(ParameterSetName="NameParameterSet")]
        public PSSessionTypeOption SessionTypeOption
        {
            get
            {
                return this.sessionTypeOption;
            }
            set
            {
                this.sessionTypeOption = value;
            }
        }

        [Parameter]
        public SwitchParameter ShowSecurityDescriptorUI
        {
            get
            {
                return this.showUI;
            }
            set
            {
                this.showUI = (bool) value;
                this.showUISpecified = true;
            }
        }

        [Parameter]
        public string StartupScript
        {
            get
            {
                return this.configurationScript;
            }
            set
            {
                this.configurationScript = value;
                this.isConfigurationScriptSpecified = true;
            }
        }

        [Parameter]
        public ApartmentState ThreadApartmentState
        {
            get
            {
                if (this.threadAptState.HasValue)
                {
                    return this.threadAptState.Value;
                }
                return ApartmentState.Unknown;
            }
            set
            {
                this.threadAptState = new ApartmentState?(value);
            }
        }

        [Parameter]
        public PSThreadOptions ThreadOptions
        {
            get
            {
                if (this.threadOptions.HasValue)
                {
                    return this.threadOptions.Value;
                }
                return PSThreadOptions.UseCurrentThread;
            }
            set
            {
                this.threadOptions = new PSThreadOptions?(value);
            }
        }

        [Parameter]
        public PSTransportOption TransportOption
        {
            get
            {
                return this.transportOption;
            }
            set
            {
                this.transportOption = value;
            }
        }

        [Parameter]
        public SwitchParameter UseSharedProcess
        {
            get
            {
                return this.useSharedProcess;
            }
            set
            {
                this.useSharedProcess = (bool) value;
                this.isUseSharedProcessSpecified = true;
            }
        }
    }
}

