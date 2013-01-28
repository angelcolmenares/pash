using System;
using System.Management.Automation.Host;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class SetWindowSizeMessage : ClientMessage
	{
		[CLSCompliant(false)]
		public Size Size
		{
			get;
			private set;
		}

		internal SetWindowSizeMessage(Size size) : base((ClientMessageType)113)
		{
			this.Size = size;
		}
	}
}