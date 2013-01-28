using System;

namespace TaskScheduler.Implementation
{
	public class OSXTaskSettings : ITaskSettings
	{
		public OSXTaskSettings ()
		{
		}

		#region ITaskSettings implementation

		public void _VtblGap1_4 ()
		{

		}

		public void _VtblGap2_6 ()
		{

		}

		public void _VtblGap3_2 ()
		{

		}

		public void _VtblGap4_6 ()
		{

		}

		public bool AllowDemandStart {
			get;
			set;
		}

		public _TASK_INSTANCES_POLICY MultipleInstances {
			get;
			set;
		}

		public bool StopIfGoingOnBatteries {
			get;
			set;
		}

		public bool DisallowStartIfOnBatteries {
			get;
			set;
		}

		public bool RunOnlyIfNetworkAvailable {
			get;
			set;
		}

		public bool Enabled {
			get;
			set;
		}

		public bool Hidden {
			get;
			set;
		}

		public IIdleSettings IdleSettings {
			get;
			set;
		}

		public bool RunOnlyIfIdle {
			get;
			set;
		}

		public bool WakeToRun {
			get;
			set;
		}

		#endregion
	}
}

