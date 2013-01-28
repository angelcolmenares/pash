using System;

namespace TaskScheduler.Implementation
{
	public class OSXRunningTaskCollection : IRunningTaskCollection
	{
		public OSXRunningTaskCollection ()
		{

		}

		#region IEnumerable implementation

		public System.Collections.IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IRunningTaskCollection implementation

		public int Count {
			get { return 0; }
		}

		#endregion
	}
}

