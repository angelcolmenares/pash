using System;
using System.IO;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.PowerShell
{
	internal class WrappedDeserializer : Serialization
	{
		internal TextReader textReader;

		private XmlReader xmlReader;

		private Deserializer xmlDeserializer;

		private string firstLine;

		private bool atEnd;

		internal bool AtEnd
		{
			get
			{
				bool flag = false;
				Serialization.DataFormat dataFormat = this.format;
				switch (dataFormat)
				{
					case Serialization.DataFormat.Text:
					{
						flag = this.atEnd;
						break;
					}
					case Serialization.DataFormat.XML:
					{
						flag = this.xmlDeserializer.Done();
						break;
					}
					case Serialization.DataFormat.None:
					{
						this.atEnd = true;
						flag = true;
						break;
					}
					default:
					{
                        flag = this.atEnd;
                        break;
					}
				}
				return flag;
			}
		}

		internal WrappedDeserializer(Serialization.DataFormat dataFormat, string streamName, TextReader input) : base(dataFormat, streamName)
		{
			if (dataFormat != Serialization.DataFormat.None)
			{
				this.textReader = input;
				this.firstLine = this.textReader.ReadLine();
				if (string.Compare(this.firstLine, Serialization.XmlCliTag, StringComparison.OrdinalIgnoreCase) == 0)
				{
					dataFormat = Serialization.DataFormat.XML;
				}
				Serialization.DataFormat dataFormat1 = this.format;
				switch (dataFormat1)
				{
					case Serialization.DataFormat.Text:
					{
						return;
					}
					case Serialization.DataFormat.XML:
					{
						this.xmlReader = XmlReader.Create(this.textReader);
						this.xmlDeserializer = new Deserializer(this.xmlReader);
						return;
					}
					default:
					{
						return;
					}
				}
			}
			else
			{
				return;
			}
		}

		internal object Deserialize()
		{
			object obj;
			string str = null;
			Serialization.DataFormat dataFormat = this.format;
			if (dataFormat == Serialization.DataFormat.Text)
			{
			}
			else if (dataFormat == Serialization.DataFormat.XML)
			{
				obj = this.xmlDeserializer.Deserialize(out str);
				return obj;
			}
			else if (dataFormat == Serialization.DataFormat.None)
			{
				this.atEnd = true;
				return null;
			}
			if (!this.atEnd)
			{
				if (this.firstLine == null)
				{
					obj = this.textReader.ReadLine();
					if (obj == null)
					{
						this.atEnd = true;
					}
				}
				else
				{
					obj = this.firstLine;
					this.firstLine = null;
				}
			}
			else
			{
				return null;
			}
			return obj;
		}

		internal void End()
		{
			Serialization.DataFormat dataFormat = this.format;
			switch (dataFormat)
			{
				default:
				{
					return;
				}
			}
		}
	}
}