using System;

namespace TaskScheduler.Implementation
{
	public class CronExecAction : CronAction, IExecAction
	{
		public CronExecAction ()
		{

		}

		#region IExecAction implementation
		public void _VtblGap1_1 ()
		{

		}
		public string Id {
			get;
			set;
		}
		public string Path {
			get;
			set;
		}
		public string Arguments {
			get;
			set;
		}
		#endregion
	}
}

