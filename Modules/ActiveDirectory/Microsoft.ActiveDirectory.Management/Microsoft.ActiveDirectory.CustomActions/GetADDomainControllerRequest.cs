using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="GetADDomainControllerRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class GetADDomainControllerRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public string[] NtdsSettingsDN;

		public GetADDomainControllerRequest()
		{
		}

		public GetADDomainControllerRequest(string Server, string[] NtdsSettingsDN)
		{
			this.Server = Server;
			this.NtdsSettingsDN = NtdsSettingsDN;
		}
	}
}