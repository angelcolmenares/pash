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
	[XmlType(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	public class ArgumentErrorDetail
	{
		private string messageField;

		private string parameterNameField;

		private string shortMessageField;

		[XmlElement(Order=0)]
		public string Message
		{
			get
			{
				return this.messageField;
			}
			set
			{
				this.messageField = value;
			}
		}

		[XmlElement(Order=1)]
		public string ParameterName
		{
			get
			{
				return this.parameterNameField;
			}
			set
			{
				this.parameterNameField = value;
			}
		}

		[XmlElement(Order=2)]
		public string ShortMessage
		{
			get
			{
				return this.shortMessageField;
			}
			set
			{
				this.shortMessageField = value;
			}
		}

		public ArgumentErrorDetail()
		{
		}
	}
}