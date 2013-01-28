using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[DataContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public class HostedService : IExtensibleDataObject
	{
		[DataMember(Order=4, EmitDefaultValue=false)]
		public DeploymentList Deployments
		{
			get;
			set;
		}

		public ExtensionDataObject ExtensionData
		{
			get;
			set;
		}

		[DataMember(Order=3, EmitDefaultValue=false)]
		public HostedServiceProperties HostedServiceProperties
		{
			get;
			set;
		}

		[DataMember(Order=5, EmitDefaultValue=false)]
		public bool? IsComplete
		{
			get;
			set;
		}

		[DataMember(Order=2, EmitDefaultValue=false)]
		public string ServiceName
		{
			get;
			set;
		}

		[DataMember(Order=1)]
		public Uri Url
		{
			get;
			set;
		}

		public HostedService()
		{
		}
	}
}