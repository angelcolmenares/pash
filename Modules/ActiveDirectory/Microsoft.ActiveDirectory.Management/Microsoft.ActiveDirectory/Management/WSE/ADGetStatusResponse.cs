using Microsoft.ActiveDirectory.Management;
using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADGetStatusResponse : AdwsResponseMsg
	{
		private DateTime? _expirationDateTime;

		private TimeSpan? _expirationTimeSpan;

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

		protected override string SupportedAction
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatusResponse";
			}
		}

		protected override bool SupportsEmptyMessage
		{
			get
			{
				return true;
			}
		}

		public ADGetStatusResponse()
		{
			this._expirationDateTime = null;
			this._expirationTimeSpan = null;
		}

		public ADGetStatusResponse(Message response) : base(response)
		{
			this._expirationDateTime = null;
			this._expirationTimeSpan = null;
		}

		public static ADGetStatusResponse CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADGetStatusResponse>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			reader.ReadStartElement("GetStatusResponse", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			XmlUtility.DeserializeExpiresIfNeeded(reader, ref this._expirationDateTime, ref this._expirationTimeSpan);
		}
	}
}