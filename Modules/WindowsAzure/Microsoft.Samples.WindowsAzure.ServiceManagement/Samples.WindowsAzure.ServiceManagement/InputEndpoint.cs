using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Name="InputEndpoint", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class InputEndpoint : Mergable<InputEndpoint>
	{
		[DataMember(Name="LoadBalancedEndpointSetName", EmitDefaultValue=false, Order=1)]
		public string LoadBalancedEndpointSetName
		{
			get
			{
				return base.GetValue<string>("LoadBalancedEndpointSetName");
			}
			set
			{
				base.SetValue<string>("LoadBalancedEndpointSetName", value);
			}
		}

		[DataMember(Name="LoadBalancerProbe", EmitDefaultValue=false, Order=5)]
		public LoadBalancerProbe LoadBalancerProbe
		{
			get
			{
				return base.GetValue<LoadBalancerProbe>("LoadBalancerProbe");
			}
			set
			{
				base.SetValue<LoadBalancerProbe>("LoadBalancerProbe", value);
			}
		}

		[DataMember(Name="LocalPort", EmitDefaultValue=false, Order=2)]
		private int? localPort
		{
			get
			{
				return base.GetField<int>("LocalPort");
			}
			set
			{
				base.SetField<int>("LocalPort", value);
			}
		}

		public int LocalPort
		{
			get
			{
				return base.GetValue<int>("LocalPort");
			}
			set
			{
				base.SetValue<int>("LocalPort", value);
			}
		}

		[DataMember(Name="Name", EmitDefaultValue=false, Order=3)]
		public string Name
		{
			get
			{
				return base.GetValue<string>("Name");
			}
			set
			{
				base.SetValue<string>("Name", value);
			}
		}

		[DataMember(Name="Port", EmitDefaultValue=false, Order=4)]
		public int? Port
		{
			get
			{
				return base.GetValue<int?>("Port");
			}
			set
			{
				base.SetValue<int?>("Port", value);
			}
		}

		[DataMember(Name="Protocol", EmitDefaultValue=false, Order=6)]
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

		[DataMember(Name="Vip", EmitDefaultValue=false, Order=7)]
		public string Vip
		{
			get
			{
				return base.GetValue<string>("Vip");
			}
			set
			{
				base.SetValue<string>("Vip", value);
			}
		}

		public InputEndpoint()
		{
		}
	}
}