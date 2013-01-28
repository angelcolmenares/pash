using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public static class ServiceManagementHelper
	{
		public static IServiceManagement CreateServiceManagementChannel(X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>();
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static IServiceManagement CreateServiceManagementChannel(Binding binding, X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(binding);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static IServiceManagement CreateServiceManagementChannel(ServiceEndpoint endpoint, X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(endpoint);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static IServiceManagement CreateServiceManagementChannel(string endpointConfigurationName, X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(endpointConfigurationName);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static IServiceManagement CreateServiceManagementChannel(Type channelType, X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(channelType);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static IServiceManagement CreateServiceManagementChannel(Uri remoteUri, X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(remoteUri);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static IServiceManagement CreateServiceManagementChannel(Binding binding, Uri remoteUri, X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(binding, remoteUri);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static IServiceManagement CreateServiceManagementChannel(string endpointConfigurationName, Uri remoteUri, X509Certificate2 cert)
		{
			WebChannelFactory<IServiceManagement> webChannelFactory = new WebChannelFactory<IServiceManagement>(endpointConfigurationName, remoteUri);
			webChannelFactory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector());
			webChannelFactory.Credentials.ClientCertificate.Certificate = cert;
			IServiceManagement serviceManagement = webChannelFactory.CreateChannel();
			return serviceManagement;
		}

		public static string DecodeFromBase64String(string original)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(original));
		}

		public static string EncodeToBase64String(string original)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(original));
		}

		public static bool TryGetExceptionDetails(CommunicationException exception, out ServiceManagementError errorDetails)
		{
			HttpStatusCode httpStatusCode = 0;
			string str = null;
			return ServiceManagementHelper.TryGetExceptionDetails(exception, out errorDetails, out httpStatusCode, out str);
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
							httpStatusCode = response.StatusCode;
							if ((int)httpStatusCode != 0x193)
							{
								if (response.Headers != null)
								{
									operationId = response.Headers["x-ms-request-id"];
								}
								Stream responseStream = response.GetResponseStream();
								using (responseStream)
								{
									if (responseStream.Length != (long)0)
									{
										try
										{
											XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(responseStream, new XmlDictionaryReaderQuotas());
											using (xmlDictionaryReader)
											{
												DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(ServiceManagementError));
												errorDetails = (ServiceManagementError)dataContractSerializer.ReadObject(xmlDictionaryReader, true);
											}
										}
										catch (SerializationException serializationException)
										{
											flag = false;
											return flag;
										}
										return true;
									}
									else
									{
										flag = false;
									}
								}
								return flag;
							}
							else
							{
								try
								{
									Stream stream = response.GetResponseStream();
									using (stream)
									{
										XmlDictionaryReader xmlDictionaryReader1 = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
										using (xmlDictionaryReader1)
										{
											DataContractSerializer dataContractSerializer1 = new DataContractSerializer(typeof(ServiceManagementError));
											errorDetails = (ServiceManagementError)dataContractSerializer1.ReadObject(xmlDictionaryReader1, true);
										}
									}
								}
								catch
								{
								}
								return true;
							}
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