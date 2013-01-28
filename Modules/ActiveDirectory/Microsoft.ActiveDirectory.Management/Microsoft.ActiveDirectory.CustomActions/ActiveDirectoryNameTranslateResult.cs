using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="ActiveDirectoryNameTranslateResult", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class ActiveDirectoryNameTranslateResult : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string NameField;

		private uint ResultField;

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

		[DataMember(IsRequired=true)]
		internal string Name
		{
			get
			{
				return this.NameField;
			}
			set
			{
				this.NameField = value;
			}
		}

		[DataMember(IsRequired=true)]
		internal uint Result
		{
			get
			{
				return this.ResultField;
			}
			set
			{
				this.ResultField = value;
			}
		}

		public ActiveDirectoryNameTranslateResult()
		{
		}
	}
}