using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "ExecutionPolicy", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113315")]
	[OutputType(new Type[] { typeof(ExecutionPolicy) })]
	public class GetExecutionPolicyCommand : PSCmdlet
	{
		private ExecutionPolicyScope executionPolicyScope;

		private bool scopeSpecified;

		private bool list;

		[Parameter(Mandatory=false)]
		public SwitchParameter List
		{
			get
			{
				return this.list;
			}
			set
			{
				this.list = value;
			}
		}

		[Parameter(Position=0, Mandatory=false, ValueFromPipelineByPropertyName=true)]
		public ExecutionPolicyScope Scope
		{
			get
			{
				return this.executionPolicyScope;
			}
			set
			{
				this.executionPolicyScope = value;
				this.scopeSpecified = true;
			}
		}

		public GetExecutionPolicyCommand()
		{
			this.executionPolicyScope = ExecutionPolicyScope.LocalMachine;
		}

		protected override void BeginProcessing()
		{
			if (!this.list || !this.scopeSpecified)
			{
				string shellID = base.Context.ShellID;
				if (!this.list)
				{
					if (!this.scopeSpecified)
					{
						base.WriteObject(SecuritySupport.GetExecutionPolicy(shellID));
						return;
					}
					else
					{
						base.WriteObject(SecuritySupport.GetExecutionPolicy(shellID, this.executionPolicyScope));
						return;
					}
				}
				else
				{
					ExecutionPolicyScope[] executionPolicyScopePreferences = SecuritySupport.ExecutionPolicyScopePreferences;
					for (int i = 0; i < (int)executionPolicyScopePreferences.Length; i++)
					{
						ExecutionPolicyScope executionPolicyScope = executionPolicyScopePreferences[i];
						PSObject pSObject = new PSObject();
						ExecutionPolicy executionPolicy = SecuritySupport.GetExecutionPolicy(shellID, executionPolicyScope);
						PSNoteProperty pSNoteProperty = new PSNoteProperty("Scope", (object)executionPolicyScope);
						pSObject.Properties.Add(pSNoteProperty);
						pSNoteProperty = new PSNoteProperty("ExecutionPolicy", (object)executionPolicy);
						pSObject.Properties.Add(pSNoteProperty);
						base.WriteObject(pSObject);
					}
					return;
				}
			}
			else
			{
				string listAndScopeSpecified = ExecutionPolicyCommands.ListAndScopeSpecified;
				ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(), "ListAndScopeSpecified", ErrorCategory.InvalidOperation, null);
				errorRecord.ErrorDetails = new ErrorDetails(listAndScopeSpecified);
				base.ThrowTerminatingError(errorRecord);
				return;
			}
		}
	}
}