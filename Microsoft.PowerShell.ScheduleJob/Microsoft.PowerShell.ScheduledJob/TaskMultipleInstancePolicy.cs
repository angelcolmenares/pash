namespace Microsoft.PowerShell.ScheduledJob
{
	public enum TaskMultipleInstancePolicy
	{
		None,
		IgnoreNew,
		Parallel,
		Queue,
		StopExisting
	}
}