using System;

namespace TaskScheduler.Implementation
{
	public class OSXTaskDefinition : ITaskDefinition
	{
		public OSXTaskDefinition ()
		{
		}

		#region ITaskDefinition implementation

		public void _VtblGap1_2 ()
		{

		}

		public void _VtblGap2_2 ()
		{

		}

		public ITriggerCollection Triggers {
			get;
			set;
		}

		public ITaskSettings Settings {
			get;
			set;
		}

		public IPrincipal Principal {
			get;
			set;
		}

		public IActionCollection Actions {
			get;
			set;
		}

		#endregion
	}
}

