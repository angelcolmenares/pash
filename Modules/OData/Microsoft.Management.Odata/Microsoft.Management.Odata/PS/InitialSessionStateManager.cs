using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Security.Principal;

namespace Microsoft.Management.Odata.PS
{
	internal class InitialSessionStateManager : IItemFactory<InitialSessionState, UserContext>
	{
		private const int TimeLimitForLoggingEvent = 30;

		private PSSessionConfiguration sessionConfiguration;

		public InitialSessionStateManager(string pssessionConfigurationAssembly, string pssessionConfigurationTypeName)
		{
			try
			{
				Type type = TypeLoader.LoadType(pssessionConfigurationAssembly, pssessionConfigurationTypeName);
				if (type != null)
				{
					this.sessionConfiguration = type.Assembly.CreateInstance(type.FullName) as PSSessionConfiguration;
					if (this.sessionConfiguration == null)
					{
						throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.PSSessionConfigurationCreationFailed, new object[0]));
					}
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = pssessionConfigurationTypeName;
					objArray[1] = pssessionConfigurationAssembly;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.TypeLoadFromAssebmlyFailed, objArray), "pssessionConfigurationTypeName");
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TraceHelper.Current.PSSessionConfigurationLoadingFailed(pssessionConfigurationAssembly, exception.ToTraceMessage("Exception"));
				if (!exception.IsSevereException())
				{
					throw new TypeLoadException(pssessionConfigurationTypeName, pssessionConfigurationAssembly, Utils.GetBaseBinDirectory(pssessionConfigurationAssembly), exception);
				}
				else
				{
					throw;
				}
			}
			TraceHelper.Current.PSSessionConfigurationLoadedSuccessfully(pssessionConfigurationTypeName);
		}

		public InitialSessionState Create(UserContext userContext, string membershipId)
		{
			InitialSessionState initialSessionState;
			using (OperationTracerWithTimeout operationTracerWithTimeout = new OperationTracerWithTimeout(new Action<string>(TraceHelper.Current.PSSessionCallStart), new Action<string>(TraceHelper.Current.PSSessionCallEnd), "InitialSessionState", new Action<string>(TraceHelper.Current.PSSessionMethodExceededTimeLimit), 30))
			{
				PSCertificateDetails pSCertificateDetail = null;
				if (userContext.ClientCertificate != null)
				{
					pSCertificateDetail = new PSCertificateDetails(userContext.ClientCertificate.Subject, userContext.ClientCertificate.Issuer, userContext.ClientCertificate.Thumbprint);
				}
				PSIdentity pSIdentity = new PSIdentity(userContext.AuthenticationType, userContext.IsAuthenticated, userContext.Name, pSCertificateDetail);
				PSPrincipal pSPrincipal = new PSPrincipal(pSIdentity, userContext.GetIdentity() as WindowsIdentity);
				PSSenderInfo pSSenderInfo = new PSSenderInfo(pSPrincipal, DataServiceController.Current.GetCurrentResourceUri().ToString());
				try
				{
					InitialSessionState initialSessionState1 = this.sessionConfiguration.GetInitialSessionState(pSSenderInfo);
					if (initialSessionState1 != null)
					{
						TraceHelper.Current.GetInitialSessionStateRequestSucceeded(userContext.Name);
						initialSessionState1.Trace();
						initialSessionState = initialSessionState1;
					}
					else
					{
						object[] objArray = new object[2];
						objArray[0] = "PSSessionState.GetInitialSessionState";
						objArray[1] = "null";
						throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.MethodReturnedInvalidOutput, objArray));
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					TraceHelper.Current.GetInitialSessionStateRequestFailed(userContext.Name, exception.Message);
					if (!exception.IsSevereException())
					{
						throw new CustomModuleInvocationFailedException(this.sessionConfiguration.GetType().AssemblyQualifiedName, "GetInitialState", exception);
					}
					else
					{
						throw;
					}
				}
			}
			return initialSessionState;
		}
	}
}