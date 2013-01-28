using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CertificateSetting : Mergable<CertificateSetting>
	{
		[DataMember(Name="StoreLocation", EmitDefaultValue=false, Order=0)]
		public string StoreLocation
		{
			get
			{
				return base.GetValue<string>("StoreLocation");
			}
			set
			{
				base.SetValue<string>("StoreLocation", value);
			}
		}

		[DataMember(Name="StoreName", EmitDefaultValue=false, Order=1)]
		public string StoreName
		{
			get
			{
				return base.GetValue<string>("StoreName");
			}
			set
			{
				base.SetValue<string>("StoreName", value);
			}
		}

		[DataMember(Name="Thumbprint", EmitDefaultValue=false, Order=2)]
		public string Thumbprint
		{
			get
			{
				return base.GetValue<string>("Thumbprint");
			}
			set
			{
				base.SetValue<string>("Thumbprint", value);
			}
		}

		public CertificateSetting()
		{
		}
	}
}