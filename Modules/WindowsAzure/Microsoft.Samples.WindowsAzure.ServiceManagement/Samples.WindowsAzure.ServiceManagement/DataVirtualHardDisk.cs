using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DataVirtualHardDisk : Mergable<DataVirtualHardDisk>
	{
		[DataMember(Name="DiskLabel", EmitDefaultValue=false, Order=1)]
		public string DiskLabel
		{
			get
			{
				return base.GetValue<string>("DiskLabel");
			}
			set
			{
				base.SetValue<string>("DiskLabel", value);
			}
		}

		[DataMember(Name="DiskName", EmitDefaultValue=false, Order=2)]
		public string DiskName
		{
			get
			{
				return base.GetValue<string>("DiskName");
			}
			set
			{
				base.SetValue<string>("DiskName", value);
			}
		}

		[DataMember(Name="HostCaching", EmitDefaultValue=false, Order=0)]
		public string HostCaching
		{
			get
			{
				return base.GetValue<string>("HostCaching");
			}
			set
			{
				base.SetValue<string>("HostCaching", value);
			}
		}

		[DataMember(Name="LogicalDiskSizeInGB", EmitDefaultValue=false, Order=4)]
		private int? logicalDiskSizeInGB
		{
			get
			{
				return base.GetField<int>("LogicalDiskSizeInGB");
			}
			set
			{
				base.SetField<int>("LogicalDiskSizeInGB", value);
			}
		}

		public int LogicalDiskSizeInGB
		{
			get
			{
				return base.GetValue<int>("LogicalDiskSizeInGB");
			}
			set
			{
				base.SetValue<int>("LogicalDiskSizeInGB", value);
			}
		}

		[DataMember(Name="Lun", EmitDefaultValue=false, Order=3)]
		public int Lun
		{
			get
			{
				return base.GetValue<int>("Lun");
			}
			set
			{
				base.SetValue<int>("Lun", value);
			}
		}

		[DataMember(Name="MediaLink", EmitDefaultValue=false, Order=5)]
		public Uri MediaLink
		{
			get
			{
				return base.GetValue<Uri>("MediaLink");
			}
			set
			{
				base.SetValue<Uri>("MediaLink", value);
			}
		}

		[DataMember(Name="SourceMediaLink", EmitDefaultValue=false, Order=6)]
		public Uri SourceMediaLink
		{
			get
			{
				return base.GetValue<Uri>("SourceMediaLink");
			}
			set
			{
				base.SetValue<Uri>("SourceMediaLink", value);
			}
		}

		public DataVirtualHardDisk()
		{
		}
	}
}