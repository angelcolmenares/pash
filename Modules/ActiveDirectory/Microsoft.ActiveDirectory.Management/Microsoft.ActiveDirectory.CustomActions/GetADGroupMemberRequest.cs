using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="GetADGroupMemberRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class GetADGroupMemberRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public string GroupDN;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=1)]
		public string PartitionDN;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=2)]
		public bool Recursive;

		public GetADGroupMemberRequest()
		{
		}

		public GetADGroupMemberRequest(string Server, string GroupDN, string PartitionDN, bool Recursive)
		{
			this.Server = Server;
			this.GroupDN = GroupDN;
			this.PartitionDN = PartitionDN;
			this.Recursive = Recursive;
		}
	}
}