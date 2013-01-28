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
	public abstract class CodeActivity : Activity
	{
		[IgnoreDataMemberAttribute]
		protected override sealed Func<Activity> Implementation { get; set; }

		protected override sealed void CacheMetadata (ActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected virtual void CacheMetadata (CodeActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected abstract void Execute (CodeActivityContext context);
	}
	
	public abstract class CodeActivity<TResult> : Activity<TResult>
	{
		[IgnoreDataMemberAttribute]
		protected override sealed Func<Activity> Implementation { get; set; }


		protected override sealed void CacheMetadata (ActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected virtual void CacheMetadata (CodeActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}
		protected abstract TResult Execute (CodeActivityContext context);
	}
}
