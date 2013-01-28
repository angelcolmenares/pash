using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Namespace="http://schemas.microsoft.com/windowsazure", Name="AvailableServices", ItemName="AvailableService")]
	public class AvailableServicesList : List<string>, IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public AvailableServicesList()
		{
		}

		public AvailableServicesList(IEnumerable<string> availableServices) : base(availableServices)
		{
		}
	}
}