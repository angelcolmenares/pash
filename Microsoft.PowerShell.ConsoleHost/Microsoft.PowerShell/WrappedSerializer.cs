using System;
using System.IO;
using System.Management.Automation;
using System.Xml;

namespace Microsoft.PowerShell
{
	internal class WrappedSerializer : Serialization
	{
		internal TextWriter textWriter;

		private XmlWriter xmlWriter;

		private Serializer xmlSerializer;

		private bool firstCall;

		internal WrappedSerializer(Serialization.DataFormat dataFormat, string streamName, TextWriter output) : base(dataFormat, streamName)
		{
			this.firstCall = true;
			this.textWriter = output;
			Serialization.DataFormat dataFormat1 = this.format;
			switch (dataFormat1)
			{
				case Serialization.DataFormat.Text:
				{
					return;
				}
				case Serialization.DataFormat.XML:
				{
					XmlWriterSettings xmlWriterSetting = new XmlWriterSettings();
					xmlWriterSetting.CheckCharacters = false;
					xmlWriterSetting.OmitXmlDeclaration = true;
					this.xmlWriter = XmlWriter.Create(this.textWriter, xmlWriterSetting);
					this.xmlSerializer = new Serializer(this.xmlWriter);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal void End()
		{
			Serialization.DataFormat dataFormat = this.format;
			switch (dataFormat)
			{
				case Serialization.DataFormat.Text:
				case Serialization.DataFormat.None:
				{
					return;
				}
				case Serialization.DataFormat.XML:
				{
					this.xmlSerializer.Done();
					this.xmlSerializer = null;
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal void Serialize(object o)
		{
			this.Serialize(o, this.streamName);
		}

		internal void Serialize(object o, string streamName)
		{
			Serialization.DataFormat dataFormat = this.format;
			switch (dataFormat)
			{
				case Serialization.DataFormat.Text:
				{
					this.textWriter.Write(o.ToString());
					return;
				}
				case Serialization.DataFormat.XML:
				{
					if (this.firstCall)
					{
						this.firstCall = false;
						this.textWriter.WriteLine(Serialization.XmlCliTag);
					}
					this.xmlSerializer.Serialize(o, streamName);
					return;
				}
				case Serialization.DataFormat.None:
				{
					return;
				}
				default:
				{
					this.textWriter.Write(o.ToString());
					return;
				}
			}
		}
	}
}