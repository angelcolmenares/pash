using System;

namespace TaskScheduler.Implementation
{
	public class OSXWeeklyTrigger : OSXTrigger, IWeeklyTrigger
	{
		public OSXWeeklyTrigger ()
		{

		}

		#region IWeeklyTrigger implementation

		public short DaysOfWeek {
			get;
			set;
		}

		public short WeeksInterval {
			get;
			set;
		}

		public string RandomDelay {
			get;
			set;
		}

		#endregion
	}
}

