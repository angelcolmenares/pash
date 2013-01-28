using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="AddressPrefixes", ItemName="AddressPrefix", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class AddressPrefixList : List<string>
	{
		public AddressPrefixList()
		{
		}
	}
}