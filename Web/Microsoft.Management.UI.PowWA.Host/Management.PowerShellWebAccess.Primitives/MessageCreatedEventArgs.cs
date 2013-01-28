using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal class MessageCreatedEventArgs : EventArgs
	{
		internal bool IsInputMessage
		{
			get;
			private set;
		}

		public ClientMessage Message
		{
			get;
			private set;
		}

		public object Reply
		{
			get;
			set;
		}

		internal MessageCreatedEventArgs(ClientMessage message, bool isInputMessage)
		{
			this.Message = message;
			this.IsInputMessage = isInputMessage;
		}
	}
}