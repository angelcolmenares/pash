using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class LoadBalancerProbe : Mergable<LoadBalancerProbe>
	{
		[DataMember(Name="Path", EmitDefaultValue=false, Order=0)]
		public string Path
		{
			get
			{
				return base.GetValue<string>("Path");
			}
			set
			{
				base.SetValue<string>("Path", value);
			}
		}

		[DataMember(Name="Port", EmitDefaultValue=false, Order=1)]
		private int? port
		{
			get
			{
				return base.GetField<int>("Port");
			}
			set
			{
				base.SetField<int>("Port", value);
			}
		}

		public int Port
		{
			get
			{
				return base.GetValue<int>("Port");
			}
			set
			{
				base.SetValue<int>("Port", value);
			}
		}

		[DataMember(Name="Protocol", EmitDefaultValue=false, Order=2)]
		public string Protocol
		{
			get
			{
				return base.GetValue<string>("Protocol");
			}
			set
			{
				base.SetValue<string>("Protocol", value);
			}
		}

		public LoadBalancerProbe()
		{
		}
	}
}