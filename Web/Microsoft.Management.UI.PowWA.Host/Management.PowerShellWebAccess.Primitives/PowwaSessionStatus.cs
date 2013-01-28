namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public enum PowwaSessionStatus
	{
		Available,
		Executing,
		Cancelling,
		Prompting,
		Closed
	}
}