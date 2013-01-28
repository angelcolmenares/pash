using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class ModifyDNRequest : DirectoryRequest
	{
		private string dn;

		private string newSuperior;

		private string newRDN;

		private bool deleteOldRDN;

		public bool DeleteOldRdn
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.deleteOldRDN;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.deleteOldRDN = value;
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

		public string NewName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.newRDN;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.newRDN = value;
			}
		}

		public string NewParentDistinguishedName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.newSuperior;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.newSuperior = value;
			}
		}

		public ModifyDNRequest()
		{
			this.deleteOldRDN = true;
		}

		public ModifyDNRequest(string distinguishedName, string newParentDistinguishedName, string newName)
		{
			this.deleteOldRDN = true;
			this.dn = distinguishedName;
			this.newSuperior = newParentDistinguishedName;
			this.newRDN = newName;
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			string str;
			XmlElement xmlElement = base.CreateRequestElement(doc, "modDNRequest", true, this.dn);
			XmlAttribute xmlAttribute = doc.CreateAttribute("newrdn", null);
			xmlAttribute.InnerText = this.newRDN;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute1 = doc.CreateAttribute("deleteoldrdn", null);
			XmlAttribute xmlAttribute2 = xmlAttribute1;
			if (this.deleteOldRDN)
			{
				str = "true";
			}
			else
			{
				str = "false";
			}
			xmlAttribute2.InnerText = str;
			xmlElement.Attributes.Append(xmlAttribute1);
			if (this.newSuperior != null)
			{
				XmlAttribute xmlAttribute3 = doc.CreateAttribute("newSuperior", null);
				xmlAttribute3.InnerText = this.newSuperior;
				xmlElement.Attributes.Append(xmlAttribute3);
			}
			return xmlElement;
		}
	}
}