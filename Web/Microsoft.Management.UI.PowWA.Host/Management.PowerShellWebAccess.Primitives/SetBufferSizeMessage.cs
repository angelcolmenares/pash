using System;
using System.Management.Automation.Host;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class SetBufferSizeMessage : ClientMessage
	{
		[CLSCompliant(false)]
		public Size Size
		{
			get;
			private set;
		}

		internal SetBufferSizeMessage(Size size) : base((ClientMessageType)111)
		{
			this.Size = size;
		}
	}
}