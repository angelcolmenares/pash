using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class OSVersionsContext : ManagementOperationContext
	{
		public string Family
		{
			get;
			set;
		}

		public string FamilyLabel
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		public bool IsDefault
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string Version
		{
			get;
			set;
		}

		public OSVersionsContext()
		{
		}
	}
}