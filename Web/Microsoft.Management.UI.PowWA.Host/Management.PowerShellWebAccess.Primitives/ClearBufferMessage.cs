namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class ClearBufferMessage : ClientMessage
	{
		internal ClearBufferMessage() : base((ClientMessageType)109)
		{

		}
	}
}