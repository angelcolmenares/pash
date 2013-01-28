using System;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal abstract class AdwsMessage : Message
	{
		protected AdwsMessage()
		{
		}

		private AdwsMessage(Message msg)
		{
		}

		internal static Message BufferToMessage(MessageBuffer buffer)
		{
			return buffer.CreateMessage();
		}

		internal static MessageBuffer MessageToBuffer(Message msg)
		{
			return msg.CreateBufferedCopy(0x7fffffff);
		}

		internal static string MessageToString(Message msg, bool indent)
		{
			MessageBuffer messageBuffer = msg.CreateBufferedCopy(0x500000);
			XmlWriterSettings xmlWriterSetting = new XmlWriterSettings();
			xmlWriterSetting.OmitXmlDeclaration = true;
			xmlWriterSetting.ConformanceLevel = ConformanceLevel.Fragment;
			xmlWriterSetting.Encoding = Encoding.UTF8;
			if (indent)
			{
				xmlWriterSetting.Indent = true;
				xmlWriterSetting.IndentChars = "    ";
				xmlWriterSetting.NewLineOnAttributes = false;
			}
			StringBuilder stringBuilder = new StringBuilder();
			XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSetting);
			XmlDictionaryWriter xmlDictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);
			messageBuffer.CreateMessage().WriteMessage(xmlDictionaryWriter);
			xmlWriter.Flush();
			xmlDictionaryWriter.Close();
			return stringBuilder.ToString();
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
		}

		protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
		{
			base.OnWriteStartEnvelope(writer);
		}

		protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
		{
			base.OnWriteStartHeaders(writer);
		}

		public virtual string ToString(bool indent)
		{
			return AdwsMessage.MessageToString(this, indent);
		}
	}
}