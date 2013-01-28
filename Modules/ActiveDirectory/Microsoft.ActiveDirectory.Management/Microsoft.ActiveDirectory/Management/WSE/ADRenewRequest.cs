using Microsoft.ActiveDirectory.Management;
using System;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADRenewRequest : AdwsRequestMsg
	{
		private string _enumerationContext;

		private DateTime? _expirationDateTime;

		private TimeSpan? _expirationTimeSpan;

		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Renew";
			}
		}

		public ADRenewRequest(string instance, string enumerationContext) : base(instance)
		{
			this._expirationDateTime = null;
			this._expirationTimeSpan = null;
			this._enumerationContext = enumerationContext;
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("Renew", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.SerializeEnumerationContext(writer, this._enumerationContext);
			XmlUtility.SerializeExpires(writer, this._expirationDateTime, this._expirationTimeSpan);
			writer.WriteEndElement();
		}

		protected override void OnWriteStartBody(XmlDictionaryWriter writer)
		{
			base.OnWriteStartBody(writer);
			writer.WriteXmlnsAttribute("wsen", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
		}

		public virtual void SetContextExpiration(DateTime expiration)
		{
			this._expirationDateTime = new DateTime?(expiration);
			this._expirationTimeSpan = null;
		}

		public virtual void SetContextExpiration(TimeSpan expiration)
		{
			this._expirationTimeSpan = new TimeSpan?(expiration);
			this._expirationDateTime = null;
		}
	}
}