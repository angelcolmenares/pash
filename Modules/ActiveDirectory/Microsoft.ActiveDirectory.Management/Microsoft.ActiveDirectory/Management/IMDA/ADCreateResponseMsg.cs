using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
	internal class ADCreateResponseMsg : AdimdaResponseMsg
	{
		private IList<DirectoryControl> _controls;

		private string _objectReference;

		private string _instance;

		public virtual IList<DirectoryControl> Controls
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

		public override string ObjectReference
		{
			get
			{
				if (!base.Closed)
				{
					return this._objectReference;
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
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/CreateResponse";
			}
		}

		public ADCreateResponseMsg()
		{
			this._controls = new List<DirectoryControl>();
		}

		public ADCreateResponseMsg(Message resp) : base(resp)
		{
			this._controls = new List<DirectoryControl>();
		}

		public static ADCreateResponseMsg CreateResponse(Message response)
		{
			return AdwsResponseMsg.DeserializeResponse<ADCreateResponseMsg>(response);
		}

		protected override void OnReadBodyContents(XmlDictionaryReader reader)
		{
			base.OnReadBodyContents(reader);
			reader.ReadStartElement("ResourceCreated", "http://schemas.xmlsoap.org/ws/2004/09/transfer");
			reader.Skip();
			reader.ReadStartElement("ReferenceParameters", "http://www.w3.org/2005/08/addressing");
			XmlUtility.DeserializeObjectReference(reader, out this._objectReference);
			this._instance = reader.ReadElementString("instance", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
			reader.ReadEndElement();
			reader.ReadEndElement();
			DirectoryControlSerializer.Deserialize(reader, out this._controls, false, false);
			reader.ReadEndElement();
		}
	}
}