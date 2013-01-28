using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using System;
using System.Globalization;

namespace Microsoft.Management.Odata.Core
{
	internal class Usage
	{
		private const int DefaultTimeSlot = 30;

		private uint concurrentRequests;

		private PerSecCounter concurrentRequestQuotaViolationsPerSec;

		private PerSecCounter requestPerSecQuotaViolations;

		private PerTimeslotCounter requests;

		private object syncObject;

		public Usage()
		{
			this.syncObject = new object();
			this.concurrentRequestQuotaViolationsPerSec = new PerSecCounter();
			PerSecCounter perSecCounter = this.concurrentRequestQuotaViolationsPerSec;
			perSecCounter.PreResetEventHandler += (object src, PerTimeslotCounter.PreResetEventArgs args) => {
				DateTime time = args.Time;
				TraceHelper.Current.ConcurrentRequestQuotaViolationCount(args.Counter, time.ToString(CultureInfo.CurrentCulture));
			}
			;
			this.requests = new PerTimeslotCounter(30);
			this.requestPerSecQuotaViolations = new PerSecCounter();
			PerSecCounter perSecCounter1 = this.requestPerSecQuotaViolations;
			perSecCounter1.PreResetEventHandler += (object src, PerTimeslotCounter.PreResetEventArgs args) => {
				DateTime time = args.Time;
				TraceHelper.Current.RequestPerSecondQuotaViolationCount(args.Counter, time.ToString(CultureInfo.CurrentCulture));
			}
			;
		}

		private bool CheckConcurrentRequestQuota(int maxConcurrentRequests)
		{
			if ((long)this.concurrentRequests < (long)maxConcurrentRequests)
			{
				return true;
			}
			else
			{
				this.concurrentRequestQuotaViolationsPerSec.Increment();
				return false;
			}
		}

		private bool CheckRequestPerTimeSlotQuota(int maxRequestPerTimeslot, int timeSlotSize)
		{
			if (this.requests.TimeSlot == timeSlotSize)
			{
				if (this.requests.Value < maxRequestPerTimeslot)
				{
					return true;
				}
				else
				{
					this.requestPerSecQuotaViolations.Increment();
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		public bool QuotaCheckAndUpdate(UserContext userContext, UserQuota quota)
		{
			bool flag;
			lock (this.syncObject)
			{
				if (this.CheckConcurrentRequestQuota(quota.MaxConcurrentRequests))
				{
					if (this.CheckRequestPerTimeSlotQuota(quota.MaxRequestsPerTimeSlot, quota.TimeSlotSize))
					{
						Usage usage = this;
						usage.concurrentRequests = usage.concurrentRequests + 1;
						this.requests.Increment(quota.TimeSlotSize);
						TraceHelper.Current.UserQuotaSucceeded(userContext.Name);
						TraceHelper.Current.DebugMessage(string.Concat("Usage.QuotaCheckAndUpdate called. Concurrent requests = ", this.concurrentRequests));
						flag = true;
					}
					else
					{
						DataServiceController.Current.QuotaSystem.UserQuotaViolation.Increment();
						TraceHelper.Current.UserQuotaViolation(userContext.Name, "MaxRequestPerTimeSlot quota violation");
						DataServiceController.Current.PerfCounters.UserQuotaViolationsPerSec.Increment();
						flag = false;
					}
				}
				else
				{
					DataServiceController.Current.QuotaSystem.UserQuotaViolation.Increment();
					TraceHelper.Current.UserQuotaViolation(userContext.Name, "MaxConcurrentRequest quota violation");
					DataServiceController.Current.PerfCounters.UserQuotaViolationsPerSec.Increment();
					flag = false;
				}
			}
			return flag;
		}

		public void RequestProcessed()
		{
			TraceHelper.Current.DebugMessage(string.Concat("Usage.RequestProcessed called. Concurrent requests = ", this.concurrentRequests));
			lock (this.syncObject)
			{
				if (this.concurrentRequests != 0)
				{
					Usage usage = this;
					usage.concurrentRequests = usage.concurrentRequests - 1;
				}
				else
				{
					throw new OverflowException(ExceptionHelpers.GetExceptionMessage(Resources.ConcurrentRequestZero, new object[0]));
				}
			}
		}

		internal int TestHookGetConcurrentRequests()
		{
			return (int)this.concurrentRequests;
		}

		public override string ToString()
		{
			object[] str = new object[4];
			str[0] = " Concurrent requests = ";
			str[1] = this.concurrentRequests;
			str[2] = " ";
			str[3] = this.requests.ToString();
			return string.Concat(str);
		}
	}
}