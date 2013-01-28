namespace Microsoft.Management.Infrastructure.Native
{
	internal enum MiCancellationReason
	{
		None = 0,
		Timeout = 1,
		Shutdown = 2,
		ServiceStop = 3
	}
}