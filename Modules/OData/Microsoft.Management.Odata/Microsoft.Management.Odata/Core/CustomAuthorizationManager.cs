using Microsoft.Management.Odata.Common;
using System;
using System.Management.Automation.Tracing;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Management.Odata.Core
{
	internal class CustomAuthorizationManager : ServiceAuthorizationManager
	{
		public CustomAuthorizationManager()
		{
		}

		protected override bool CheckAccessCore(OperationContext operationContext)
		{
			TraceHelper.Current.CorrelateWithActivity(EtwActivity.GetActivityId());
			TraceHelper.Current.RequestStart();
			DataServiceController.Current.PerfCounters.ActiveRequests.Increment();
			operationContext.TraceIncomingMessage();
			bool flag = false;
			IIdentity identity = CurrentRequestHelper.Identity;
			X509Certificate2 certificate = CurrentRequestHelper.Certificate;
			identity.Trace();
			if (identity == null || !identity.IsAuthenticated)
			{
				TraceHelper.Current.DebugMessage(string.Concat("Unauthenticated user tried to access", identity.ToTraceMessage()));
			}
			else
			{
				UserContext userContext = new UserContext(identity, certificate);
				userContext.Trace();
				flag = DataServiceController.Current.IsAuthorized(userContext, operationContext.IncomingMessageHeaders.To);
			}
			if (!flag)
			{
				if (identity == null)
				{
					TraceHelper.Current.UserNotAuthorized(string.Empty, string.Empty);
				}
				else
				{
					TraceHelper.Current.UserNotAuthorized(identity.Name, identity.AuthenticationType);
				}
				HttpResponseMessageProperty httpResponseMessageProperty = new HttpResponseMessageProperty();
				httpResponseMessageProperty.StatusCode = HttpStatusCode.Unauthorized;
				operationContext.OutgoingMessageProperties.Add(HttpResponseMessageProperty.Name, httpResponseMessageProperty);
			}
			return flag;
		}
	}
}