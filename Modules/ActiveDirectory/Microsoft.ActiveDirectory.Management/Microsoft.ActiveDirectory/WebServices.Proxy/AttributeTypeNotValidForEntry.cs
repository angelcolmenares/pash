using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[Serializable]
	[XmlType(Namespace="http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	public class AttributeTypeNotValidForEntry
	{
		private string attributeTypeField;

		[XmlElement(Order=0)]
		public string AttributeType
		{
			get
			{
				return this.attributeTypeField;
			}
			set
			{
				this.attributeTypeField = value;
			}
		}

		public AttributeTypeNotValidForEntry()
		{
		}
	}
}