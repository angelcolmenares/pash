using System;

namespace TaskScheduler.Implementation
{
	public class OSXLogonTrigger : OSXTrigger, ILogonTrigger
	{
		public OSXLogonTrigger ()
		{

		}

		#region ILogonTrigger implementation

		public void _VtblGap1_1 ()
		{

		}

		public void _VtblGap2_8 ()
		{

		}

		public string Delay {
			get;
			set;
		}

		public string UserId {
			get;
			set;
		}

		#endregion


	}
}

