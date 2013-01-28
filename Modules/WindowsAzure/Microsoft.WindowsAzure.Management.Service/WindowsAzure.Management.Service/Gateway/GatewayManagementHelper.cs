using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Xml;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	public static class GatewayManagementHelper
	{
		public static IGatewayServiceManagement CreateGatewayManagementChannel(Binding binding, Uri remoteUri, X509Certificate2 cert)
		{
			WebChannelFactory<IGatewayServiceManagement> webChannelFactory = new WebChannelFactory<IGatewayServiceManagement>(binding, remoteUri);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			return webChannelFactory.CreateChannel();
		}

		public static bool TryGetExceptionDetails(CommunicationException exception, out ServiceManagementError errorDetails)
		{
			HttpStatusCode httpStatusCode = 0;
			string str = null;
			return GatewayManagementHelper.TryGetExceptionDetails(exception, out errorDetails, out httpStatusCode, out str);
		}

		public static bool TryGetExceptionDetails(CommunicationException exception, out ServiceManagementError errorDetails, out HttpStatusCode httpStatusCode, out string operationId)
		{
			bool flag;
			errorDetails = null;
			httpStatusCode = 0;
			operationId = null;
			if (exception != null)
			{
				if (exception.Message != "Internal Server Error")
				{
					WebException innerException = exception.InnerException as WebException;
					if (innerException != null)
					{
						HttpWebResponse response = innerException.Response as HttpWebResponse;
						if (response != null)
						{
							if (response.Headers != null)
							{
								operationId = response.Headers["x-ms-request-id"];
							}
							Stream responseStream = response.GetResponseStream();
							using (responseStream)
							{
								try
								{
									if (responseStream == null || responseStream.Length == (long)0)
									{
										flag = false;
										return flag;
									}
									else
									{
										XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(responseStream, new XmlDictionaryReaderQuotas());
										DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ServiceManagementError));
										errorDetails = (ServiceManagementError)dataContractSerializer.ReadObject(xmlDictionaryReader, true);
									}
								}
								catch (Exception exception1)
								{
									flag = false;
									return flag;
								}
								return true;
							}
							return flag;
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					httpStatusCode = HttpStatusCode.InternalServerError;
					return true;
				}
			}
			else
			{
				return false;
			}
		}
	}
}