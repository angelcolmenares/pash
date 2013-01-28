namespace Microsoft.PowerShell.Commands
{
	public enum WmiState
	{
		NotStarted,
		Running,
		Stopping,
		Stopped,
		Completed,
		Failed
	}
}