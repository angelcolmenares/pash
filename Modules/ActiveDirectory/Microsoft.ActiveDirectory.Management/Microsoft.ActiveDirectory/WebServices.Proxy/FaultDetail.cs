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
	[XmlInclude(typeof(PullFault))]
	[XmlInclude(typeof(EnumerateFault))]
	[XmlInclude(typeof(RenewFault))]
	[XmlRoot(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	[XmlType(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory")]
	public class FaultDetail
	{
		private ArgumentErrorDetail argumentErrorField;

		private string errorField;

		private DirectoryErrorDetail directoryErrorField;

		private string invalidAttributeTypeField;

		private string invalidOperationField;

		private ChangeType invalidChangeField;

		private AttributeTypeAndValue invalidAttributeTypeOrValueField;

		private string shortErrorField;

		private string unknownAttributeField;

		[XmlElement(Order=0)]
		public ArgumentErrorDetail ArgumentError
		{
			get
			{
				return this.argumentErrorField;
			}
			set
			{
				this.argumentErrorField = value;
			}
		}

		[XmlElement(Order=2)]
		public DirectoryErrorDetail DirectoryError
		{
			get
			{
				return this.directoryErrorField;
			}
			set
			{
				this.directoryErrorField = value;
			}
		}

		[XmlElement(Order=1)]
		public string Error
		{
			get
			{
				return this.errorField;
			}
			set
			{
				this.errorField = value;
			}
		}

		[XmlElement(Order=3)]
		public string InvalidAttributeType
		{
			get
			{
				return this.invalidAttributeTypeField;
			}
			set
			{
				this.invalidAttributeTypeField = value;
			}
		}

		[XmlElement(Order=6)]
		public AttributeTypeAndValue InvalidAttributeTypeOrValue
		{
			get
			{
				return this.invalidAttributeTypeOrValueField;
			}
			set
			{
				this.invalidAttributeTypeOrValueField = value;
			}
		}

		[XmlElement(Order=5)]
		public ChangeType InvalidChange
		{
			get
			{
				return this.invalidChangeField;
			}
			set
			{
				this.invalidChangeField = value;
			}
		}

		[XmlElement(Order=4)]
		public string InvalidOperation
		{
			get
			{
				return this.invalidOperationField;
			}
			set
			{
				this.invalidOperationField = value;
			}
		}

		[XmlElement(Order=7)]
		public string ShortError
		{
			get
			{
				return this.shortErrorField;
			}
			set
			{
				this.shortErrorField = value;
			}
		}

		[XmlElement(Order=8)]
		public string UnknownAttribute
		{
			get
			{
				return this.unknownAttributeField;
			}
			set
			{
				this.unknownAttributeField = value;
			}
		}

		public FaultDetail()
		{
		}
	}
}