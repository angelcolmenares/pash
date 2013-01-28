using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Activities
{
	public class CimActivityImplementationContext : ActivityImplementationContext
	{
		private static ScriptBlock _moduleScriptBlock;

		private string _moduleDefinition;

		public string ComputerName
		{
			get;
			set;
		}

		public string ModuleDefinition
		{
			get
			{
				return this._moduleDefinition;
			}
		}

		public ScriptBlock ModuleScriptBlock
		{
			get
			{
				return CimActivityImplementationContext._moduleScriptBlock;
			}
		}

		public Uri ResourceUri
		{
			get;
			set;
		}

		public CimSession Session
		{
			get;
			set;
		}

		public CimSessionOptions SessionOptions
		{
			get;
			set;
		}

		public CimActivityImplementationContext(ActivityImplementationContext activityImplementationContext, string computerName, PSCredential credential, string certificateThumbprint, AuthenticationMechanism? authenticationMechanism, bool useSsl, uint port, PSSessionOption sessionOption, CimSession session, CimSessionOptions cimSessionOptions, string moduleDefinition, Uri resourceUri)
		{
			if (activityImplementationContext != null)
			{
				base.PowerShellInstance = activityImplementationContext.PowerShellInstance;
				this.ResourceUri = resourceUri;
				this.ComputerName = computerName;
				base.PSCredential = credential;
				base.PSCertificateThumbprint = certificateThumbprint;
				base.PSAuthentication = authenticationMechanism;
				base.PSUseSsl = new bool?(useSsl);
				base.PSPort = new uint?(port);
				base.PSSessionOption = sessionOption;
				this.Session = session;
				this.SessionOptions = cimSessionOptions;
				if (moduleDefinition != null)
				{
					CimActivityImplementationContext._moduleScriptBlock = ScriptBlock.Create(moduleDefinition);
					this._moduleDefinition = moduleDefinition;
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("activityImplementationContext");
			}
		}

		public override void CleanUp()
		{
			if (this.Session != null && !string.IsNullOrEmpty(this.ComputerName))
			{
				CimConnectionManager.GetGlobalCimConnectionManager().ReleaseSession(this.ComputerName, this.Session);
				this.Session = null;
				this.ComputerName = null;
			}
		}
	}
}