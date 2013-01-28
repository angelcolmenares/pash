using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="MoveADOperationMasterRoleResponse", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class MoveADOperationMasterRoleResponse
	{
		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public bool WasSeized;

		public MoveADOperationMasterRoleResponse()
		{
		}

		public MoveADOperationMasterRoleResponse(bool WasSeized)
		{
			this.WasSeized = WasSeized;
		}
	}
}