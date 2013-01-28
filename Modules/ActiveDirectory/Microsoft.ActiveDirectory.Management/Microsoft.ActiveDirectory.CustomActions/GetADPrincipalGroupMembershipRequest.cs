using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="GetADPrincipalGroupMembershipRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class GetADPrincipalGroupMembershipRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public string PartitionDN;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=1)]
		public string PrincipalDN;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=2)]
		public string ResourceContextPartition;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=3)]
		public string ResourceContextServer;

		public GetADPrincipalGroupMembershipRequest()
		{
		}

		public GetADPrincipalGroupMembershipRequest(string Server, string PartitionDN, string PrincipalDN, string ResourceContextPartition, string ResourceContextServer)
		{
			this.Server = Server;
			this.PartitionDN = PartitionDN;
			this.PrincipalDN = PrincipalDN;
			this.ResourceContextPartition = ResourceContextPartition;
			this.ResourceContextServer = ResourceContextServer;
		}
	}
}