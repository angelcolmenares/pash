using Microsoft.Management.Infrastructure;
using System;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Activities
{
	public class ActivityImplementationContext
	{
		public bool? AppendOutput
		{
			get;
			set;
		}

		public string Authority
		{
			get;
			set;
		}

		public CimSession[] CimSession
		{
			get;
			set;
		}

		public WSManConnectionInfo ConnectionInfo
		{
			get;
			set;
		}

		public bool? Debug
		{
			get;
			set;
		}

		public bool EnableAllPrivileges
		{
			get;
			set;
		}

		public ActionPreference? ErrorAction
		{
			get;
			set;
		}

		public ImpersonationLevel Impersonation
		{
			get;
			set;
		}

		public PSDataCollection<PSObject> Input
		{
			get;
			set;
		}

		public string Locale
		{
			get;
			set;
		}

		public bool? MergeErrorToOutput
		{
			get;
			set;
		}

		public string Namespace
		{
			get;
			set;
		}

		public System.Management.Automation.PowerShell PowerShellInstance
		{
			get;
			set;
		}

		public uint? PSActionRetryCount
		{
			get;
			set;
		}

		public uint? PSActionRetryIntervalSec
		{
			get;
			set;
		}

		public uint? PSActionRunningTimeoutSec
		{
			get;
			set;
		}

		public PSActivityEnvironment PSActivityEnvironment
		{
			get;
			set;
		}

		public bool? PSAllowRedirection
		{
			get;
			set;
		}

		public string PSApplicationName
		{
			get;
			set;
		}

		public AuthenticationMechanism? PSAuthentication
		{
			get;
			set;
		}

		public AuthenticationLevel PSAuthenticationLevel
		{
			get;
			set;
		}

		public string PSCertificateThumbprint
		{
			get;
			set;
		}

		public string[] PSComputerName
		{
			get;
			set;
		}

		public string PSConfigurationName
		{
			get;
			set;
		}

		public uint? PSConnectionRetryCount
		{
			get;
			set;
		}

		public uint? PSConnectionRetryIntervalSec
		{
			get;
			set;
		}

		public string[] PSConnectionUri
		{
			get;
			set;
		}

		public PSCredential PSCredential
		{
			get;
			set;
		}

		public PSDataCollection<DebugRecord> PSDebug
		{
			get;
			set;
		}

		public bool? PSDisableSerialization
		{
			get;
			set;
		}

		public PSDataCollection<ErrorRecord> PSError
		{
			get;
			set;
		}

		public bool? PSPersist
		{
			get;
			set;
		}

		public uint? PSPort
		{
			get;
			set;
		}

		public PSDataCollection<ProgressRecord> PSProgress
		{
			get;
			set;
		}

		public string PSProgressMessage
		{
			get;
			set;
		}

		public RemotingBehavior PSRemotingBehavior
		{
			get;
			set;
		}

		public string[] PSRequiredModules
		{
			get;
			set;
		}

		public PSSessionOption PSSessionOption
		{
			get;
			set;
		}

		public bool? PSUseSsl
		{
			get;
			set;
		}

		public PSDataCollection<VerboseRecord> PSVerbose
		{
			get;
			set;
		}

		public PSDataCollection<WarningRecord> PSWarning
		{
			get;
			set;
		}

		public string PSWorkflowPath
		{
			get;
			set;
		}

		public PSDataCollection<PSObject> Result
		{
			get;
			set;
		}

		public bool? Verbose
		{
			get;
			set;
		}

		public ActionPreference? WarningAction
		{
			get;
			set;
		}

		public bool? WhatIf
		{
			get;
			set;
		}

		public object WorkflowContext
		{
			get;
			set;
		}

		public ActivityImplementationContext()
		{
		}

		public virtual void CleanUp()
		{
		}
	}
}