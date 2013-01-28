using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class ExitMessage : ClientMessage
	{
		public int ExitCode
		{
			get;
			private set;
		}

		internal ExitMessage(int exitCode) : base((ClientMessageType)100)
		{
			this.ExitCode = exitCode;
		}
	}
}