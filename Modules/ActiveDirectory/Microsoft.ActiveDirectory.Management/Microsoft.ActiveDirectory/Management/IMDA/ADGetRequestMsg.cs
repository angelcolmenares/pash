using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
	internal class ADGetRequestMsg : AdimdaRequestMsg
	{
		private IList<DirectoryControl> _controls;

		private IList<string> _attributeList;

		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";
			}
		}

		public ADGetRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, IList<string> attributeList) : base(instance, objectReference)
		{
			if (attributeList == null || attributeList.Count == 0)
			{
				throw new ArgumentOutOfRangeException("attributeList");
			}
			else
			{
				this._controls = controls;
				this._attributeList = attributeList;
				return;
			}
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("BaseObjectSearchRequest", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
			writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
			base.OnWriteBodyContents(writer);
			string str = writer.LookupPrefix("http://schemas.microsoft.com/2008/1/ActiveDirectory/Data");
			string str1 = writer.LookupPrefix("http://schemas.microsoft.com/2008/1/ActiveDirectory");
			if (this._attributeList != null)
			{
				XmlUtility.SerializeAttributeList(writer, "AttributeType", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess", str1, str, this._attributeList);
			}
			if (this._controls != null)
			{
				DirectoryControlSerializer.Serialize(writer, this._controls);
			}
			writer.WriteEndElement();
		}
	}
}