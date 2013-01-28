namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class ReadLineMessage : PromptMessageBase
	{
		internal ReadLineMessage() : base((ClientMessageType)104)
		{
		}
	}
}