using System;

namespace Microsoft.PowerShell.Workflow
{
	[Flags]
	internal enum WorkflowFilterTypes
	{
		None = 0,
		JobMetadata = 1,
		PrivateMetadata = 2,
		WorkflowSpecificParameters = 4,
		CommonParameters = 8,
		All = 15
	}
}