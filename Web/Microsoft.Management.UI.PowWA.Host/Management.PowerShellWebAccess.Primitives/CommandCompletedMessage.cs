using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class CommandCompletedMessage : ClientMessage
	{
		public string Prompt
		{
			get;
			private set;
		}

		internal CommandCompletedMessage(string prompt) : base(0)
		{
			this.Prompt = prompt;
		}
	}
}