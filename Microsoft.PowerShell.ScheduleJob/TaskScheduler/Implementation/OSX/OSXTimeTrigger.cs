using System;

namespace TaskScheduler.Implementation
{
	public class OSXTimeTrigger : OSXTrigger, ITimeTrigger
	{
		public OSXTimeTrigger ()
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

