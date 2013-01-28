using System;

namespace TaskScheduler.Implementation
{
	public class CronRepetitionPattern : IRepetitionPattern
	{
		public CronRepetitionPattern ()
		{
		}

		#region IRepetitionPattern implementation

		public string Interval {
			get;
			set;
		}

		public string Duration {
			get;
			set;
		}

		public bool StopAtDurationEnd {
			get;
			set;
		}

		#endregion
	}
}

