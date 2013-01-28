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
	[ContentProperty ("Body")]
	public sealed class DoWhile : NativeActivity
	{
		public DoWhile ()
		{
			throw new NotImplementedException ();
		}
		public DoWhile (Activity<bool> condition)
		{
			throw new NotImplementedException ();
		}
		public DoWhile (Expression<Func<ActivityContext, bool>> condition)
		{
			throw new NotImplementedException ();
		}

		public Activity Body { get; set; }
		public Activity<bool> Condition { get; set; }
		public Collection<Variable> Variables { get { throw new NotImplementedException (); } }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
