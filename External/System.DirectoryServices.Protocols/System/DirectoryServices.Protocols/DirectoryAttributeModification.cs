using System.ComponentModel;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DirectoryAttributeModification : DirectoryAttribute
	{
		private DirectoryAttributeOperation attributeOperation;

		public DirectoryAttributeOperation Operation
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.attributeOperation;
			}
			set
			{
				if (value < DirectoryAttributeOperation.Add || value > DirectoryAttributeOperation.Replace)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DirectoryAttributeOperation));
				}
				else
				{
					this.attributeOperation = value;
					return;
				}
			}
		}

		public DirectoryAttributeModification()
		{
			this.attributeOperation = DirectoryAttributeOperation.Replace;
		}

		internal XmlElement ToXmlNode(XmlDocument doc)
		{
			XmlElement xmlElement = doc.CreateElement("modification", "urn:oasis:names:tc:DSML:2:0:core");
			base.ToXmlNodeCommon(xmlElement);
			XmlAttribute xmlAttribute = doc.CreateAttribute("operation", null);
			DirectoryAttributeOperation operation = this.Operation;
			if (operation == DirectoryAttributeOperation.Add)
			{
				xmlAttribute.InnerText = "add";
			}
			else if (operation == DirectoryAttributeOperation.Delete)
			{
				xmlAttribute.InnerText = "delete";
			}
			else if (operation == DirectoryAttributeOperation.Replace)
			{
				xmlAttribute.InnerText = "replace";
			}
			else
			{
				throw new InvalidEnumArgumentException("Operation", (int)this.Operation, typeof(DirectoryAttributeOperation));
			}
			xmlElement.Attributes.Append(xmlAttribute);
			return xmlElement;
		}
	}
}