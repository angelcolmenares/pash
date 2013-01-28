using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ArgumentErrorDetailCA", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class ArgumentErrorDetailCA : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string MessageField;

		private string ParameterNameField;

		private string ShortMessageField;

		public ExtensionDataObject ExtensionData
		{
			get
			{
				return this.extensionDataField;
			}
			set
			{
				this.extensionDataField = value;
			}
		}

		[DataMember]
		internal string Message
		{
			get
			{
				return this.MessageField;
			}
			set
			{
				this.MessageField = value;
			}
		}

		[DataMember]
		internal string ParameterName
		{
			get
			{
				return this.ParameterNameField;
			}
			set
			{
				this.ParameterNameField = value;
			}
		}

		[DataMember]
		internal string ShortMessage
		{
			get
			{
				return this.ShortMessageField;
			}
			set
			{
				this.ShortMessageField = value;
			}
		}

		public ArgumentErrorDetailCA()
		{
		}
	}
}