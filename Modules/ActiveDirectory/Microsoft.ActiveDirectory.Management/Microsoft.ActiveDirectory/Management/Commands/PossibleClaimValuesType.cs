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
	[XmlRoot("PossibleClaimValues", Namespace="http://schemas.microsoft.com/2010/08/ActiveDirectory/PossibleValues", IsNullable=false)]
	[XmlType(Namespace="http://schemas.microsoft.com/2010/08/ActiveDirectory/PossibleValues")]
	public class PossibleClaimValuesType
	{
		private object itemField;

		[XmlAnyElement]
		[XmlElement("UnsignedIntegerList", typeof(UnsignedIntegerListType))]
		[XmlElement("IntegerList", typeof(IntegerListType))]
		[XmlElement("StringList", typeof(StringListType))]
		public object Item
		{
			get
			{
				return this.itemField;
			}
			set
			{
				this.itemField = value;
			}
		}

		public PossibleClaimValuesType()
		{
		}
	}
}