using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.Management.Service
{
	[CollectionDataContract(Name="Connections", ItemName="Connection", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class ConnectionCollection : Collection<Connection>
	{
		public ConnectionCollection()
		{
		}

		public ConnectionCollection(IList<Connection> connections) : base(connections)
		{
		}
	}
}