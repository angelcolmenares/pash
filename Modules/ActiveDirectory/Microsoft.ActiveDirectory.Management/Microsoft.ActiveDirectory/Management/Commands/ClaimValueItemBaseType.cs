using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[GeneratedCode("xsd", "4.0.30319.1")]
	[Serializable]
	[XmlInclude(typeof(StringItemType))]
	[XmlInclude(typeof(UnsignedIntegerItemType))]
	[XmlInclude(typeof(IntegerItemType))]
	[XmlType(Namespace="http://schemas.microsoft.com/2010/08/ActiveDirectory/PossibleValues")]
	public abstract class ClaimValueItemBaseType
	{
		private string valueGUIDField;

		private string valueDisplayNameField;

		private string valueDescriptionField;

		private XmlElement[] anyField;

		[XmlAnyElement]
		public XmlElement[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}

		public string ValueDescription
		{
			get
			{
				return this.valueDescriptionField;
			}
			set
			{
				this.valueDescriptionField = value;
			}
		}

		public string ValueDisplayName
		{
			get
			{
				return this.valueDisplayNameField;
			}
			set
			{
				this.valueDisplayNameField = value;
			}
		}

		public string ValueGUID
		{
			get
			{
				return this.valueGUIDField;
			}
			set
			{
				this.valueGUIDField = value;
			}
		}

		protected ClaimValueItemBaseType()
		{
		}
	}
}