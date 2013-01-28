using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class SetBackgroundColorMessage : ClientMessage
	{
		public string Color
		{
			get;
			private set;
		}

		internal SetBackgroundColorMessage(ConsoleColor color) : base((ClientMessageType)110)
		{
			this.Color = HtmlHelper.ToHtmlColor(color);
		}
	}
}