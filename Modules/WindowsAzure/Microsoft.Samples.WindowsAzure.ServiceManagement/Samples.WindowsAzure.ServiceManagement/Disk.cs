using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class Disk : IExtensibleDataObject
	{
		[DataMember(EmitDefaultValue=false, Order=0)]
		public string AffinityGroup
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=1)]
		public RoleReference AttachedTo
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=3)]
		public bool IsCorrupted
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=4)]
		public string Label
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=5)]
		public string Location
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=6)]
		public int LogicalDiskSizeInGB
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=7)]
		public Uri MediaLink
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=8)]
		public string Name
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=2)]
		public string OS
		{
			get;
			set;
		}

		[DataMember(EmitDefaultValue=false, Order=9)]
		public string SourceImageName
		{
			get;
			set;
		}

		ExtensionDataObject System.Runtime.Serialization.IExtensibleDataObject.ExtensionData
		{
			get;
			set;
		}

		public Disk()
		{
		}
	}
}