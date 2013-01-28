using System;
using System.Collections.ObjectModel;

namespace System.Activities.Tracking
{
	public class WorkflowInstanceQuery : TrackingQuery
	{
		public WorkflowInstanceQuery ()
		{
			States = new Collection<string> ();
		}
		
		public Collection<string> States { get; private set; }
	}
}
