using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class AddRequest : DirectoryRequest
	{
		private string dn;

		private DirectoryAttributeCollection attributeList;

		public DirectoryAttributeCollection Attributes
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.attributeList;
			}
		}

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

		public AddRequest()
		{
			this.attributeList = new DirectoryAttributeCollection();
		}

		public AddRequest(string distinguishedName, DirectoryAttribute[] attributes) : this()
		{
			this.dn = distinguishedName;
			if (attributes != null)
			{
				for (int i = 0; i < (int)attributes.Length; i++)
				{
					this.attributeList.Add(attributes[i]);
				}
			}
		}

		public AddRequest(string distinguishedName, string objectClass) : this()
		{
			if (objectClass != null)
			{
				this.dn = distinguishedName;
				DirectoryAttribute directoryAttribute = new DirectoryAttribute();
				directoryAttribute.Name = "objectClass";
				directoryAttribute.Add(objectClass);
				this.attributeList.Add(directoryAttribute);
				return;
			}
			else
			{
				throw new ArgumentNullException("objectClass");
			}
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			XmlElement xmlElement = base.CreateRequestElement(doc, "addRequest", true, this.dn);
			if (this.attributeList != null)
			{
				foreach (DirectoryAttribute directoryAttribute in this.attributeList)
				{
					XmlElement xmlNode = directoryAttribute.ToXmlNode(doc, "attr");
					xmlElement.AppendChild(xmlNode);
				}
			}
			return xmlElement;
		}
	}
}