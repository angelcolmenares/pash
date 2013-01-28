using System;
using System.Activities.Validation;

namespace Microsoft.PowerShell.Workflow
{
	internal class PSWorkflowValidationResults
	{
		internal bool IsWorkflowSuspendable
		{
			get;
			set;
		}

		internal ValidationResults Results
		{
			get;
			set;
		}

		internal PSWorkflowValidationResults()
		{
			this.IsWorkflowSuspendable = false;
			this.Results = null;
		}
	}
}