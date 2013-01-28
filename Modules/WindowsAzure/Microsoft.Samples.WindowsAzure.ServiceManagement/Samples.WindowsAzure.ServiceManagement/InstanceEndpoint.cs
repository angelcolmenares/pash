using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class InstanceEndpoint : IExtensibleDataObject
	{
		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=4)]
		public int LocalPort
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public string Protocol
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public int PublicPort
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string Vip
		{
			get;
			set;
		}

		public InstanceEndpoint()
		{
		}
	}
}