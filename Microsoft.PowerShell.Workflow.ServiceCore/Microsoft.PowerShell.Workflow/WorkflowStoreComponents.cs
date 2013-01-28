using System;

namespace Microsoft.PowerShell.Workflow
{
	[Flags]
	public enum WorkflowStoreComponents
	{
		Streams = 1,
		Metadata = 2,
		Definition = 4,
		Timer = 8,
		JobState = 16,
		TerminatingError = 32
	}
}