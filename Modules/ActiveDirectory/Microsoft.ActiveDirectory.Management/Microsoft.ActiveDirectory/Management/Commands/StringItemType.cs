using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "4.0.30319.1")]
	[Serializable]
	[XmlType(Namespace="http://schemas.microsoft.com/2010/08/ActiveDirectory/PossibleValues")]
	public class StringItemType : ClaimValueItemBaseType
	{
		private string valueField;

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

		public StringItemType()
		{
		}
	}
}