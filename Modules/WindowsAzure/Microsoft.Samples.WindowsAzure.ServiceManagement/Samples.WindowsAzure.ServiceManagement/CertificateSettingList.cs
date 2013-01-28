using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	[CollectionDataContract(Name="CertificateSettings", Namespace="http://schemas.microsoft.com/windowsazure")]
	public class CertificateSettingList : List<CertificateSetting>
	{
		public CertificateSettingList()
		{
		}
	}
}