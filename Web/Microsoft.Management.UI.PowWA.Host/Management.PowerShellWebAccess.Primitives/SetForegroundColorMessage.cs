using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class SetForegroundColorMessage : ClientMessage
	{
		public string Color
		{
			get;
			private set;
		}

		internal SetForegroundColorMessage(ConsoleColor color) : base((ClientMessageType)112)
		{
			this.Color = HtmlHelper.ToHtmlColor(color);
		}
	}
}