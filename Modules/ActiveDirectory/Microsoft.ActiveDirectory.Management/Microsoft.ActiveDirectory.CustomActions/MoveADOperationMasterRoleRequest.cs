using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[MessageContract(WrapperName="MoveADOperationMasterRoleRequest", WrapperNamespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", IsWrapped=true)]
	internal class MoveADOperationMasterRoleRequest
	{
		[MessageHeader(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
		public string Server;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=0)]
		public ActiveDirectoryOperationMasterRole OperationMasterRole;

		[MessageBodyMember(Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions", Order=1)]
		public bool Seize;

		public MoveADOperationMasterRoleRequest()
		{
		}

		public MoveADOperationMasterRoleRequest(string Server, ActiveDirectoryOperationMasterRole OperationMasterRole, bool Seize)
		{
			this.Server = Server;
			this.OperationMasterRole = OperationMasterRole;
			this.Seize = Seize;
		}
	}
}