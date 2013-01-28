using Microsoft.Management.Odata.Common;
using System;

namespace Microsoft.Management.Odata
{
	public class UserQuota
	{
		private BoundedInteger maxConcurrentRequests;

		private BoundedInteger maxRequestsPerTimeSlot;

		private BoundedInteger timeSlotSize;

		public int MaxConcurrentRequests
		{
			get
			{
				return this.maxConcurrentRequests.Value;
			}
		}

		public int MaxRequestsPerTimeSlot
		{
			get
			{
				return this.maxRequestsPerTimeSlot.Value;
			}
		}

		public int TimeSlotSize
		{
			get
			{
				return this.timeSlotSize.Value;
			}
		}

		public UserQuota(int maxConcurrentRequests, int maxRequestsPerTimeSlot, int timeSlotSize)
		{
			this.maxConcurrentRequests = new BoundedInteger(maxConcurrentRequests, 1, 0x7fffffff);
			this.timeSlotSize = new BoundedInteger(timeSlotSize, 1, 0x7fffffff);
			this.maxRequestsPerTimeSlot = new BoundedInteger(maxRequestsPerTimeSlot, 1, 0x7fffffff);
		}
	}
}