using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OperatingSystemFamily : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string Label
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

		[DataMember(Order=3)]
		public OperatingSystemList OperatingSystems
		{
			get;
			set;
		}

		public OperatingSystemFamily()
		{
		}
	}
}