using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal abstract class AdwsRequestMsg : AdwsMessage
	{
		private const int _initialHeaderBufferSize = 7;

		private MessageHeaders _messageHeaders;

		private MessageProperties _messageProperties;

		public abstract string Action
		{
			get;
		}

		public override MessageHeaders Headers
		{
			get
			{
				return this._messageHeaders;
			}
		}

		public override MessageProperties Properties
		{
			get
			{
				return this._messageProperties;
			}
		}

		public override MessageVersion Version
		{
			get
			{
				return MessageVersion.Soap12WSAddressing10;
			}
		}

		private AdwsRequestMsg()
		{
			this._messageProperties = new MessageProperties();
		}

		protected AdwsRequestMsg(string instance)
		{
			this._messageProperties = new MessageProperties();
			this._messageHeaders = new MessageHeaders(this.Version, 7);
			this.Headers.Action = this.Action;
			this.Headers.Add(MessageHeader.CreateHeader("instance", "http://schemas.microsoft.com/2008/1/ActiveDirectory", instance));
		}

		protected AdwsRequestMsg(string instance, string objectReferenceProperty) : this(instance)
		{
			if (!string.IsNullOrEmpty(objectReferenceProperty))
			{
				this.Headers.Add(MessageHeader.CreateHeader("objectReferenceProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory", objectReferenceProperty));
			}
		}

		protected virtual void AddPrefixIfNeeded(XmlDictionaryWriter writer, string prefix, string ns)
		{
			if (writer.LookupPrefix(ns) == null)
			{
				writer.WriteXmlnsAttribute(prefix, ns);
			}
		}

		protected override void OnWriteStartEnvelope(XmlDictionaryWriter writer)
		{
			base.OnWriteStartEnvelope(writer);
			writer.WriteXmlnsAttribute("addata", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data");
			writer.WriteXmlnsAttribute("ad", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
			writer.WriteXmlnsAttribute("xsd", "http://www.w3.org/2001/XMLSchema");
			writer.WriteXmlnsAttribute("xsi", "http://www.w3.org/2001/XMLSchema-instance");
			if (writer.LookupPrefix("http://www.w3.org/2005/08/addressing") == null)
			{
				writer.WriteXmlnsAttribute("wsa", "http://www.w3.org/2005/08/addressing");
			}
		}

		protected override void OnWriteStartHeaders(XmlDictionaryWriter writer)
		{
			base.OnWriteStartHeaders(writer);
		}
	}
}