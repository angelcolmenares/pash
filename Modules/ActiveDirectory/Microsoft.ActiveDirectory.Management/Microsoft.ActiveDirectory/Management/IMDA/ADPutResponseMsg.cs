using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
	internal class ADPutResponseMsg : AdimdaResponseMsg
	{
		private IList<DirectoryControl> _controls;

		public virtual IList<DirectoryControl> Controls
		{
			get
			{
				return this._controls;
			}
		}

		protected override string SupportedAction
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/PutResponse";
			}
		}

		protected override bool SupportsEmptyMessage
		{
			get
			{
				return true;
			}
		}

		public ADPutResponseMsg()
		{
			this._controls = new List<DirectoryControl>();
		}

		public ADPutResponseMsg(Message response) : base(response)
		{
			this._controls = new List<DirectoryControl>();
		}

		public static ADPutResponseMsg CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADPutResponseMsg>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			base.OnReadBodyContents(reader);
			DirectoryControlSerializer.Deserialize(reader, out this._controls, false, false);
		}
	}
}