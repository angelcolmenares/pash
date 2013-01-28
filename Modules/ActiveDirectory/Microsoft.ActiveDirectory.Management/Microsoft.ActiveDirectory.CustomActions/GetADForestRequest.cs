using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="GetADForestRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class GetADForestRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		public GetADForestRequest()
		{
		}

		public GetADForestRequest(string Server)
		{
			this.Server = Server;
		}
	}
}