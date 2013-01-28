using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[Cmdlet("New", "CimSession", DefaultParameterSetName="CredentialParameterSet", HelpUri="http://go.microsoft.com/fwlink/?LinkId=227967")]
	[OutputType(new Type[] { typeof(CimSession) })]
	public sealed class NewCimSessionCommand : CimBaseCommand
	{
		private PasswordAuthenticationMechanism authentication;

		private bool authenticationSet;

		private PSCredential credential;

		private string certificatethumbprint;

		private string[] computername;

		private string name;

		private uint operationTimeout;

		internal bool operationTimeoutSet;

		private SwitchParameter skipTestConnection;

		private uint port;

		private bool portSet;

		private CimSessionOptions sessionOption;

		private CimNewSession cimNewSession;

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CredentialParameterSet")]
		public PasswordAuthenticationMechanism Authentication
		{
			get
			{
				return this.authentication;
			}
			set
			{
				this.authentication = value;
				this.authenticationSet = true;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true, ParameterSetName="CertificatePrameterSet")]
		public string CertificateThumbprint
		{
			get
			{
				return this.certificatethumbprint;
			}
			set
			{
				this.certificatethumbprint = value;
			}
		}

		[Alias(new string[] { "CN", "ServerName" })]
		[Parameter(Position=0, ValueFromPipelineByPropertyName=true)]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
			}
		}

		[Credential]
		[Parameter(Position=1, ParameterSetName="CredentialParameterSet")]
		public PSCredential Credential
		{
			get
			{
				return this.credential;
			}
			set
			{
				this.credential = value;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		[Alias(new string[] { "OT" })]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		public uint OperationTimeoutSec
		{
			get
			{
				return this.operationTimeout;
			}
			set
			{
				this.operationTimeout = value;
				this.operationTimeoutSet = true;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public uint Port
		{
			get
			{
				return this.port;
			}
			set
			{
				this.port = value;
				this.portSet = true;
			}
		}

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public CimSessionOptions SessionOption
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public SwitchParameter SkipTestConnection
		{
			get
			{
				return this.skipTestConnection;
			}
			set
			{
				this.skipTestConnection = value;
			}
		}

		public NewCimSessionCommand()
		{
		}

		protected override void BeginProcessing()
		{
			this.cimNewSession = new CimNewSession();
			this.CmdletOperation = new CmdletOperationTestCimSession(this, this.cimNewSession);
			base.AtBeginProcess = false;
		}

		internal void BuildSessionOptions (out CimSessionOptions outputOptions, out CimCredential outputCredential)
		{
			WSManSessionOptions wSManSessionOption;
			PasswordAuthenticationMechanism authentication;
			DebugHelper.WriteLogEx ();
			CimSessionOptions dComSessionOption = null;

			/* Requires Authentication for Remote Host */
			if (this.credential == null && ComputerName != null) {
				bool requiredAuth = false;
				foreach(var c in ComputerName)
				{
					if (c != null && !c.Equals("localhost", StringComparison.OrdinalIgnoreCase))
					{
						requiredAuth = true;
						break;
					}
				}
				if (requiredAuth)
				{
					TrySetCredentials();
				}
			}

			if (this.SessionOption != null) {
				if (this.SessionOption as WSManSessionOptions == null) {
					dComSessionOption = new DComSessionOptions (this.sessionOption as DComSessionOptions);
				} else {
					dComSessionOption = new WSManSessionOptions (this.sessionOption as WSManSessionOptions);
				}
			}
			outputOptions = null;
			outputCredential = null;
			if (dComSessionOption != null) {
				DComSessionOptions dComSessionOption1 = dComSessionOption as DComSessionOptions;
				if (dComSessionOption1 != null) {
					bool flag = false;
					string empty = string.Empty;
					if (this.CertificateThumbprint != null) {
						flag = true;
						empty = "CertificateThumbprint";
					}
					if (this.portSet) {
						flag = true;
						empty = "Port";
					}
					if (flag) {
						base.ThrowConflictParameterWasSet ("New-CimSession", empty, "DComSessionOptions");
						return;
					}
				}
			}

			if (this.portSet || this.CertificateThumbprint != null) {
				if (dComSessionOption == null) {
					wSManSessionOption = new WSManSessionOptions ();
				} else {
					wSManSessionOption = dComSessionOption as WSManSessionOptions;
				}
				WSManSessionOptions port = wSManSessionOption;
				if (this.portSet) {
					port.DestinationPort = this.Port;
					this.portSet = false;
				}
				if (this.CertificateThumbprint != null) {
					CimCredential cimCredential = new CimCredential (CertificateAuthenticationMechanism.Default, this.CertificateThumbprint);
					port.AddDestinationCredentials (cimCredential);
				}
				dComSessionOption = port;
			}
			if (this.operationTimeoutSet && dComSessionOption != null) {
				dComSessionOption.Timeout = TimeSpan.FromSeconds ((double)((float)this.OperationTimeoutSec));
			}

			if (this.authenticationSet || this.credential != null) {
				if (this.authenticationSet) {
					authentication = this.Authentication;
				} else {
					authentication = PasswordAuthenticationMechanism.Default;
				}
				PasswordAuthenticationMechanism passwordAuthenticationMechanism = authentication;
				if (this.authenticationSet) {
					this.authenticationSet = false;
				}
				CimCredential cimCredential1 = base.CreateCimCredentials (this.Credential, passwordAuthenticationMechanism, "New-CimSession", "Authentication");
				if (cimCredential1 != null) {
					object[] objArray = new object[1];
					objArray [0] = cimCredential1;
					DebugHelper.WriteLog ("Credentials: {0}", 1, objArray);
					outputCredential = cimCredential1;
					if (dComSessionOption != null) {
						object[] objArray1 = new object[1];
						objArray1 [0] = dComSessionOption;
						DebugHelper.WriteLog ("Add credentials to option: {0}", 1, objArray1);
						dComSessionOption.AddDestinationCredentials (cimCredential1);
					}
				} else {
					return;
				}
			}
		
			object[] objArray2 = new object[1];
			objArray2[0] = outputOptions;
			DebugHelper.WriteLogEx("Set outputOptions: {0}", 1, objArray2);
			outputOptions = dComSessionOption;
		}

		private void TrySetCredentials ()
		{
			try 
			{
				this.credential = GetOnTheFlyCredentials ();
				this.authenticationSet = true;
			}
			catch(Exception ex)
			{
				var msg = ex.Message;
			}
		}


		protected override void DisposeInternal()
		{
			base.DisposeInternal();
			if (this.cimNewSession != null)
			{
				this.cimNewSession.Dispose();
			}
		}

		protected override void EndProcessing()
		{
			this.cimNewSession.ProcessRemainActions(this.CmdletOperation);
		}

		protected override void ProcessRecord()
		{
			CimSessionOptions cimSessionOption = null;
			CimCredential cimCredential = null;
			this.BuildSessionOptions(out cimSessionOption, out cimCredential);
			this.cimNewSession.NewCimSession(this, cimSessionOption, cimCredential);
			this.cimNewSession.ProcessActions(this.CmdletOperation);
		}
	}
}