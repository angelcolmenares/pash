namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PowwaSessionStatusInfo
	{
		public ClientMessage PromptMessage
		{
			get;
			private set;
		}

		public PowwaSessionStatus Status
		{
			get;
			private set;
		}

		internal PowwaSessionStatusInfo(PowwaSessionStatus status, ClientMessage promptMessage)
		{
			this.Status = status;
			this.PromptMessage = promptMessage;
		}
	}
}