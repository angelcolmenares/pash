using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class WriteLineMessage : ClientMessage
	{
		public string BackgroundColor
		{
			get;
			private set;
		}

		public string ForegroundColor
		{
			get;
			private set;
		}

		public string Value
		{
			get;
			private set;
		}

		internal WriteLineMessage(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value) : this(HtmlHelper.ToHtmlColor(foregroundColor), HtmlHelper.ToHtmlColor(backgroundColor), value)
		{
		}

		internal WriteLineMessage(string foregroundColor, string backgroundColor, string value) : base((ClientMessageType)107)
		{
			this.ForegroundColor = foregroundColor;
			this.BackgroundColor = backgroundColor;
			this.Value = value;
		}
	}
}