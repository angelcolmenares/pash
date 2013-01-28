using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.Faults;
using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal class AdwsResponseMsg : AdwsMessage
	{
		private Message _response;

		private string _objectReference;

		private bool _closed;

		private bool _isFault;

		public virtual string Action
		{
			get
			{
				if (!this.Closed)
				{
					return this.Headers.Action;
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		protected bool Closed
		{
			get
			{
				return this._closed;
			}
			set
			{
				this._closed = value;
			}
		}

		public virtual AdwsFault FaultData
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override MessageHeaders Headers
		{
			get
			{
				if (!this.Closed)
				{
					return this._response.Headers;
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		public override bool IsFault
		{
			get
			{
				return this._isFault;
			}
		}

		public virtual string ObjectReference
		{
			get
			{
				if (!this.Closed)
				{
					return this._objectReference;
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		public override MessageProperties Properties
		{
			get
			{
				if (!this.Closed)
				{
					return this._response.Properties;
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		protected virtual string SupportedAction
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		protected virtual bool SupportsEmptyMessage
		{
			get
			{
				return false;
			}
		}

		public override MessageVersion Version
		{
			get
			{
				if (!this.Closed)
				{
					return this._response.Version;
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		internal AdwsResponseMsg()
		{
		}

		protected AdwsResponseMsg(Message response)
		{
			this.DeserializeMessage(response);
		}

		protected void DeserializeMessage(Message response)
		{
			this.OnReadHeaders(response.Headers);
			if (response.Headers.HaveMandatoryHeadersBeenUnderstood())
			{
				if (response.IsEmpty)
				{
					if (!this.SupportsEmptyMessage)
					{
						throw new ADException(StringResources.ADWSXmlParserEmptyMessageReceived);
					}
				}
				else
				{
					XmlDictionaryReader readerAtBodyContents = response.GetReaderAtBodyContents();
					using (readerAtBodyContents)
					{
						this.OnReadBodyContents(readerAtBodyContents);
					}
				}
				this._response = response;
				return;
			}
			else
			{
				throw new ADException(StringResources.ADWSXmlParserMandatoryHeaderNotUnderstood);
			}
		}

		protected static T DeserializeResponse<T>(Message response)
		where T : AdwsResponseMsg, new()
		{
			T t = Activator.CreateInstance<T>();
			t.DeserializeMessage(response);
			return t;
		}

		protected override void OnClose()
		{
			base.OnClose();
			this._response.Close();
			this.Closed = true;
		}

		protected virtual void OnReadBodyContents(XmlDictionaryReader reader)
		{
		}

		protected virtual void OnReadHeaders(MessageHeaders Headers)
		{
			XmlUtility.MarkHeaderAsUnderstood(Headers, "Action", "http://www.w3.org/2005/08/addressing");
			int num = Headers.FindHeader("Action", "http://www.w3.org/2005/08/addressing");
			XmlDictionaryReader readerAtHeader = Headers.GetReaderAtHeader(num);
			string str = readerAtHeader.ReadElementString("Action", "http://www.w3.org/2005/08/addressing");
			if (string.Equals(str, this.SupportedAction, StringComparison.Ordinal))
			{
				num = Headers.FindHeader("objectReferenceProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				if (num >= 0)
				{
					readerAtHeader = Headers.GetReaderAtHeader(num);
					this._objectReference = readerAtHeader.ReadElementString("objectReferenceProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
				}
				return;
			}
			else
			{
				throw new ADException(StringResources.ADWSXmlParserInvalidActionForMessage);
			}
		}

		public override string ToString(bool indent)
		{
			if (!this.Closed)
			{
				return AdwsMessage.MessageToString(this._response, indent);
			}
			else
			{
				throw new ObjectDisposedException("AdwsMessage");
			}
		}
	}
}