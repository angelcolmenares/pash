using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Deployments", ItemName="Deployment", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class DeploymentList : List<Deployment>
	{
		public DeploymentList()
		{
		}

		public DeploymentList(IEnumerable<Deployment> deployments) : base(deployments)
		{
		}
	}
}