using Microsoft.ActiveDirectory.Management;
using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal abstract class ADEnumerateResponse : AdwsResponseMsg
	{
		private DateTime? _expirationDateTime;

		private TimeSpan? _expirationDuration;

		private string _enumerationContext;

		public object ContextExpiration
		{
			get
			{
				if (!base.Closed)
				{
					if (!this._expirationDuration.HasValue)
					{
						if (!this._expirationDateTime.HasValue)
						{
							return null;
						}
						else
						{
							return this._expirationDateTime.Value;
						}
					}
					else
					{
						return this._expirationDuration.Value;
					}
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		public string EnumerationContext
		{
			get
			{
				if (!base.Closed)
				{
					return this._enumerationContext;
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		protected override string SupportedAction
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/EnumerateResponse";
			}
		}

		public ADEnumerateResponse()
		{
		}

		protected ADEnumerateResponse(Message response) : base(response)
		{
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			reader.ReadStartElement("EnumerateResponse", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.DeserializeExpiresIfNeeded(reader, ref this._expirationDateTime, ref this._expirationDuration);
			this._enumerationContext = XmlUtility.DeserializeEunmerationContext(reader);
		}
	}
}