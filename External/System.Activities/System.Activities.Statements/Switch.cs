using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Transactions;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Windows.Markup;

namespace System.Activities.Statements
{
	[ContentProperty ("Cases")]
	public sealed class Switch<T> : NativeActivity
	{
		public Switch ()
		{
			throw new NotImplementedException ();
		}
		public Switch (Activity<T> expression)
		{
			throw new NotImplementedException ();
		}
		public Switch (Expression<Func<ActivityContext, T>> expression)
		{
			throw new NotImplementedException ();
		}
		public Switch (InArgument<T> expression)
		{
			throw new NotImplementedException ();
		}

		public IDictionary<T, Activity> Cases { get { throw new NotImplementedException (); } }
		public Activity Default { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T> Expression { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
