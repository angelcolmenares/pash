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
	[XmlRoot(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[XmlType(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	public class EnumerateFault : FaultDetail
	{
		private string invalidPropertyField;

		[XmlElement(Order=0)]
		public string InvalidProperty
		{
			get
			{
				return this.invalidPropertyField;
			}
			set
			{
				this.invalidPropertyField = value;
			}
		}

		public EnumerateFault()
		{
		}
	}
}