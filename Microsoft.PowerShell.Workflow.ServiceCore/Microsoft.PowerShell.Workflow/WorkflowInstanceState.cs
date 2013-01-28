namespace Microsoft.PowerShell.Workflow
{
	internal enum WorkflowInstanceState
	{
		NotStarted,
		Loaded,
		Executing,
		CompletedAndClosed,
		Faulted,
		Canceled,
		Aborted,
		UnhandledExceptionAndTermination,
		Unloaded,
		Unknown
	}
}