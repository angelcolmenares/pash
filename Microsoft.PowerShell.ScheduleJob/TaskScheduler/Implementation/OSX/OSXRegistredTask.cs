using System;

namespace TaskScheduler.Implementation
{
	public class OSXRegistredTask : IRegisteredTask
	{
		public OSXRegistredTask ()
		{

		}

		#region IRegisteredTask implementation

		public void _VtblGap1_7 ()
		{

		}

		public IRunningTaskCollection GetInstances (int flags)
		{
			throw new NotImplementedException ();
		}

		public void _VtblGap2_4 ()
		{

		}

		public void _VtblGap3_3 ()
		{

		}

		public void Stop (int flags)
		{

		}

		public ITaskDefinition Definition {
			get;
			private set;
		}

		#endregion
	}
}

