using Microsoft.WindowsAzure.Management.Model;
using System;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class OSImageContext : ManagementOperationContext
	{
		public string AffinityGroup
		{
			get;
			set;
		}

		public string Category
		{
			get;
			set;
		}

		public string ImageName
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string Location
		{
			get;
			set;
		}

		public int LogicalSizeInGB
		{
			get;
			set;
		}

		public Uri MediaLink
		{
			get;
			set;
		}

		public string OS
		{
			get;
			set;
		}

		public OSImageContext()
		{
		}
	}
}