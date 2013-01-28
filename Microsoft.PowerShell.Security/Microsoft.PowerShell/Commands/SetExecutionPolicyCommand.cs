using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "ExecutionPolicy", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113394")]
	public class SetExecutionPolicyCommand : PSCmdlet
	{
		private ExecutionPolicy executionPolicy;

		private ExecutionPolicyScope executionPolicyScope;

		private SwitchParameter force;

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true)]
		public ExecutionPolicy ExecutionPolicy
		{
			get
			{
				return this.executionPolicy;
			}
			set
			{
				this.executionPolicy = value;
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
				this.force = value;
			}
		}

		[Parameter(Position=1, Mandatory=false, ValueFromPipelineByPropertyName=true)]
		public ExecutionPolicyScope Scope
		{
			get
			{
				return this.executionPolicyScope;
			}
			set
			{
				this.executionPolicyScope = value;
			}
		}

		public SetExecutionPolicyCommand()
		{
			this.executionPolicyScope = ExecutionPolicyScope.LocalMachine;
		}

		protected override void BeginProcessing()
		{
			if (this.executionPolicyScope == ExecutionPolicyScope.UserPolicy || this.executionPolicyScope == ExecutionPolicyScope.MachinePolicy)
			{
				string cantSetGroupPolicy = ExecutionPolicyCommands.CantSetGroupPolicy;
				ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(), "CantSetGroupPolicy", ErrorCategory.InvalidOperation, null);
				errorRecord.ErrorDetails = new ErrorDetails(cantSetGroupPolicy);
				base.ThrowTerminatingError(errorRecord);
			}
		}

		private bool IsProcessInteractive()
		{
			bool flag;
			if (base.MyInvocation.CommandOrigin == CommandOrigin.Runspace)
			{
				if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
				{
					try
					{
						Process currentProcess = Process.GetCurrentProcess();
						TimeSpan now = DateTime.Now - currentProcess.StartTime;
						TimeSpan totalProcessorTime = now - currentProcess.TotalProcessorTime;
						if (totalProcessorTime.TotalSeconds <= 1)
						{
							return false;
						}
						else
						{
							flag = true;
						}
					}
					catch (Win32Exception win32Exception)
					{
						flag = false;
					}
					return flag;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		protected override void ProcessRecord()
		{
			string shellID = base.Context.ShellID;
			string executionPolicy = SecuritySupport.GetExecutionPolicy(this.ExecutionPolicy);
			if (this.ShouldProcessPolicyChange(executionPolicy))
			{
				SecuritySupport.SetExecutionPolicy(this.executionPolicyScope, this.ExecutionPolicy, shellID);
				if (this.ExecutionPolicy != ExecutionPolicy.Undefined)
				{
					string str = SecuritySupport.GetExecutionPolicy(shellID).ToString();
					if (!string.Equals(str, executionPolicy, StringComparison.OrdinalIgnoreCase))
					{
						string str1 = StringUtil.Format(ExecutionPolicyCommands.ExecutionPolicyOverridden, str);
						string executionPolicyOverriddenRecommendedAction = ExecutionPolicyCommands.ExecutionPolicyOverriddenRecommendedAction;
						ErrorRecord errorRecord = new ErrorRecord(new SecurityException(), "ExecutionPolicyOverride", ErrorCategory.PermissionDenied, null);
						errorRecord.ErrorDetails = new ErrorDetails(str1);
						errorRecord.ErrorDetails.RecommendedAction = executionPolicyOverriddenRecommendedAction;
						base.ThrowTerminatingError(errorRecord);
					}
				}
				PSSQMAPI.UpdateExecutionPolicy(shellID, this.ExecutionPolicy);
				PSEtwLog.LogSettingsEvent(MshLog.GetLogContext(base.Context, base.MyInvocation), EtwLoggingStrings.ExecutionPolicyName, executionPolicy, null);
			}
		}

		private bool ShouldProcessPolicyChange(string localPreference)
		{
			bool flag;
			if (!base.ShouldProcess(localPreference))
			{
				return false;
			}
			else
			{
				if (!this.Force && this.IsProcessInteractive())
				{
					string setExecutionPolicyQuery = ExecutionPolicyCommands.SetExecutionPolicyQuery;
					string setExecutionPolicyCaption = ExecutionPolicyCommands.SetExecutionPolicyCaption;
					try
					{
						if (base.ShouldContinue(setExecutionPolicyQuery, setExecutionPolicyCaption))
						{
							return true;
						}
						else
						{
							flag = false;
						}
					}
					catch (InvalidOperationException invalidOperationException)
					{
						flag = true;
					}
					catch (HostException hostException)
					{
						flag = true;
					}
					return flag;
				}
				return true;
			}
		}
	}
}