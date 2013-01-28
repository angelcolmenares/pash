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
	public sealed class WorkflowInstanceProxy
	{
		internal WorkflowInstanceProxy ()
		{
		}

		public Guid Id { get { throw new NotImplementedException (); } }
		public Activity WorkflowDefinition { get { throw new NotImplementedException (); } }

		public IAsyncResult BeginResumeBookmark (Bookmark bookmark, object value, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginResumeBookmark (Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public BookmarkResumptionResult EndResumeBookmark (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
