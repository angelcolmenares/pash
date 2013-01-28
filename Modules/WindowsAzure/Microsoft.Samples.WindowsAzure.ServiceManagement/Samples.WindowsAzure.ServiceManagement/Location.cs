using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Location : IExtensibleDataObject
	{
		[DataMember(Order=3, EmitDefaultValue=false)]
		public AvailableServicesList AvailableServices
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string DisplayName
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public string Name
		{
			get;
			set;
		}

		public Location()
		{
		}
	}
}