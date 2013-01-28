using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
	internal class ADPutRequestMsg : AdimdaRequestMsg
	{
		private IList<DirectoryControl> _controls;

		private IList<DirectoryAttributeModification> _attributeMods;

		private string _relativeDistinguishedName;

		private string _parent;

		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Put";
			}
		}

		private ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls) : base(instance, objectReference)
		{
			this._controls = controls;
		}

		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, IList<DirectoryAttributeModification> attributeModifications) : this(instance, objectReference, controls)
		{
			this._attributeMods = attributeModifications;
		}

		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, string relativeDistinguishedName) : this(instance, objectReference, controls)
		{
			this._relativeDistinguishedName = relativeDistinguishedName;
		}

		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, string relativeDistinguishedName, string parent) : this(instance, objectReference, controls)
		{
			this._relativeDistinguishedName = relativeDistinguishedName;
			this._parent = parent;
		}

		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, string relativeDistinguishedName, string parent, IList<DirectoryAttributeModification> attributeModifications) : this(instance, objectReference, controls)
		{
			this._relativeDistinguishedName = relativeDistinguishedName;
			this._parent = parent;
			this._attributeMods = attributeModifications;
		}

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			base.OnWriteBodyContents(writer);
			writer.WriteStartElement("ModifyRequest", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
			writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
			if (this._attributeMods != null)
			{
				foreach (DirectoryAttributeModification _attributeMod in this._attributeMods)
				{
					AttributeTypeAndValueSerializer.Serialize(writer, _attributeMod);
				}
			}
			if (this._parent != null)
			{
				AttributeTypeAndValueSerializer.Serialize(writer, ChangeOperation.Replace, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "container-hierarchy-parent", this._parent);
			}
			if (this._relativeDistinguishedName != null)
			{
				AttributeTypeAndValueSerializer.Serialize(writer, ChangeOperation.Replace, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "relativeDistinguishedName", this._relativeDistinguishedName);
			}
			DirectoryControlSerializer.Serialize(writer, this._controls);
			writer.WriteEndElement();
		}
	}
}