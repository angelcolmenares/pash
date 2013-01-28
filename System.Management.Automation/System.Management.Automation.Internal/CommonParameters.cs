namespace System.Management.Automation.Internal
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    public sealed class CommonParameters
    {
        private MshCommandRuntime commandRuntime;
        internal static string[] CommonCommandParameters = new string[] { "Verbose", "Debug", "ErrorAction", "WarningAction", "ErrorVariable", "WarningVariable", "OutVariable", "OutBuffer" };
        internal static string[] CommonWorkflowParameters = new string[] { 
            "PSComputerName", "JobName", "PSApplicationName", "PSCredential", "PSPort", "PSConfigurationName", "PSConnectionURI", "PSSessionOption", "PSAuthentication", "PSAuthenticationLevel", "PSCertificateThumbprint", "PSConnectionRetryCount", "PSConnectionRetryIntervalSec", "PSRunningTimeoutSec", "PSElapsedTimeoutSec", "PSPersist", 
            "PSPrivateMetadata", "InputObject", "PSParameterCollection", "AsJob", "PSUseSSL", "PSAllowRedirection"
         };
        internal static Type[] CommonWorkflowParameterTypes = new Type[] { 
            typeof(string[]), typeof(string), typeof(string), typeof(PSCredential), typeof(int), typeof(string), typeof(string[]), typeof(PSSessionOption), typeof(AuthenticationMechanism), typeof(AuthenticationLevel), typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), 
            typeof(object), typeof(object), typeof(Hashtable), typeof(bool), typeof(bool), typeof(bool)
         };

        internal CommonParameters(MshCommandRuntime commandRuntime)
        {
            if (commandRuntime == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandRuntime");
            }
            this.commandRuntime = commandRuntime;
        }

        [Parameter, Alias(new string[] { "db" })]
        public SwitchParameter Debug
        {
            get
            {
                return this.commandRuntime.Debug;
            }
            set
            {
                this.commandRuntime.Debug = (bool) value;
            }
        }

        [Parameter, Alias(new string[] { "ea" })]
        public ActionPreference ErrorAction
        {
            get
            {
                return this.commandRuntime.ErrorAction;
            }
            set
            {
                this.commandRuntime.ErrorAction = value;
            }
        }

        [Alias(new string[] { "ev" }), Parameter, ValidateVariableName]
        public string ErrorVariable
        {
            get
            {
                return this.commandRuntime.ErrorVariable;
            }
            set
            {
                this.commandRuntime.ErrorVariable = value;
            }
        }

        [Alias(new string[] { "ob" }), ValidateRange(0, 0x7fffffff), Parameter]
        public int OutBuffer
        {
            get
            {
                return this.commandRuntime.OutBuffer;
            }
            set
            {
                this.commandRuntime.OutBuffer = value;
            }
        }

        [ValidateVariableName, Parameter, Alias(new string[] { "ov" })]
        public string OutVariable
        {
            get
            {
                return this.commandRuntime.OutVariable;
            }
            set
            {
                this.commandRuntime.OutVariable = value;
            }
        }

        [Alias(new string[] { "vb" }), Parameter]
        public SwitchParameter Verbose
        {
            get
            {
                return this.commandRuntime.Verbose;
            }
            set
            {
                this.commandRuntime.Verbose = (bool) value;
            }
        }

        [Parameter, Alias(new string[] { "wa" })]
        public ActionPreference WarningAction
        {
            get
            {
                return this.commandRuntime.WarningPreference;
            }
            set
            {
                this.commandRuntime.WarningPreference = value;
            }
        }

        [Alias(new string[] { "wv" }), ValidateVariableName, Parameter]
        public string WarningVariable
        {
            get
            {
                return this.commandRuntime.WarningVariable;
            }
            set
            {
                this.commandRuntime.WarningVariable = value;
            }
        }

        internal class ValidateVariableName : ValidateArgumentsAttribute
        {
            protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
            {
                string str = arguments as string;
                if (str != null)
                {
                    if (str.StartsWith("+", StringComparison.Ordinal))
                    {
                        str = str.Substring(1);
                    }
                    VariablePath path = new VariablePath(str);
                    if (!path.IsVariable)
                    {
                        throw new ValidationMetadataException("ArgumentNotValidVariableName", null, Metadata.ValidateVariableName, new object[] { str });
                    }
                }
            }
        }
    }
}

