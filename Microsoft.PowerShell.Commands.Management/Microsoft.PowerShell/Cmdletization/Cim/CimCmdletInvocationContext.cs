using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class CimCmdletInvocationContext
	{
		private readonly Lazy<CimSession> _defaultCimSession;

		public CimCmdletDefinitionContext CmdletDefinitionContext
		{
			get;
			private set;
		}

		public InvocationInfo CmdletInvocationInfo
		{
			get;
			private set;
		}

		public ActionPreference DebugActionPreference
		{
			get;
			private set;
		}

		public ActionPreference ErrorActionPreference
		{
			get;
			private set;
		}

		public bool IsRunningInBackground
		{
			get
			{
				return this.CmdletInvocationInfo.BoundParameters.ContainsKey("AsJob");
			}
		}

		public string NamespaceOverride
		{
			get;
			private set;
		}

		public MshCommandRuntime.ShouldProcessPossibleOptimization ShouldProcessOptimization
		{
			get;
			private set;
		}

		public bool ShowComputerName
		{
			get
			{
				return this.CmdletInvocationInfo.BoundParameters.ContainsKey("CimSession");
			}
		}

		public ActionPreference VerboseActionPreference
		{
			get;
			private set;
		}

		public ActionPreference WarningActionPreference
		{
			get;
			private set;
		}

		internal CimCmdletInvocationContext(CimCmdletDefinitionContext cmdletDefinitionContext, Cmdlet cmdlet, string namespaceOverride)
		{
			this._defaultCimSession = new Lazy<CimSession>(new Func<CimSession>(CimCmdletInvocationContext.CreateDefaultCimSession));
			this.CmdletDefinitionContext = cmdletDefinitionContext;
			this.NamespaceOverride = namespaceOverride;
			this.CmdletInvocationInfo = cmdlet.MyInvocation;
			MshCommandRuntime commandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
			this.DebugActionPreference = commandRuntime.DebugPreference;
			Cmdlet cmdlet1 = cmdlet;
			ActionPreference debugActionPreference = this.DebugActionPreference;
			string str = "Debug";
			Func<string> func = () => CmdletizationResources.CimCmdletAdapter_DebugInquire;
			CimCmdletInvocationContext.WarnAboutUnsupportedActionPreferences(cmdlet1, debugActionPreference, str, func, () => string.Empty);
			this.WarningActionPreference = commandRuntime.WarningPreference;
			Cmdlet cmdlet2 = cmdlet;
			ActionPreference warningActionPreference = this.WarningActionPreference;
			string str1 = "WarningAction";
			Func<string> func1 = () => CmdletizationResources.CimCmdletAdapter_WarningInquire;
			CimCmdletInvocationContext.WarnAboutUnsupportedActionPreferences(cmdlet2, warningActionPreference, str1, func1, () => CmdletizationResources.CimCmdletAdapter_WarningStop);
			this.VerboseActionPreference = commandRuntime.VerbosePreference;
			this.ErrorActionPreference = commandRuntime.ErrorAction;
			this.ShouldProcessOptimization = commandRuntime.CalculatePossibleShouldProcessOptimization();
		}

		private static CimSession CreateDefaultCimSession()
		{
			return CimSession.Create(null);
		}

		public CimSession GetDefaultCimSession()
		{
			return this._defaultCimSession.Value;
		}

		private static void WarnAboutUnsupportedActionPreferences(Cmdlet cmdlet, ActionPreference effectiveActionPreference, string nameOfCommandLineParameter, Func<string> inquireMessageGetter, Func<string> stopMessageGetter)
		{
			string str;
			ActionPreference actionPreference = effectiveActionPreference;
			if (actionPreference == ActionPreference.Stop)
			{
				str = stopMessageGetter();
			}
			else if (actionPreference == ActionPreference.Continue)
			{
				return;
			}
			else if (actionPreference == ActionPreference.Inquire)
			{
				str = inquireMessageGetter();
			}
			else
			{
				return;
			}
			bool flag = cmdlet.MyInvocation.BoundParameters.ContainsKey(nameOfCommandLineParameter);
			if (flag)
			{
				Exception argumentException = new ArgumentException(str);
				ErrorRecord errorRecord = new ErrorRecord(argumentException, "ActionPreferenceNotSupportedByCimCmdletAdapter", ErrorCategory.NotImplemented, null);
				cmdlet.ThrowTerminatingError(errorRecord);
			}
		}
	}
}