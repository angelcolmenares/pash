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
	[XmlRoot(Namespace="http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	[XmlType(Namespace="http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	public class AttributeTypeNotValid
	{
		private AttributeTypeNotValidForDialect attributeTypeNotValidForDialectField;

		private AttributeTypeNotValidForEntry attributeTypeNotValidForEntryField;

		[XmlElement(Order=0)]
		public AttributeTypeNotValidForDialect AttributeTypeNotValidForDialect
		{
			get
			{
				return this.attributeTypeNotValidForDialectField;
			}
			set
			{
				this.attributeTypeNotValidForDialectField = value;
			}
		}

		[XmlElement(Order=1)]
		public AttributeTypeNotValidForEntry AttributeTypeNotValidForEntry
		{
			get
			{
				return this.attributeTypeNotValidForEntryField;
			}
			set
			{
				this.attributeTypeNotValidForEntryField = value;
			}
		}

		public AttributeTypeNotValid()
		{
		}
	}
}