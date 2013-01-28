using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WSE
{
	internal class ADPullResponse : AdwsResponseMsg
	{
		private List<ADWSResultEntry> _results;

		private string _enumerationContext;

		private IList<DirectoryControl> _controls;

		public IList<DirectoryControl> Controls
		{
			get
			{
				if (!base.Closed)
				{
					return this._controls;
				}
				else
				{
					throw new ObjectDisposedException("AdwsMessage");
				}
			}
		}

		public virtual string EnumerationContext
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

		public virtual IList<ADWSResultEntry> Results
		{
			get
			{
				if (!base.Closed)
				{
					return this._results;
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
				return "http://schemas.xmlsoap.org/ws/2004/09/enumeration/PullResponse";
			}
		}

		public ADPullResponse()
		{
			this._results = new List<ADWSResultEntry>();
			this._controls = new List<DirectoryControl>();
		}

		public ADPullResponse(Message response) : base(response)
		{
			this._results = new List<ADWSResultEntry>();
			this._controls = new List<DirectoryControl>();
		}

		public static ADPullResponse CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADPullResponse>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			ADWSResultEntry aDWSResultEntry = null;
			reader.ReadStartElement("PullResponse", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			this._enumerationContext = XmlUtility.DeserializeEunmerationContext(reader);
			if (reader.IsStartElement("Items", "http://schemas.xmlsoap.org/ws/2004/09/enumeration"))
			{
				reader.Read();
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					ResultSerializer.Deserialize(reader, ResultSerializer.ResultDialect.WSEnumeration, out aDWSResultEntry);
					this._results.Add(aDWSResultEntry);
				}
				reader.ReadEndElement();
			}
			DirectoryControlSerializer.Deserialize(reader, out this._controls, false, false);
		}
	}
}