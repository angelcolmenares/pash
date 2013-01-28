using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class MachineImage : IExtensibleDataObject
	{
		[DataMember(Order=5, EmitDefaultValue=false)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(Order=11)]
		public long CompressedSizeInBytes
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public string Description
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=15)]
		public bool InUse
		{
			get;
			set;
		}

		[DataMember(Order=2)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=4, EmitDefaultValue=false)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(Order=10)]
		public long MountedSizeInBytes
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

		[DataMember(Order=7)]
		public string ParentImageName
		{
			get;
			set;
		}

		[DataMember(Order=13, EmitDefaultValue=false)]
		public string ParentTimestamp
		{
			get;
			set;
		}

		[DataMember(Order=12, EmitDefaultValue=false)]
		public string ParentUuid
		{
			get;
			set;
		}

		[DataMember(Order=6)]
		public string Status
		{
			get;
			set;
		}

		[DataMember(Order=9)]
		public string Timestamp
		{
			get;
			set;
		}

		[DataMember(Order=8)]
		public string Uuid
		{
			get;
			set;
		}

		public MachineImage()
		{
		}
	}
}