using System;
using System.Diagnostics;
using System.Security;

namespace System.Runtime.Diagnostics
{
	internal class EventTraceActivity
	{
		public Guid ActivityId;

		private static EventTraceActivity empty;

		public static EventTraceActivity Empty
		{
			get
			{
				if (EventTraceActivity.empty == null)
				{
					EventTraceActivity.empty = new EventTraceActivity(Guid.Empty, false);
				}
				return EventTraceActivity.empty;
			}
		}

		public static string Name
		{
			get
			{
				return "E2EActivity";
			}
		}

		public EventTraceActivity(bool setOnThread = false) : this(Guid.NewGuid(), setOnThread)
		{
		}

		public EventTraceActivity(Guid guid, bool setOnThread = false)
		{
			this.ActivityId = guid;
			if (setOnThread)
			{
				this.SetActivityIdOnThread();
			}
		}

		[SecuritySafeCritical]
		public static Guid GetActivityIdFromThread()
		{
			return Trace.CorrelationManager.ActivityId;
		}

		[SecuritySafeCritical]
		public static EventTraceActivity GetFromThreadOrCreate(bool clearIdOnThread = false)
		{
			Guid activityId = Trace.CorrelationManager.ActivityId;
			if (activityId != Guid.Empty)
			{
				if (clearIdOnThread)
				{
					Trace.CorrelationManager.ActivityId = Guid.Empty;
				}
			}
			else
			{
				activityId = Guid.NewGuid();
			}
			return new EventTraceActivity(activityId, false);
		}

		public void SetActivityId(Guid guid)
		{
			this.ActivityId = guid;
		}

		[SecuritySafeCritical]
		private void SetActivityIdOnThread()
		{
			Trace.CorrelationManager.ActivityId = this.ActivityId;
		}
	}
}