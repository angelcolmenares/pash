using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="CustomActionFault", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	[KnownType(typeof(SetPasswordFault))]
	[KnownType(typeof(MoveADOperationMasterRoleFault))]
	[KnownType(typeof(GetADDomainControllerFault))]
	[KnownType(typeof(GetADGroupMemberFault))]
	[KnownType(typeof(ChangeOptionalFeatureFault))]
	[KnownType(typeof(GetADForestFault))]
	[KnownType(typeof(ChangePasswordFault))]
	[KnownType(typeof(GetADPrincipalGroupMembershipFault))]
	[KnownType(typeof(GetADDomainFault))]
	[KnownType(typeof(TranslateNameFault))]
	[KnownType(typeof(GetADPrincipalAuthorizationGroupFault))]
	internal class CustomActionFault : IExtensibleDataObject
	{
		private ExtensionDataObject extensionDataField;

		private ArgumentErrorDetailCA ArgumentErrorField;

		private DirectoryErrorDetailCA DirectoryErrorField;

		private string ErrorField;

		private string ShortErrorField;

		[DataMember]
		internal ArgumentErrorDetailCA ArgumentError
		{
			get
			{
				return this.ArgumentErrorField;
			}
			set
			{
				this.ArgumentErrorField = value;
			}
		}

		[DataMember]
		internal DirectoryErrorDetailCA DirectoryError
		{
			get
			{
				return this.DirectoryErrorField;
			}
			set
			{
				this.DirectoryErrorField = value;
			}
		}

		[DataMember]
		internal string Error
		{
			get
			{
				return this.ErrorField;
			}
			set
			{
				this.ErrorField = value;
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
		internal string ShortError
		{
			get
			{
				return this.ShortErrorField;
			}
			set
			{
				this.ShortErrorField = value;
			}
		}

		public CustomActionFault()
		{
		}
	}
}