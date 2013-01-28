using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[GeneratedCode("svcutil", "3.0.4506.2123")]
	[Serializable]
	[XmlRoot(Namespace="http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	[XmlType(TypeName="FaultDetail", Namespace="http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	public class FaultDetail1
	{
		private int sizeLimitField;

		private bool sizeLimitFieldSpecified;

		private string valueField;

		private XmlSerializerNamespaces xmlnsField;

		[XmlAttribute(Form=XmlSchemaForm.Qualified, Namespace="http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
		public int SizeLimit
		{
			get
			{
				return this.sizeLimitField;
			}
			set
			{
				this.sizeLimitField = value;
			}
		}

		[XmlIgnore]
		public bool SizeLimitSpecified
		{
			get
			{
				return this.sizeLimitFieldSpecified;
			}
			set
			{
				this.sizeLimitFieldSpecified = value;
			}
		}

		[XmlText]
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}

		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces xmlns
		{
			get
			{
				return this.xmlnsField;
			}
			set
			{
				this.xmlnsField = value;
			}
		}

		public FaultDetail1()
		{
		}
	}
}