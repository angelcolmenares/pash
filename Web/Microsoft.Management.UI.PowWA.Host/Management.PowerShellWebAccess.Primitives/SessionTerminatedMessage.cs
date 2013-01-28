namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class SessionTerminatedMessage : ClientMessage
	{
		internal SessionTerminatedMessage() : base((ClientMessageType)115)
		{
		}
	}
}