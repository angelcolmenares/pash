using Microsoft.Management.Odata;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Web;

namespace Microsoft.Management.Odata.Common
{
	internal static class CurrentRequestHelper
	{
		private static Uri testHookUri;

		private static IIdentity testHookIdentity;

		public static X509Certificate2 Certificate
		{
			get
			{
				if (HttpContext.Current != null)
				{
					HttpClientCertificate clientCertificate = HttpContext.Current.Request.ClientCertificate;
					if (clientCertificate != null && clientCertificate.Certificate != null && (int)clientCertificate.Certificate.Length > 0)
					{
						return new X509Certificate2(clientCertificate.Certificate);
					}
				}
				return null;
			}
		}

		public static Uri EndPointAddress
		{
			get
			{
				if (OperationContext.Current == null)
				{
					if (CurrentRequestHelper.testHookUri == null)
					{
						throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.ValidContextNotSet, new object[0]));
					}
					else
					{
						return CurrentRequestHelper.testHookUri;
					}
				}
				else
				{
					return OperationContext.Current.EndpointDispatcher.EndpointAddress.Uri;
				}
			}
		}

		public static IIdentity Identity
		{
			get
			{
				if (HttpContext.Current == null)
				{
					if (OperationContext.Current == null)
					{
						if (CurrentRequestHelper.testHookIdentity == null)
						{
							throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.ValidContextNotSet, new object[0]));
						}
						else
						{
							return CurrentRequestHelper.testHookIdentity;
						}
					}
					else
					{
						var securityContext = OperationContext.Current.ServiceSecurityContext;
						if (securityContext == null)
						{
							return new GenericIdentity("Anonymous", "");
						}
						return securityContext.PrimaryIdentity;
					}
				}
				else
				{
					return HttpContext.Current.User.Identity;
				}
			}
		}

		public static Uri Uri
		{
			get
			{
				if (HttpContext.Current == null)
				{
					if (OperationContext.Current == null)
					{
						if (CurrentRequestHelper.testHookUri == null)
						{
							throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.ValidContextNotSet, new object[0]));
						}
						else
						{
							return CurrentRequestHelper.testHookUri;
						}
					}
					else
					{
						return OperationContext.Current.IncomingMessageHeaders.To;
					}
				}
				else
				{
					return HttpContext.Current.Request.Url;
				}
			}
		}

		internal static void TestHookClear()
		{
			CurrentRequestHelper.testHookUri = null;
			CurrentRequestHelper.testHookIdentity = null;
		}

		internal static void TestHookSetIdentity(IIdentity identity)
		{
			CurrentRequestHelper.testHookIdentity = identity;
		}

		internal static void TestHookSetUri(Uri uri)
		{
			CurrentRequestHelper.testHookUri = uri;
		}
	}
}