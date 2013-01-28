using System;

namespace TaskScheduler.Implementation
{
	public class CronTimeTrigger : CronTrigger, ITimeTrigger
	{
		public CronTimeTrigger ()
		{
		}

		#region ITimeTrigger implementation

		public void _VtblGap2_2 ()
		{

		}

		public IRepetitionPattern Repetition {
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

