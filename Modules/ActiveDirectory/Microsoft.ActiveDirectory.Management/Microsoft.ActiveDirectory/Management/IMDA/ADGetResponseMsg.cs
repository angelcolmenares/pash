using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
	internal class ADGetResponseMsg : AdimdaResponseMsg
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

		public ADGetResponseMsg()
		{
			this._controls = new List<DirectoryControl>();
		}

		public ADGetResponseMsg(Message response) : base(response)
		{
			this._controls = new List<DirectoryControl>();
		}

		public static ADGetResponseMsg CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADGetResponseMsg>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			reader.ReadStartElement("BaseObjectSearchResponse", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
			ResultSerializer.Deserialize(reader, ResultSerializer.ResultDialect.XPath1, out this._entry);
			DirectoryControlSerializer.Deserialize(reader, out this._controls, false, false);
			reader.ReadEndElement();
		}
	}
}