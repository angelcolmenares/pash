using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class GatewayManagementOperationContext : ManagementOperationContext
	{
		public string Data
		{
			get;
			set;
		}

		public string ErrorCode
		{
			get;
			set;
		}

		public string ErrorMessage
		{
			get;
			set;
		}

		public GatewayManagementOperationContext()
		{
		}
	}
}