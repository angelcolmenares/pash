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
	public abstract class AsyncCodeActivity : Activity
	{
		[IgnoreDataMemberAttribute]
		protected override sealed Func<Activity> Implementation { get; set; }

		protected abstract IAsyncResult BeginExecute (AsyncCodeActivityContext context, AsyncCallback callback, object state);

		protected override sealed void CacheMetadata (ActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected virtual void CacheMetadata (CodeActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected virtual void Cancel (AsyncCodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		protected abstract void EndExecute (AsyncCodeActivityContext context, IAsyncResult result);
	}
	
	public abstract class AsyncCodeActivity<TResult> : Activity<TResult>
	{
		[IgnoreDataMemberAttribute]
		protected override sealed Func<Activity> Implementation { get; set; }

		protected abstract IAsyncResult BeginExecute (AsyncCodeActivityContext context, AsyncCallback callback, object state);

		protected override sealed void CacheMetadata (ActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected virtual void CacheMetadata (CodeActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected virtual void Cancel (AsyncCodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		protected abstract TResult EndExecute (AsyncCodeActivityContext context, IAsyncResult result);
	}
}
