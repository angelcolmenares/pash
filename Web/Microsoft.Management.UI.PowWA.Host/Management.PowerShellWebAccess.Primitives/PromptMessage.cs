using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PromptMessage : PromptMessageBase
	{
		public string Caption
		{
			get;
			private set;
		}

		public PromptFieldDescription[] Descriptions
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		internal PromptMessage(string caption, string message, PromptFieldDescription[] descriptions) : base((ClientMessageType)101)
		{
			this.Caption = caption;
			this.Message = message;
			this.Descriptions = descriptions;
		}
	}
}