using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.WST
{
	internal class ADGetRequestMsg : AdwsRequestMsg
	{
		private IList<DirectoryControl> _controls;

		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";
			}
		}

		public ADGetRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls) : base(instance, objectReference)
		{
			this._controls = controls;
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			base.OnWriteBodyContents(writer);
			if (this._controls != null)
			{
				DirectoryControlSerializer.Serialize(writer, this._controls);
			}
		}
	}
}