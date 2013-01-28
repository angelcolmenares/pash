using Microsoft.Samples.WindowsAzure.ServiceManagement;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml;

namespace Microsoft.WindowsAzure.Management.Service
{
	public class SMErrorHelper
	{
		public SMErrorHelper()
		{
		}

		public static bool TryGetExceptionDetails(CommunicationException exception, out ServiceManagementError errorDetails, out string operationId)
		{
			HttpStatusCode httpStatusCode = 0;
			return SMErrorHelper.TryGetExceptionDetails(exception, out errorDetails, out httpStatusCode, out operationId);
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
							if (response.StatusCode != HttpStatusCode.NotFound)
							{
								Stream responseStream = response.GetResponseStream();
								using (responseStream)
								{
									if (responseStream.Length != (long)0)
									{
										try
										{
											errorDetails = new ServiceManagementError();
											responseStream.Seek((long)0, SeekOrigin.Begin);
											StreamReader streamReader = new StreamReader(responseStream);
											using (StringReader stringReader = new StringReader(streamReader.ReadToEnd()))
											{
												XmlReader xmlReader = XmlReader.Create(stringReader);
												while (xmlReader.Read())
												{
													XmlNodeType nodeType = xmlReader.NodeType;
													if (nodeType != XmlNodeType.Element)
													{
														continue;
													}
													if (xmlReader.Name != "Code")
													{
														if (xmlReader.Name != "Message")
														{
															continue;
														}
														xmlReader.Read();
														errorDetails.Message = xmlReader.Value;
													}
													else
													{
														xmlReader.Read();
														errorDetails.Code = xmlReader.Value;
													}
												}
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
								errorDetails = new ServiceManagementError();
								errorDetails.Message = string.Concat(response.ResponseUri.AbsoluteUri, " does not exist.");
								errorDetails.Code = response.StatusCode.ToString();
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