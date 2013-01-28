using System;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DeleteRequest : DirectoryRequest
	{
		private string dn;

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

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DeleteRequest()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DeleteRequest(string distinguishedName)
		{
			this.dn = distinguishedName;
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			XmlElement xmlElement = base.CreateRequestElement(doc, "delRequest", true, this.dn);
			return xmlElement;
		}
	}
}