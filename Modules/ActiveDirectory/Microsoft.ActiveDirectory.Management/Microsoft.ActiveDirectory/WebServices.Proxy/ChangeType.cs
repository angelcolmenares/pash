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
	public class ChangeType : AttributeTypeAndValue
	{
		private string operationField;

		[XmlAttribute]
		public string Operation
		{
			get
			{
				return this.operationField;
			}
			set
			{
				this.operationField = value;
			}
		}

		public ChangeType()
		{
		}
	}
}