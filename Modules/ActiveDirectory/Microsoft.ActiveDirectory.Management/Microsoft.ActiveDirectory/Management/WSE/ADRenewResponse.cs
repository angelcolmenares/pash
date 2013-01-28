using Microsoft.ActiveDirectory.Management;
using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADRenewResponse : AdwsResponseMsg
	{
		private DateTime? _expirationDateTime;

		private TimeSpan? _expirationTimeSpan;

		private string _enumerationContext;

		public object ContextExpiration
		{
			get
			{
				if (!base.Closed)
				{
					if (!this._expirationTimeSpan.HasValue)
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
						return this._expirationTimeSpan.Value;
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
				return this._enumerationContext;
			}
		}

		protected override string SupportedAction
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/RenewResponse";
			}
		}

		protected override bool SupportsEmptyMessage
		{
			get
			{
				return true;
			}
		}

		public ADRenewResponse()
		{
			this._expirationDateTime = null;
			this._expirationTimeSpan = null;
		}

		public ADRenewResponse(Message response) : base(response)
		{
			this._expirationDateTime = null;
			this._expirationTimeSpan = null;
		}

		public static ADRenewResponse CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADRenewResponse>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			reader.ReadStartElement("RenewResponse", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.DeserializeExpiresIfNeeded(reader, ref this._expirationDateTime, ref this._expirationTimeSpan);
			this._enumerationContext = XmlUtility.DeserializeEunmerationContext(reader);
		}
	}
}