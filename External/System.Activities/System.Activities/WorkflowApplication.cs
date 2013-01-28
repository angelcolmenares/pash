using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Transactions;
using System.Windows.Markup;
using System.Xaml;
using System.Xml.Linq;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.Tracking;
using System.Activities.Validation;

namespace System.Activities
{
	public sealed class WorkflowApplication : WorkflowInstance
	{
		public WorkflowApplication (Activity workflowDefinition)
			: base (workflowDefinition)
		{
			throw new NotImplementedException ();
		}

		public WorkflowApplication (Activity workflowDefinition, IDictionary<string, Object> inputs)
			: base (workflowDefinition)
		{
			throw new NotImplementedException ();
		}

		public Action<WorkflowApplicationAbortedEventArgs> Aborted { get; set; }
		public Action<WorkflowApplicationCompletedEventArgs> Completed { get; set; }
		public WorkflowInstanceExtensionManager Extensions { get { throw new NotImplementedException (); } }
		public override Guid Id { get { throw new NotImplementedException (); } }
		public Action<WorkflowApplicationIdleEventArgs> Idle { get; set; }
		public InstanceStore InstanceStore { get; set; }
		public Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction> OnUnhandledException { get; set; }
		public Func<WorkflowApplicationIdleEventArgs, PersistableIdleAction> PersistableIdle { get; set; }
		public Action<WorkflowApplicationEventArgs> Unloaded { get; set; }

		protected internal override bool SupportsInstanceKeys {
			get { throw new NotImplementedException (); }
		}

		public void Abort (string reason)
		{
			throw new NotImplementedException ();
		}
		public void AddInitialInstanceValues (IDictionary<XName, Object> writeOnlyValues)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginCancel (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginCancel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginLoad (Guid instanceId, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginLoad (Guid instanceId, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginLoadRunnableInstance (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginLoadRunnableInstance (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginPersist (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginPersist (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginResumeBookmark (Bookmark bookmark, object value, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginResumeBookmark (string bookmarkName, object value, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginResumeBookmark (Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginResumeBookmark (string bookmarkName, object value, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginRun (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginRun (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginTerminate (Exception reason, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginTerminate (string reason, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginTerminate (Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginTerminate (string reason, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginUnload (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public IAsyncResult BeginUnload (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		public void Cancel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void EndCancel (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public void EndLoad (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public void EndLoadRunnableInstance (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public void EndPersist (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public BookmarkResumptionResult EndResumeBookmark (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public void EndRun (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public void EndTerminate (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public void EndUnload (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		public ReadOnlyCollection<BookmarkInfo> GetBookmarks (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void Load (Guid instanceId)
		{
			throw new NotImplementedException ();
		}
		public void Load (Guid instanceId, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void LoadRunnableInstance (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void Persist (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public BookmarkResumptionResult ResumeBookmark (Bookmark bookmark, object value)
		{
			throw new NotImplementedException ();
		}
		public BookmarkResumptionResult ResumeBookmark (string bookmarkName, object value)
		{
			throw new NotImplementedException ();
		}
		public BookmarkResumptionResult ResumeBookmark (Bookmark bookmark, object value, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public BookmarkResumptionResult ResumeBookmark (string bookmarkName, object value, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void Run ()
		{
			throw new NotImplementedException ();
		}
		public void Run (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void Terminate (Exception reason)
		{
			throw new NotImplementedException ();
		}
		public void Terminate (string reason)
		{
			throw new NotImplementedException ();
		}
		public void Terminate (Exception reason, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void Terminate (string reason, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void Unload ()
		{
			throw new NotImplementedException ();
		}
		public void Unload (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected internal override IAsyncResult OnBeginAssociateKeys (ICollection<InstanceKey> keys, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected internal override IAsyncResult OnBeginPersist (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected internal override IAsyncResult OnBeginResumeBookmark (Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected internal override void OnDisassociateKeys (ICollection<InstanceKey> keys)
		{
			throw new NotImplementedException ();
		}

		protected internal override void OnEndAssociateKeys (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected internal override void OnEndPersist (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected internal override BookmarkResumptionResult OnEndResumeBookmark (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnNotifyPaused ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnNotifyUnhandledException (Exception exception, Activity source, string sourceInstanceId)
		{
			throw new NotImplementedException ();
		}

		protected internal override void OnRequestAbort (Exception reason)
		{
			throw new NotImplementedException ();
		}

	}
	
	public class WorkflowApplicationAbortedEventArgs : WorkflowApplicationEventArgs
	{
		internal WorkflowApplicationAbortedEventArgs ()
		{
		}
		
		public Exception Reason { get; private set; }
	}
}
