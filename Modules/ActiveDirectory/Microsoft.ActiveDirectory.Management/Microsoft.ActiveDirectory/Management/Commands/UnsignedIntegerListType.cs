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
	public class UnsignedIntegerListType
	{
		private UnsignedIntegerItemType[] itemField;

		[XmlElement("Item")]
		public UnsignedIntegerItemType[] Item
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

		public UnsignedIntegerListType()
		{
		}
	}
}