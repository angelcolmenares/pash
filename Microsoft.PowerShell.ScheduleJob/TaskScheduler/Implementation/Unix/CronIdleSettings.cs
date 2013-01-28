using System;

namespace TaskScheduler.Implementation
{
	public class CronIdleSettings : IIdleSettings
	{
		public CronIdleSettings ()
		{
		}

		#region IIdleSettings implementation

		public string IdleDuration {
			get;
			set;
		}

		public string WaitTimeout {
			get;
			set;
		}

		public bool StopOnIdleEnd {
			get;
			set;
		}

		public bool RestartOnIdle {
			get;
			set;
		}

		#endregion
	}
}

