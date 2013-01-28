using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="Certificates", ItemName="Certificate", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CertificateList : List<Certificate>
	{
		public CertificateList()
		{
		}

		public CertificateList(IEnumerable<Certificate> certificateList) : base(certificateList)
		{
		}
	}
}