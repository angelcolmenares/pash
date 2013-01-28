using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
	internal class ADCreateRequestMsg : AdimdaRequestMsg
	{
		private IList<DirectoryControl> _controls;

		private IList<DirectoryAttribute> _attributes;

		private string _relativeDistinguishedName;

		private string _parentContainer;

		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Create";
			}
		}

		public ADCreateRequestMsg(string instance, string parent, string relativeDistinguishedName, IList<DirectoryControl> controls) : base(instance, null)
		{
			this._relativeDistinguishedName = relativeDistinguishedName;
			this._parentContainer = parent;
			this._controls = controls;
		}

		public ADCreateRequestMsg(string instance, string parent, string relativeDistinguishedName, IList<DirectoryControl> controls, IList<DirectoryAttribute> attributes) : this(instance, parent, relativeDistinguishedName, controls)
		{
			this._attributes = attributes;
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			writer.WriteStartElement("AddRequest", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
			writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
			if (this._attributes != null)
			{
				foreach (DirectoryAttribute _attribute in this._attributes)
				{
					AttributeTypeAndValueSerializer.Serialize(writer, _attribute);
				}
			}
			AttributeTypeAndValueSerializer.Serialize(writer, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "relativeDistinguishedName", this._relativeDistinguishedName);
			AttributeTypeAndValueSerializer.Serialize(writer, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "container-hierarchy-parent", this._parentContainer);
			if (this._controls != null)
			{
				DirectoryControlSerializer.Serialize(writer, this._controls);
			}
			writer.WriteEndElement();
		}
	}
}