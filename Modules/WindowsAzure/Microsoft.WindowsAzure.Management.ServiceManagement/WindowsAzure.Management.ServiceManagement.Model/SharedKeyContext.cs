using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class SharedKeyContext : ManagementOperationContext
	{
		public string Value
		{
			get;
			set;
		}

		public SharedKeyContext()
		{
		}
	}
}