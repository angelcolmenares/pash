using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DsmlAuthRequest : DirectoryRequest
	{
		private string directoryPrincipal;

		public string Principal
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryPrincipal;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.directoryPrincipal = value;
			}
		}

		public DsmlAuthRequest()
		{
			this.directoryPrincipal = "";
		}

		public DsmlAuthRequest(string principal)
		{
			this.directoryPrincipal = "";
			this.directoryPrincipal = principal;
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			XmlElement xmlElement = base.CreateRequestElement(doc, "authRequest", false, null);
			XmlAttribute principal = doc.CreateAttribute("principal", null);
			principal.InnerText = this.Principal;
			xmlElement.Attributes.Append(principal);
			return xmlElement;
		}
	}
}