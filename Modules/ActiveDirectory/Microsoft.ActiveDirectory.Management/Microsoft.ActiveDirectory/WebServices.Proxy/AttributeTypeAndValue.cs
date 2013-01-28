using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[Serializable]
	[XmlInclude(typeof(ChangeType))]
	[XmlType(Namespace="http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
	public class AttributeTypeAndValue
	{
		private DataSet attributeTypeField;

		private DataSet attributeValueField;

		[XmlElement(Order=0)]
		public DataSet AttributeType
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

		[XmlElement(Order=1)]
		public DataSet AttributeValue
		{
			get
			{
				return this.attributeValueField;
			}
			set
			{
				this.attributeValueField = value;
			}
		}

		public AttributeTypeAndValue()
		{
		}
	}
}