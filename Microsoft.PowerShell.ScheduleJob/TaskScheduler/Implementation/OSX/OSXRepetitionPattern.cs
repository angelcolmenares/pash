using System;

namespace TaskScheduler.Implementation
{
	public class OSXRepetitionPattern : IRepetitionPattern
	{
		public OSXRepetitionPattern ()
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

