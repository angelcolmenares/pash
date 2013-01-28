using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Activities
{
	public class WorkflowApplicationEventArgs : EventArgs
	{
		public Guid InstanceId { get { throw new NotImplementedException (); } }

		public IEnumerable<T> GetInstanceExtensions<T>() where T : class
		{
			throw new NotImplementedException ();
		}
	}
}
