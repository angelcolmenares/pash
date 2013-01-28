using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ShutdownRoleOperation : RoleOperation
	{
		public override string OperationType
		{
			get
			{
				return "ShutdownRoleOperation";
			}
			set
			{
			}
		}

		public ShutdownRoleOperation()
		{
		}
	}
}