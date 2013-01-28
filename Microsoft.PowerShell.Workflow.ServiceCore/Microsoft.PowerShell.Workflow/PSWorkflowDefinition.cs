using System;
using System.Activities;

namespace Microsoft.PowerShell.Workflow
{
	public sealed class PSWorkflowDefinition
	{
		private Activity workflow;

		private string workflowXaml;

		private string runtimeAssemblyPath;

		public string RuntimeAssemblyPath
		{
			get
			{
				return this.runtimeAssemblyPath;
			}
			set
			{
				this.runtimeAssemblyPath = value;
			}
		}

		public Activity Workflow
		{
			get
			{
				return this.workflow;
			}
			set
			{
				this.workflow = value;
			}
		}

		public string WorkflowXaml
		{
			get
			{
				return this.workflowXaml;
			}
			set
			{
				this.workflowXaml = value;
			}
		}

		public PSWorkflowDefinition(Activity workflow, string workflowXaml, string runtimeAssemblyPath)
		{
			this.workflow = workflow;
			this.workflowXaml = workflowXaml;
			this.runtimeAssemblyPath = runtimeAssemblyPath;
		}
	}
}