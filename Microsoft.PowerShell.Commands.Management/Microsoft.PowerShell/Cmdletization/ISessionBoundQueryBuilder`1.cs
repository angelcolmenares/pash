namespace Microsoft.PowerShell.Cmdletization
{
	internal interface ISessionBoundQueryBuilder<out TSession>
	{
		TSession GetTargetSession();
	}
}