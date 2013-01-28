using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public abstract class DirectoryRequest : DirectoryOperation
	{
		internal DirectoryControlCollection directoryControlCollection;

		public DirectoryControlCollection Controls
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryControlCollection;
			}
		}

		public string RequestId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryRequestID;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.directoryRequestID = value;
			}
		}

		internal DirectoryRequest()
		{
			Utility.CheckOSVersion();
			this.directoryControlCollection = new DirectoryControlCollection();
		}

		internal XmlElement CreateRequestElement(XmlDocument doc, string requestName, bool includeDistinguishedName, string distinguishedName)
		{
			XmlElement xmlElement = doc.CreateElement(requestName, "urn:oasis:names:tc:DSML:2:0:core");
			if (includeDistinguishedName)
			{
				XmlAttribute xmlAttribute = doc.CreateAttribute("dn", null);
				xmlAttribute.InnerText = distinguishedName;
				xmlElement.Attributes.Append(xmlAttribute);
			}
			if (this.directoryRequestID != null)
			{
				XmlAttribute xmlAttribute1 = doc.CreateAttribute("requestID", null);
				xmlAttribute1.InnerText = this.directoryRequestID;
				xmlElement.Attributes.Append(xmlAttribute1);
			}
			if (this.directoryControlCollection != null)
			{
				foreach (DirectoryControl directoryControl in this.directoryControlCollection)
				{
					XmlElement xmlNode = directoryControl.ToXmlNode(doc);
					xmlElement.AppendChild(xmlNode);
				}
			}
			return xmlElement;
		}

		protected abstract XmlElement ToXmlNode(XmlDocument doc);

		internal XmlElement ToXmlNodeHelper(XmlDocument doc)
		{
			return this.ToXmlNode(doc);
		}
	}
}