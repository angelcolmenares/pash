using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="ChangePasswordRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class ChangePasswordRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public string AccountDN;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=1)]
		public string NewPassword;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=2)]
		public string OldPassword;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=3)]
		public string PartitionDN;

		public ChangePasswordRequest()
		{
		}

		public ChangePasswordRequest(string Server, string AccountDN, string NewPassword, string OldPassword, string PartitionDN)
		{
			this.Server = Server;
			this.AccountDN = AccountDN;
			this.NewPassword = NewPassword;
			this.OldPassword = OldPassword;
			this.PartitionDN = PartitionDN;
		}
	}
}