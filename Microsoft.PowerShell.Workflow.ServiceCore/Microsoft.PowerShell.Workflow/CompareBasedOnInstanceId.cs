using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Workflow
{
	internal class CompareBasedOnInstanceId : IEqualityComparer<WorkflowJobDefinition>
	{
		public CompareBasedOnInstanceId()
		{
		}

		public bool Equals(WorkflowJobDefinition x, WorkflowJobDefinition y)
		{
			return x.InstanceId == y.InstanceId;
		}

		public int GetHashCode(WorkflowJobDefinition obj)
		{
			Guid instanceId = obj.InstanceId;
			return instanceId.GetHashCode();
		}
	}
}