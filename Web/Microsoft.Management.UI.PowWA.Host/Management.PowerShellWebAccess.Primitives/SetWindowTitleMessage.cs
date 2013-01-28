using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class SetWindowTitleMessage : ClientMessage
	{
		public string Title
		{
			get;
			private set;
		}

		internal SetWindowTitleMessage(string tile) : base((ClientMessageType)114)
		{
			this.Title = tile;
		}
	}
}