using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="PrepareMachineImage", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class PrepareImageUploadInput : IExtensibleDataObject
	{
		[DataMember(Order=8, EmitDefaultValue=false)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(Order=5)]
		public long CompressedSizeInBytes
		{
			get;
			set;
		}

		[DataMember(Order=2)]
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

		[DataMember(Order=1)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(Order=7, EmitDefaultValue=false)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(Order=6)]
		public long MountedSizeInBytes
		{
			get;
			set;
		}

		[DataMember(Order=10, EmitDefaultValue=false)]
		public string ParentTimestamp
		{
			get;
			set;
		}

		[DataMember(Order=9, EmitDefaultValue=false)]
		public string ParentUuid
		{
			get;
			set;
		}

		[DataMember(Order=4)]
		public string Timestamp
		{
			get;
			set;
		}

		[DataMember(Order=3)]
		public string Uuid
		{
			get;
			set;
		}

		public PrepareImageUploadInput()
		{
		}
	}
}