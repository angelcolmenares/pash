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
	[XmlRoot(Namespace="http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	[XmlType(Namespace="http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
	public class FragmentDialect
	{
		private string valueField;

		[XmlText(DataType="anyURI")]
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

		public FragmentDialect()
		{
		}
	}
}