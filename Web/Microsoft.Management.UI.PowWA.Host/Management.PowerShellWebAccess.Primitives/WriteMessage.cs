using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class WriteMessage : ClientMessage
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

		internal WriteMessage(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value) : base((ClientMessageType)106)
		{
			this.ForegroundColor = HtmlHelper.ToHtmlColor(foregroundColor);
			this.BackgroundColor = HtmlHelper.ToHtmlColor(backgroundColor);
			this.Value = value;
		}

		internal void Append(string value)
		{
			WriteMessage writeMessage = this;
			writeMessage.Value = string.Concat(writeMessage.Value, value);
			base.ResetJsonSerializationLength();
		}
	}
}