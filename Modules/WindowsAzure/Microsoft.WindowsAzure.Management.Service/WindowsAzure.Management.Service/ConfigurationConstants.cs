using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.WindowsAzure.Management.Service
{
	public static class ConfigurationConstants
	{
		public const string ServiceManagementEndpoint = "https://management.core.windows.net";

		public static Binding WebHttpBinding()
		{
			WebHttpBinding webHttpBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
			webHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
			webHttpBinding.ReaderQuotas.MaxStringContentLength = 0x4000000;
			webHttpBinding.MaxBufferSize = 0x4c4b40;
			webHttpBinding.MaxReceivedMessageSize = (long)0x4c4b40;
			webHttpBinding.ReaderQuotas.MaxArrayLength = 0x4c4b40;
			return webHttpBinding;
		}
	}
}