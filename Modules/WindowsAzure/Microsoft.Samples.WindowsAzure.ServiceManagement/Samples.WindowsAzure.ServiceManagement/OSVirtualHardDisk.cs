using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class OSVirtualHardDisk : Mergable<OSVirtualHardDisk>
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

		[DataMember(Name="MediaLink", EmitDefaultValue=false, Order=3)]
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

		[DataMember(Name="OS", EmitDefaultValue=false, Order=5)]
		public string OS
		{
			get
			{
				return base.GetValue<string>("OS");
			}
			set
			{
				base.SetValue<string>("OS", value);
			}
		}

		[DataMember(Name="SourceImageName", EmitDefaultValue=false, Order=4)]
		public string SourceImageName
		{
			get
			{
				return base.GetValue<string>("SourceImageName");
			}
			set
			{
				base.SetValue<string>("SourceImageName", value);
			}
		}

		public OSVirtualHardDisk()
		{
		}
	}
}