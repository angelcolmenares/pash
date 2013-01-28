using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PSRemotingActivity : PSActivity, IImplementsConnectionRetry
	{
		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<bool?> PSAllowRedirection
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string> PSApplicationName
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<AuthenticationMechanism?> PSAuthentication
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string> PSCertificateThumbprint
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string[]> PSComputerName
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string> PSConfigurationName
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSConnectionRetryCount
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSConnectionRetryIntervalSec
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string[]> PSConnectionUri
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<PSCredential> PSCredential
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSPort
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<RemotingBehavior> PSRemotingBehavior
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<PSSessionOption> PSSessionOption
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<bool?> PSUseSsl
		{
			get;
			set;
		}

		protected virtual bool SupportsCustomRemoting
		{
			get
			{
				return false;
			}
		}

		protected PSRemotingActivity()
		{
		}

		private void CreatePowerShellInstance(NativeActivityContext context, WSManConnectionInfo connection, List<ActivityImplementationContext> commands)
		{
			ActivityImplementationContext powerShell = this.GetPowerShell(context);
			if (connection == null)
			{
				base.UpdateImplementationContextForLocalExecution(powerShell, context);
			}
			else
			{
				powerShell.ConnectionInfo = connection;
				Runspace runspace = RunspaceFactory.CreateRunspace(connection);
				powerShell.PowerShellInstance.Runspace = runspace;
			}
			commands.Add(powerShell);
		}

		protected override List<ActivityImplementationContext> GetImplementation(NativeActivityContext context)
		{
			bool hasValue;
			string[] strArrays = this.PSComputerName.Get(context);
			string[] strArrays1 = this.PSConnectionUri.Get(context);
			PSSessionOption pSSessionOption = this.PSSessionOption.Get(context);
			List<ActivityImplementationContext> activityImplementationContexts = new List<ActivityImplementationContext>();
			RemotingBehavior remotingBehavior = this.PSRemotingBehavior.Get(context);
			if (this.PSRemotingBehavior.Expression == null)
			{
				remotingBehavior = RemotingBehavior.PowerShell;
			}
			if (remotingBehavior != RemotingBehavior.Custom || this.SupportsCustomRemoting)
			{
				if (this.PSCredential.Get(context) != null)
				{
					AuthenticationMechanism? nullable = this.PSAuthentication.Get(context);
					if (nullable.GetValueOrDefault() != AuthenticationMechanism.NegotiateWithImplicitCredential)
					{
						hasValue = false;
					}
					else
					{
						hasValue = nullable.HasValue;
					}
					if (hasValue)
					{
						throw new ArgumentException(Resources.CredentialParameterCannotBeSpecifiedWithNegotiateWithImplicitAuthentication);
					}
				}
				if ((remotingBehavior == RemotingBehavior.PowerShell || PSActivity.IsActivityInlineScript(this) && base.RunWithCustomRemoting(context)) && (this.GetIsComputerNameSpecified(context) || strArrays1 != null && (int)strArrays1.Length > 0))
				{
					AuthenticationMechanism? nullable1 = this.PSAuthentication.Get(context);
					bool? nullable2 = this.PSAllowRedirection.Get(context);
					List<WSManConnectionInfo> connectionInfo = ActivityUtils.GetConnectionInfo(strArrays, strArrays1, this.PSCertificateThumbprint.Get(context), this.PSConfigurationName.Get(context), this.PSUseSsl.Get(context), this.PSPort.Get(context), this.PSApplicationName.Get(context), this.PSCredential.Get(context), nullable1.GetValueOrDefault(AuthenticationMechanism.Default), nullable2.GetValueOrDefault(false), pSSessionOption);
					foreach (WSManConnectionInfo wSManConnectionInfo in connectionInfo)
					{
						this.CreatePowerShellInstance(context, wSManConnectionInfo, activityImplementationContexts);
					}
				}
				else
				{
					this.CreatePowerShellInstance(context, null, activityImplementationContexts);
				}
				return activityImplementationContexts;
			}
			else
			{
				throw new ArgumentException(Resources.CustomRemotingNotSupported);
			}
		}

		protected bool GetIsComputerNameSpecified(ActivityContext context)
		{
			if (this.PSComputerName.Get(context) == null)
			{
				return false;
			}
			else
			{
				return (int)this.PSComputerName.Get(context).Length > 0;
			}
		}
	}
}