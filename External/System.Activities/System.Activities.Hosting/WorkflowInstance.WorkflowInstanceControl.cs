using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Threading;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Activities.Tracking;

namespace System.Activities.Hosting
{
	public abstract partial class WorkflowInstance
	{
		protected struct WorkflowInstanceControl
		{
			public static bool operator == (WorkflowInstanceControl left, WorkflowInstanceControl right)
			{
				throw new NotImplementedException ();
			}
			public static bool operator != (WorkflowInstanceControl left, WorkflowInstanceControl right)
			{
				throw new NotImplementedException ();
			}

			public bool HasPendingTrackingRecords { get { throw new NotImplementedException (); } }
			public bool IsPersistable { get { throw new NotImplementedException (); } }
			public WorkflowInstanceState State { get { throw new NotImplementedException (); } }
			public bool TrackingEnabled { get { throw new NotImplementedException (); } }

			public void Abort ()
			{
				throw new NotImplementedException ();
			}
			public void Abort (Exception reason)
			{
				throw new NotImplementedException ();
			}
			public IAsyncResult BeginFlushTrackingRecords (TimeSpan timeout,AsyncCallback callback,object state)
			{
				throw new NotImplementedException ();
			}
			public void EndFlushTrackingRecords (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}
			public override bool Equals (object obj)
			{
				throw new NotImplementedException ();
			}
			public void FlushTrackingRecords (TimeSpan timeout)
			{
				throw new NotImplementedException ();
			}
			public Exception GetAbortReason ()
			{
				throw new NotImplementedException ();
			}
			public ReadOnlyCollection<BookmarkInfo> GetBookmarks ()
			{
				throw new NotImplementedException ();
			}
			public ReadOnlyCollection<BookmarkInfo> GetBookmarks (BookmarkScope scope)
			{
				throw new NotImplementedException ();
			}
			public ActivityInstanceState GetCompletionState ()
			{
				throw new NotImplementedException ();
			}
			public ActivityInstanceState GetCompletionState (out Exception terminationException)
			{
				throw new NotImplementedException ();
			}
			public ActivityInstanceState GetCompletionState (out IDictionary<string, object> outputs,out Exception terminationException)
			{
				throw new NotImplementedException ();
			}
			public override int GetHashCode ()
			{
				throw new NotImplementedException ();
			}
			public IDictionary<string, LocationInfo> GetMappedVariables ()
			{
				throw new NotImplementedException ();
			}
			public void PauseWhenPersistable ()
			{
				throw new NotImplementedException ();
			}
			public object PrepareForSerialization ()
			{
				throw new NotImplementedException ();
			}
			public void RequestPause ()
			{
				throw new NotImplementedException ();
			}
			public void Run ()
			{
				throw new NotImplementedException ();
			}
			public BookmarkResumptionResult ScheduleBookmarkResumption (Bookmark bookmark,object value)
			{
				throw new NotImplementedException ();
			}
			public BookmarkResumptionResult ScheduleBookmarkResumption (Bookmark bookmark,object value,BookmarkScope scope)
			{
				throw new NotImplementedException ();
			}
			public void ScheduleCancel ()
			{
				throw new NotImplementedException ();
			}
			public void Terminate (Exception reason)
			{
				throw new NotImplementedException ();
			}
			public void Track (WorkflowInstanceRecord instanceRecord)
			{
				throw new NotImplementedException ();
			}
		}
	}
}
