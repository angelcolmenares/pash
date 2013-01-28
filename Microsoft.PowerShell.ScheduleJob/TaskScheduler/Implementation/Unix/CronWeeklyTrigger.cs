using System;

namespace TaskScheduler.Implementation
{
	public class CronWeeklyTrigger : CronTrigger, IWeeklyTrigger
	{
		public CronWeeklyTrigger ()
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

