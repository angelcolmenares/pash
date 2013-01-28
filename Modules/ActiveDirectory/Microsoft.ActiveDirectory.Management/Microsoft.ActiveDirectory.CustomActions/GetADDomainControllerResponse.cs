using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="GetADDomainControllerResponse", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class GetADDomainControllerResponse
	{
		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public ActiveDirectoryDomainController[] DomainControllers;

		public GetADDomainControllerResponse()
		{
		}

		public GetADDomainControllerResponse(ActiveDirectoryDomainController[] DomainControllers)
		{
			this.DomainControllers = DomainControllers;
		}
	}
}