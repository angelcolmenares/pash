using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WST
{
	internal class ADDeleteResponseMsg : AdwsResponseMsg
	{
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

		protected override string SupportedAction
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/DeleteResponse";
			}
		}

		protected override bool SupportsEmptyMessage
		{
			get
			{
				return true;
			}
		}

		public ADDeleteResponseMsg()
		{
			this._controls = new List<DirectoryControl>();
		}

		public ADDeleteResponseMsg(Message response) : base(response)
		{
			this._controls = new List<DirectoryControl>();
		}

		public static ADDeleteResponseMsg CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADDeleteResponseMsg>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			base.OnReadBodyContents(reader);
			DirectoryControlSerializer.Deserialize(reader, out this._controls, false, false);
		}
	}
}