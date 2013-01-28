using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class VirtualNetworkSite : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=7)]
		public AddressSpace AddressSpace
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=4)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=9)]
		public DnsSettings Dns
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=10)]
		public Gateway Gateway
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string Id
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6)]
		public bool InUse
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public string Name
		{
			get;
			private set;
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public string State
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=8)]
		public SubnetList Subnets
		{
			get;
			set;
		}

		public VirtualNetworkSite(string name)
		{
			this.Name = name;
		}
	}
}