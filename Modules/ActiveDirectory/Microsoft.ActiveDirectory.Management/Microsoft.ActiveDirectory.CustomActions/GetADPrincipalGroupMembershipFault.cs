using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.ActiveDirectory.CustomActions
{
	[DataContract(Name="GetADPrincipalGroupMembershipFault", Namespace="http://schemas.microsoft.com/2008/1/ActiveDirectory/CustomActions")]
	[DebuggerStepThrough]
	[GeneratedCode("System.Runtime.Serialization", "3.0.0.0")]
	internal class GetADPrincipalGroupMembershipFault : CustomActionFault
	{
		public GetADPrincipalGroupMembershipFault()
		{
		}
	}
}