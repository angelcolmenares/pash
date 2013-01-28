using System;

namespace Microsoft.PowerShell
{
	internal class Serialization
	{
		protected static string XmlCliTag;

		protected string streamName;

		protected Serialization.DataFormat format;

		static Serialization()
		{
			Serialization.XmlCliTag = "#< CLIXML";
		}

		protected Serialization(Serialization.DataFormat dataFormat, string streamName)
		{
			this.format = dataFormat;
			this.streamName = streamName;
		}

		internal enum DataFormat
		{
			Text,
			XML,
			None
		}
	}
}