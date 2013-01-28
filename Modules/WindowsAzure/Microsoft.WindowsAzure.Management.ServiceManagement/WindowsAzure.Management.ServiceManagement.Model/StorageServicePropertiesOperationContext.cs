using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class StorageServicePropertiesOperationContext : StorageServiceOperationContext
	{
		public string AffinityGroup
		{
			get;
			set;
		}

		public IEnumerable<string> Endpoints
		{
			get;
			set;
		}

		public string GeoPrimaryLocation
		{
			get;
			set;
		}

		public bool? GeoReplicationEnabled
		{
			get;
			set;
		}

		public string GeoSecondaryLocation
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

		public string StatusOfPrimary
		{
			get;
			set;
		}

		public string StatusOfSecondary
		{
			get;
			set;
		}

		public string StorageAccountDescription
		{
			get;
			set;
		}

		public string StorageAccountStatus
		{
			get;
			set;
		}

		public StorageServicePropertiesOperationContext()
		{
		}
	}
}