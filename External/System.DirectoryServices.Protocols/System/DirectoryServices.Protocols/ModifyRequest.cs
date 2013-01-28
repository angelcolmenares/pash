using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class ModifyRequest : DirectoryRequest
	{
		private string dn;

		private DirectoryAttributeModificationCollection attributeModificationList;

		public string DistinguishedName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dn;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dn = value;
			}
		}

		public DirectoryAttributeModificationCollection Modifications
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.attributeModificationList;
			}
		}

		public ModifyRequest()
		{
			this.attributeModificationList = new DirectoryAttributeModificationCollection();
		}

		public ModifyRequest(string distinguishedName, DirectoryAttributeModification[] modifications) : this()
		{
			this.dn = distinguishedName;
			this.attributeModificationList.AddRange(modifications);
		}

		public ModifyRequest(string distinguishedName, DirectoryAttributeOperation operation, string attributeName, object[] values) : this()
		{
			this.dn = distinguishedName;
			if (attributeName != null)
			{
				DirectoryAttributeModification directoryAttributeModification = new DirectoryAttributeModification();
				directoryAttributeModification.Operation = operation;
				directoryAttributeModification.Name = attributeName;
				if (values != null)
				{
					for (int i = 0; i < (int)values.Length; i++)
					{
						directoryAttributeModification.Add(values[i]);
					}
				}
				this.attributeModificationList.Add(directoryAttributeModification);
				return;
			}
			else
			{
				throw new ArgumentNullException("attributeName");
			}
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			XmlElement xmlElement = base.CreateRequestElement(doc, "modifyRequest", true, this.dn);
			if (this.attributeModificationList != null)
			{
				foreach (DirectoryAttributeModification directoryAttributeModification in this.attributeModificationList)
				{
					XmlElement xmlNode = directoryAttributeModification.ToXmlNode(doc);
					xmlElement.AppendChild(xmlNode);
				}
			}
			return xmlElement;
		}
	}
}