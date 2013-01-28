using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Capabilities", ItemName="Capability", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CapabilitiesList : List<string>, IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		public CapabilitiesList()
		{
		}

		public CapabilitiesList(IEnumerable<string> capabilities) : base(capabilities)
		{
		}
	}
}