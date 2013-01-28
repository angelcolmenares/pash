using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Data.Services;
using System.Globalization;
using System.Management.Automation.Tracing;
using System.Net;

namespace Microsoft.Management.Odata.Core
{
	internal class QuotaSystem
	{
		public LockedPerSecCounter SystemQuotaViolation
		{
			get;
			private set;
		}

		public LockedPerSecCounter UserQuotaViolation
		{
			get;
			private set;
		}

		public QuotaSystem()
		{
			this.SystemQuotaViolation = new LockedPerSecCounter();
			LockedPerSecCounter systemQuotaViolation = this.SystemQuotaViolation;
			systemQuotaViolation.PreResetEventHandler += (object src, PerTimeslotCounter.PreResetEventArgs args) => {
				DateTime time = args.Time;
				TraceHelper.Current.SystemQuotaViolationCount(args.Counter, time.ToString(CultureInfo.CurrentCulture));
			}
			;
			this.UserQuotaViolation = new LockedPerSecCounter();
			LockedPerSecCounter lockedPerSecCounter = this.SystemQuotaViolation;
			lockedPerSecCounter.PreResetEventHandler += (object src, PerTimeslotCounter.PreResetEventArgs args) => {
				DateTime time = args.Time;
				TraceHelper.Current.UserQuotaViolationCount(args.Counter, time.ToString(CultureInfo.CurrentCulture));
			}
			;
		}

		public void CheckCmdletExecutionQuota(UserContext user)
		{
			int num = DataServiceController.Current.IncrementCmdletExecutionCount(user);
			int maxCmdletsPerRequest = DataServiceController.Current.Configuration.PowerShell.Quotas.MaxCmdletsPerRequest;
			if (num <= maxCmdletsPerRequest)
			{
				return;
			}
			else
			{
				TraceHelper.Current.MaxCmdletQuotaViolation((uint)num);
				DataServiceController.Current.PerfCounters.SystemQuotaViolationsPerSec.Increment();
				DataServiceController.Current.QuotaSystem.SystemQuotaViolation.Increment();
				throw new DataServiceException(0x193, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.Forbidden, Resources.CmdletExecutionPerRequestQuotaExceeded, new object[0]));
			}
		}

		public static void ProcessedRequestHandler(object source, DataServiceProcessingPipelineEventArgs args)
		{
			TraceHelper.Current.DebugMessage("QuotaSystem.ProcessedRequestHandler entered");
			if (args != null)
			{
				args.OperationContext.Trace();
			}
			UserContext userContext = new UserContext(CurrentRequestHelper.Identity, CurrentRequestHelper.Certificate);
			if (DataServiceController.Current.IsRequestProcessingStarted(userContext))
			{
				try
				{
					DataServiceController.Current.SetRequestProcessingState(userContext, false);
					UserDataCache.UserDataEnvelope userDataEnvelope = DataServiceController.Current.UserDataCache.Get(userContext);
					using (userDataEnvelope)
					{
						userDataEnvelope.Data.Usage.RequestProcessed();
					}
					TraceHelper.Current.RequestProcessingEnd();
				}
				finally
				{
					DataServiceController.Current.UserDataCache.TryUnlockKey(userContext);
					TraceHelper.Current.DebugMessage("QuotaSystem.ProcessedRequestHandler exited");
				}
				return;
			}
			else
			{
				TraceHelper.Current.DebugMessage("QuotaSystem.ProcessedRequestHandler IsRequestProcessingStarted returned false");
				return;
			}
		}

		public static void ProcessingRequestHandler(object source, DataServiceProcessingPipelineEventArgs args)
		{
			UserData userDatum = null;
			TraceHelper.Current.DebugMessage("QuotaSystem.ProcessingRequestHandler entered");
			if (args != null && args.OperationContext != null)
			{
				TraceHelper.CorrelateWithClientRequestId(args.OperationContext);
			}
			UserContext userContext = new UserContext(CurrentRequestHelper.Identity, CurrentRequestHelper.Certificate);
			if (!DataServiceController.Current.IsRequestProcessingStarted(userContext))
			{
				UserDataCache.UserDataEnvelope userDataEnvelope = DataServiceController.Current.UserDataCache.Get(userContext);
				using (userDataEnvelope)
				{
					UserQuota userQuota = DataServiceController.Current.GetUserQuota(userContext);
					if (args != null)
					{
						Guid activityId = EtwActivity.GetActivityId();
						args.OperationContext.ResponseHeaders.Add("request-id", activityId.ToString());
					}
					if (userDataEnvelope.Data.Usage.QuotaCheckAndUpdate(userContext, userQuota))
					{
						DataServiceController.Current.UserDataCache.TryLockKey(userContext, out userDatum);
					}
					else
					{
						throw new DataServiceException(0x193, ExceptionHelpers.GetDataServiceExceptionMessage(HttpStatusCode.Forbidden, Resources.UserQuotaExceeded, new object[0]));
					}
				}
				DataServiceController.Current.SetRequestProcessingState(userContext, true);
				TraceHelper.Current.RequestProcessingStart();
				DataServiceController.Current.UserDataCache.Trace();
				TraceHelper.Current.DebugMessage("QuotaSystem.ProcessingRequestHandler exited");
				return;
			}
			else
			{
				TraceHelper.Current.DebugMessage("QuotaSystem.ProcessingRequestHandler IsRequestProcessingStarted returned true");
				return;
			}
		}
	}
}