using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class ExtendedRequest : DirectoryRequest
	{
		private string requestName;

		private byte[] requestValue;

		public string RequestName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.requestName;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.requestName = value;
			}
		}

		public byte[] RequestValue
		{
			get
			{
				if (this.requestValue != null)
				{
					byte[] numArray = new byte[(int)this.requestValue.Length];
					for (int i = 0; i < (int)this.requestValue.Length; i++)
					{
						numArray[i] = this.requestValue[i];
					}
					return numArray;
				}
				else
				{
					return new byte[0];
				}
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.requestValue = value;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ExtendedRequest()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ExtendedRequest(string requestName)
		{
			this.requestName = requestName;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ExtendedRequest(string requestName, byte[] requestValue) : this(requestName)
		{
			this.requestValue = requestValue;
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			XmlElement xmlElement = base.CreateRequestElement(doc, "extendedRequest", false, null);
			XmlElement xmlElement1 = doc.CreateElement("requestName", "urn:oasis:names:tc:DSML:2:0:core");
			xmlElement1.InnerText = this.requestName;
			xmlElement.AppendChild(xmlElement1);
			if (this.requestValue != null)
			{
				XmlElement base64String = doc.CreateElement("requestValue", "urn:oasis:names:tc:DSML:2:0:core");
				base64String.InnerText = Convert.ToBase64String(this.requestValue);
				XmlAttribute xmlAttribute = doc.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
				xmlAttribute.InnerText = "xsd:base64Binary";
				base64String.Attributes.Append(xmlAttribute);
				xmlElement.AppendChild(base64String);
			}
			return xmlElement;
		}
	}
}