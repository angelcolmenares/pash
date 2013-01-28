using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="ExtendedPropertiesList", ItemName="ExtendedProperty", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ExtendedPropertiesList : List<ExtendedProperty>
	{
		public ExtendedPropertiesList()
		{
		}

		public ExtendedPropertiesList(IEnumerable<ExtendedProperty> propertyList) : base(propertyList)
		{
		}
	}
}