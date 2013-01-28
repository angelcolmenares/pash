using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Security.Principal;
using System.ServiceModel;

namespace Microsoft.Management.Odata.Core
{
	internal class CustomAuthorizationHandler
	{
		private const int TimeLimitForLoggingEvent = 30;

		private CustomAuthorization customAuthorization;

		private CustomAuthorizationHandler.ICustomContextStore customContextStore;

		public CustomAuthorizationHandler(string customAuthzAssembly, string customAuthzTypeName, CustomAuthorizationHandler.ICustomContextStore customContextStore)
		{
			try
			{
				if (customContextStore != null)
				{
					this.customContextStore = customContextStore;
				}
				else
				{
					this.customContextStore = new CustomAuthorizationHandler.OperationContextBasedContextStore();
				}
				Type type = TypeLoader.LoadType(customAuthzAssembly, customAuthzTypeName);
				if (type != null)
				{
					this.customAuthorization = type.Assembly.CreateInstance(type.FullName) as CustomAuthorization;
					ExceptionHelpers.ThrowArgumentExceptionIf("customAuthzTypeName", this.customAuthorization == null, Resources.CustomAuthorizationPluginCreationFailed, new object[0]);
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = customAuthzTypeName;
					objArray[1] = customAuthzAssembly;
					throw new ArgumentException(ExceptionHelpers.GetExceptionMessage(Resources.TypeLoadFromAssebmlyFailed, objArray), "customAuthzTypeName");
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TraceHelper.Current.CustomAuthorizationLoadingFailed(customAuthzAssembly, exception.ToTraceMessage("Exception"));
				if (!exception.IsSevereException())
				{
					throw new TypeLoadException(customAuthzTypeName, customAuthzAssembly, Utils.GetBaseBinDirectory(customAuthzAssembly), exception);
				}
				else
				{
					throw;
				}
			}
			TraceHelper.Current.CustomAuthorizationLoadedSuccessfully(customAuthzTypeName);
		}

		public SafeRefCountedContainer<WindowsIdentity> GetAuthorizedUserIdentity(UserContext userContext)
		{
			CustomAuthorizationHandler.CustomContext context = this.customContextStore.GetContext();
			if (context == null || context.Identity == null)
			{
				throw new UnauthorizedAccessException(userContext.Name, userContext.AuthenticationType, userContext.IsAuthenticated);
			}
			else
			{
				return context.Identity;
			}
		}

		public DataContext GetDataContext()
		{
			return this.customContextStore.GetContext().DataContext;
		}

		public string GetMembershipId(UserContext userContext, Uri resourceUri)
		{
			string membershipId;
			string str;
			TraceHelper.Current.MethodCall0("CustomAuthorizationHandler", "GetMembershipId");
			SenderInfo senderInfo = new SenderInfo(userContext.GetIdentity(), userContext.ClientCertificate, resourceUri);
			try
			{
				using (OperationTracerWithTimeout operationTracerWithTimeout = new OperationTracerWithTimeout(new Action<string>(TraceHelper.Current.CustomAuthzCallStart), new Action<string>(TraceHelper.Current.CustomAuthzCallEnd), "GetMembershipId", new Action<string>(TraceHelper.Current.CustomAuthzExceedTimeLimit), 30))
				{
					membershipId = this.customAuthorization.GetMembershipId(senderInfo);
					if (string.IsNullOrEmpty(membershipId))
					{
						object[] objArray = new object[2];
						objArray[0] = "CustomAuthorization.AuthorizeUser";
						objArray[1] = "<null>";
						throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.MethodReturnedInvalidOutput, objArray));
					}
				}
				str = membershipId;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TraceHelper.Current.GetMembershipId(userContext.Name, exception.Message);
				if (!exception.IsSevereException())
				{
					throw new CustomModuleInvocationFailedException(this.customAuthorization.GetType().AssemblyQualifiedName, "GetMembershipId", exception);
				}
				else
				{
					throw;
				}
			}
			return str;
		}

		public UserQuota GetUserQuota(UserContext userContext)
		{
			CustomAuthorizationHandler.CustomContext context = this.customContextStore.GetContext();
			if (context == null || context.Identity == null)
			{
				throw new UnauthorizedAccessException(userContext.Name, userContext.AuthenticationType, userContext.IsAuthenticated);
			}
			else
			{
				return context.UserQuota;
			}
		}

		public int IncrementCmdletExecutionCount(UserContext userContext)
		{
			CustomAuthorizationHandler.CustomContext context = this.customContextStore.GetContext();
			if (context == null || context.Identity == null)
			{
				throw new UnauthorizedAccessException(userContext.Name, userContext.AuthenticationType, userContext.IsAuthenticated);
			}
			else
			{
				CustomAuthorizationHandler.CustomContext customContext = context;
				int cmdletsExecuted = customContext.CmdletsExecuted + 1;
				int num = cmdletsExecuted;
				customContext.CmdletsExecuted = cmdletsExecuted;
				return num;
			}
		}

		public bool IsAuthorized(UserContext userContext, Uri resourceUri)
		{
			SenderInfo senderInfo = new SenderInfo(userContext.GetIdentity(), userContext.ClientCertificate, resourceUri);
			CustomAuthorizationHandler.CustomContext customContext = null;
			TraceHelper.Current.MethodCall0("CustomAuthorizationHandler", "IsAuthorized");
			try
			{
				using (OperationTracerWithTimeout operationTracerWithTimeout = new OperationTracerWithTimeout(new Action<string>(TraceHelper.Current.CustomAuthzCallStart), new Action<string>(TraceHelper.Current.CustomAuthzCallEnd), "AuthorizeUser", new Action<string>(TraceHelper.Current.CustomAuthzExceedTimeLimit), 30))
				{
					UserQuota userQuotum = null;
					WindowsIdentity windowsIdentity = this.customAuthorization.AuthorizeUser(senderInfo, out userQuotum);
					if (windowsIdentity != null)
					{
						if (userQuotum != null)
						{
							TraceHelper.Current.UserQuotaInformation(userContext.Name, userQuotum.MaxConcurrentRequests, userQuotum.MaxRequestsPerTimeSlot, userQuotum.TimeSlotSize);
							customContext = new CustomAuthorizationHandler.CustomContext(windowsIdentity, userQuotum);
						}
						else
						{
							object[] nullQuota = new object[2];
							nullQuota[0] = "CustomAuthorization.AuthorizeUser";
							nullQuota[1] = Resources.NullQuota;
							throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.MethodReturnedInvalidOutput, nullQuota));
						}
					}
					else
					{
						object[] nullWindowsIdentity = new object[2];
						nullWindowsIdentity[0] = "CustomAuthorization.AuthorizeUser";
						nullWindowsIdentity[1] = Resources.NullWindowsIdentity;
						throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(Resources.MethodReturnedInvalidOutput, nullWindowsIdentity));
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (!exception.IsSevereException())
				{
					exception.Trace(null);
				}
				else
				{
					throw;
				}
			}
			this.customContextStore.StoreContext(customContext);
			if (customContext == null || customContext.Identity == null)
			{
				TraceHelper.Current.AuthorizeUserRequestFailed(userContext.Name, userContext.AuthenticationType);
				return false;
			}
			else
			{
				TraceHelper.Current.AuthorizeUserRequestSucceeded(userContext.Name);
				return true;
			}
		}

		public bool IsRequestProcessingStarted(UserContext userContext)
		{
			CustomAuthorizationHandler.CustomContext context = this.customContextStore.GetContext();
			if (context == null || context.Identity == null)
			{
				throw new UnauthorizedAccessException(userContext.Name, userContext.AuthenticationType, userContext.IsAuthenticated);
			}
			else
			{
				return context.IsRequestProcessingStarted;
			}
		}

		public void SetCustomStateStore(CustomAuthorizationHandler.ICustomContextStore context)
		{
			this.customContextStore = context;
		}

		public void SetDataContext(DataContext dataContext)
		{
			this.customContextStore.GetContext().DataContext = dataContext;
		}

		public void SetRequestProcessingState(UserContext userContext, bool requestProcessingState)
		{
			CustomAuthorizationHandler.CustomContext context = this.customContextStore.GetContext();
			if (context == null || context.Identity == null)
			{
				throw new UnauthorizedAccessException(userContext.Name, userContext.AuthenticationType, userContext.IsAuthenticated);
			}
			else
			{
				context.IsRequestProcessingStarted = requestProcessingState;
				return;
			}
		}

		public class CustomContext : IExtension<OperationContext>
		{
			public int CmdletsExecuted
			{
				get;
				set;
			}

			public DataContext DataContext
			{
				get;
				set;
			}

			public SafeRefCountedContainer<WindowsIdentity> Identity
			{
				get;
				private set;
			}

			public bool IsRequestProcessingStarted
			{
				get;
				set;
			}

			public UserQuota UserQuota
			{
				get;
				private set;
			}

			public CustomContext(WindowsIdentity identity, UserQuota quota)
			{
				this.Identity = new SafeRefCountedContainer<WindowsIdentity>(identity);
				this.UserQuota = quota;
				this.CmdletsExecuted = 0;
			}

			public void Attach(OperationContext owner)
			{
			}

			public void Detach(OperationContext owner)
			{
			}
		}

		public interface ICustomContextStore
		{
			CustomAuthorizationHandler.CustomContext GetContext();

			void StoreContext(CustomAuthorizationHandler.CustomContext customContext);
		}

		public class OperationContextBasedContextStore : CustomAuthorizationHandler.ICustomContextStore
		{
			public OperationContextBasedContextStore()
			{
			}


			public CustomAuthorizationHandler.CustomContext GetContext ()
			{
				/* TODO: REVIEW */
				var customContext = OperationContext.Current.Extensions.Find<CustomAuthorizationHandler.CustomContext> ();
				if (customContext == null) {
					customContext = new CustomAuthorizationHandler.CustomContext(System.Security.Principal.WindowsIdentity.GetCurrent (), new UserQuota(100, 100, 1000));
					OperationContext.Current.Extensions.Add(customContext);
				}
				return customContext;
			}

			public void StoreContext(CustomAuthorizationHandler.CustomContext customContext)
			{
				if (customContext != null && customContext.Identity != null)
				{
					CustomAuthorizationHandler.CustomContext customContext1 = OperationContext.Current.Extensions.Find<CustomAuthorizationHandler.CustomContext>();
					if (customContext1 != null)
					{
						OperationContext.Current.Extensions.Remove(customContext1);
					}
					OperationContext.Current.Extensions.Add(customContext);
				}
			}
		}
	}
}