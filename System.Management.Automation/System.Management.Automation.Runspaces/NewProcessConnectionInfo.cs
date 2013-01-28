namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;

    internal sealed class NewProcessConnectionInfo : RunspaceConnectionInfo
    {
        private System.Management.Automation.Runspaces.AuthenticationMechanism authMechanism;
        private PSCredential credential;
        private ScriptBlock initScript;
        private PowerShellProcessInstance process;
        private Version psVersion;
        private bool shouldRunsAs32;

        internal NewProcessConnectionInfo(PSCredential credential)
        {
            this.credential = credential;
            this.authMechanism = System.Management.Automation.Runspaces.AuthenticationMechanism.Default;
        }

        public NewProcessConnectionInfo Copy()
        {
            return new NewProcessConnectionInfo(this.credential) { AuthenticationMechanism = this.AuthenticationMechanism, InitializationScript = this.InitializationScript, RunAs32 = this.RunAs32, PSVersion = this.PSVersion, Process = this.Process };
        }

        public override System.Management.Automation.Runspaces.AuthenticationMechanism AuthenticationMechanism
        {
            get
            {
                return this.authMechanism;
            }
            set
            {
                if (value != System.Management.Automation.Runspaces.AuthenticationMechanism.Default)
                {
                    throw PSTraceSource.NewInvalidOperationException("RemotingErrorIdStrings", PSRemotingErrorId.IPCSupportsOnlyDefaultAuth.ToString(), new object[] { value.ToString(), System.Management.Automation.Runspaces.AuthenticationMechanism.Default.ToString() });
                }
                this.authMechanism = value;
            }
        }

        public override string CertificateThumbprint
        {
            get
            {
                return string.Empty;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string ComputerName
        {
            get
            {
                return "localhost";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override PSCredential Credential
        {
            get
            {
                return this.credential;
            }
            set
            {
                this.credential = value;
                this.authMechanism = System.Management.Automation.Runspaces.AuthenticationMechanism.Default;
            }
        }

        public ScriptBlock InitializationScript
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

        internal PowerShellProcessInstance Process
        {
            get
            {
                return this.process;
            }
            set
            {
                this.process = value;
            }
        }

        public Version PSVersion
        {
            get
            {
                return this.psVersion;
            }
            set
            {
                this.psVersion = value;
            }
        }

        public bool RunAs32
        {
            get
            {
                return this.shouldRunsAs32;
            }
            set
            {
                this.shouldRunsAs32 = value;
            }
        }

		public override PSObject ToPSObjectForRemoting ()
		{
			var obj = RemotingEncoder.CreateEmptyPSObject ();
			return obj;
		}

		public static RunspaceConnectionInfo FromPSObjectForRemoting(PSObject obj)
		{
			return new NewProcessConnectionInfo(null);
		}

    }
}

