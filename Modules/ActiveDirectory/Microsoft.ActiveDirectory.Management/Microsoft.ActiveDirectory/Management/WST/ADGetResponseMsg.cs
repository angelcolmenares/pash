using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WST
{
	internal class ADGetResponseMsg : AdwsResponseMsg
	{
		private ADWSResultEntry _entry;

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

		public ADWSResultEntry Entry
		{
			get
			{
				if (!base.Closed)
				{
					return this._entry;
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
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse";
			}
		}

		protected override bool SupportsEmptyMessage
		{
			get
			{
				return true;
			}
		}

		public ADGetResponseMsg()
		{
			this._controls = new List<DirectoryControl>();
		}

		public ADGetResponseMsg(Message response) : base(response)
		{
			this._controls = new List<DirectoryControl>();
		}

		internal static ADGetResponseMsg CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADGetResponseMsg>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			base.OnReadBodyContents(reader);
			XmlReader xmlReader = reader.ReadSubtree();
			using (xmlReader)
			{
				xmlReader.Read();
				ResultSerializer.Deserialize(xmlReader, ResultSerializer.ResultDialect.WSTransfer, out this._entry);
			}
			reader.Read();
			DirectoryControlSerializer.Deserialize(reader, out this._controls, false, false);
			reader.ReadEndElement();
		}
	}
}