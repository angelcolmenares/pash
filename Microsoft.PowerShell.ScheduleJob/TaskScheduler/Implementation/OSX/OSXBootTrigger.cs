using System;

namespace TaskScheduler.Implementation
{
	public class OSXBootTrigger : OSXTrigger, IBootTrigger
	{
		public OSXBootTrigger ()
		{

		}

		#region IBootTrigger implementation

		public void _VtblGap2_8 ()
		{
			throw new NotImplementedException ();
		}

		public string Delay {
			get;
			set;
		}

		#endregion
	}
}

