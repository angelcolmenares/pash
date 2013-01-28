using System;
using System.Activities.Hosting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace System.Activities
{
	public class WorkflowApplicationIdleEventArgs : WorkflowApplicationEventArgs
	{
		internal WorkflowApplicationIdleEventArgs ()
		{
		}

		public ReadOnlyCollection<BookmarkInfo> Bookmarks { get { throw new NotImplementedException (); } }
	}
}
