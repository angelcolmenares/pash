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
	public class NativeActivityContext : ActivityContext
	{
		internal NativeActivityContext ()
		{
		}

		public BookmarkScope DefaultBookmarkScope { get { throw new NotImplementedException (); } }
		public bool IsCancellationRequested { get { throw new NotImplementedException (); } }
		public ExecutionProperties Properties { get { throw new NotImplementedException (); } }

		public void Abort (Exception reason)
		{
			throw new NotImplementedException ();
		}
		public void AbortChildInstance (ActivityInstance activity)
		{
			throw new NotImplementedException ();
		}
		public void CancelChild (ActivityInstance activityInstance)
		{
			throw new NotImplementedException ();
		}
		public void CancelChildren ()
		{
			throw new NotImplementedException ();
		}
		public Bookmark CreateBookmark (BookmarkCallback callback)
		{
			throw new NotImplementedException ();
		}
		public Bookmark CreateBookmark (string name)
		{
			throw new NotImplementedException ();
		}
		public Bookmark CreateBookmark (BookmarkCallback callback, BookmarkOptions options)
		{
			throw new NotImplementedException ();
		}
		public Bookmark CreateBookmark (string name, BookmarkCallback callback)
		{
			throw new NotImplementedException ();
		}
		public Bookmark CreateBookmark (string name, BookmarkCallback callback, BookmarkOptions options)
		{
			throw new NotImplementedException ();
		}
		public Bookmark CreateBookmark (string name, BookmarkCallback callback, BookmarkScope scope)
		{
			throw new NotImplementedException ();
		}
		public Bookmark CreateBookmark (string name, BookmarkCallback callback, BookmarkScope scope, BookmarkOptions options)
		{
			throw new NotImplementedException ();
		}
		public ReadOnlyCollection<ActivityInstance> GetChildren ()
		{
			throw new NotImplementedException ();
		}
		public object GetValue (Variable variable)
		{
			throw new NotImplementedException ();
		}
		public T GetValue<T> (Variable<T> variable)
		{
			throw new NotImplementedException ();
		}
		public void MarkCanceled ()
		{
			throw new NotImplementedException ();
		}
		public void RemoveAllBookmarks ()
		{
			throw new NotImplementedException ();
		}
		public bool RemoveBookmark (Bookmark bookmark)
		{
			throw new NotImplementedException ();
		}
		public bool RemoveBookmark (string name)
		{
			throw new NotImplementedException ();
		}
		public bool RemoveBookmark (string name, BookmarkScope scope)
		{
			throw new NotImplementedException ();
		}
		public BookmarkResumptionResult ResumeBookmark (Bookmark bookmark, object value)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction (ActivityAction activityAction, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T> (ActivityAction<T> activityAction, T argument, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2> (ActivityAction<T1, T2> activityAction, T1 argument1, T2 argument2, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3> (ActivityAction<T1, T2, T3> activityAction, T1 argument1, T2 argument2, T3 argument3, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4> (ActivityAction<T1, T2, T3, T4> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5> (ActivityAction<T1, T2, T3, T4, T5> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6> (ActivityAction<T1, T2, T3, T4, T5, T6> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7> (ActivityAction<T1, T2, T3, T4, T5, T6, T7> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> (ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, T16 argument16, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleActivity (Activity activity)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleActivity (Activity activity, CompletionCallback onCompleted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleActivity (Activity activity, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleActivity (Activity activity, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleActivity<TResult> (Activity<TResult> activity, CompletionCallback<TResult> onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleDelegate (ActivityDelegate activityDelegate, IDictionary<string, Object> inputParameters, DelegateCompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<TResult> (ActivityFunc<TResult> activityFunc, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T, TResult> (ActivityFunc<T, TResult> activityFunc, T argument, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, TResult> (ActivityFunc<T1, T2, TResult> activityFunc, T1 argument1, T2 argument2, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, TResult> (ActivityFunc<T1, T2, T3, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, TResult> (ActivityFunc<T1, T2, T3, T4, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, TResult> (ActivityFunc<T1, T2, T3, T4, T5, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> (ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8, T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, T16 argument16, CompletionCallback onCompleted, FaultCallback onFaulted)
		{
			throw new NotImplementedException ();
		}
		public void SetValue (Variable variable, object value)
		{
			throw new NotImplementedException ();
		}
		public void SetValue<T> (Variable<T> variable, T value)
		{
			throw new NotImplementedException ();
		}
		public void Track (CustomTrackingRecord record)
		{
			throw new NotImplementedException ();
		}
	}
}
