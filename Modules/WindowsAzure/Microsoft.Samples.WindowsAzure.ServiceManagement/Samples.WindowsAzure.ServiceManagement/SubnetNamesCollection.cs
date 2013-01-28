using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="SubnetNames", ItemName="SubnetName", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class SubnetNamesCollection : Collection<string>
	{
		public SubnetNamesCollection()
		{
		}
	}
}