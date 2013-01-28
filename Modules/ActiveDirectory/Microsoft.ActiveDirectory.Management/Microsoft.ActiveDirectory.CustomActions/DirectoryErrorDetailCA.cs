using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="DirectoryErrorDetailCA", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class DirectoryErrorDetailCA : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private string ErrorCodeField;

		private string ExtendedErrorMessageField;

		private string MatchedDNField;

		private string MessageField;

		private string[] ReferralField;

		private string ShortMessageField;

		private string Win32ErrorCodeField;

		[DataMember]
		internal string ErrorCode
		{
			get
			{
				return this.ErrorCodeField;
			}
			set
			{
				this.ErrorCodeField = value;
			}
		}

		[DataMember]
		internal string ExtendedErrorMessage
		{
			get
			{
				return this.ExtendedErrorMessageField;
			}
			set
			{
				this.ExtendedErrorMessageField = value;
			}
		}

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
		internal string MatchedDN
		{
			get
			{
				return this.MatchedDNField;
			}
			set
			{
				this.MatchedDNField = value;
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
		internal string[] Referral
		{
			get
			{
				return this.ReferralField;
			}
			set
			{
				this.ReferralField = value;
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

		[DataMember]
		internal string Win32ErrorCode
		{
			get
			{
				return this.Win32ErrorCodeField;
			}
			set
			{
				this.Win32ErrorCodeField = value;
			}
		}

		public DirectoryErrorDetailCA()
		{
		}
	}
}