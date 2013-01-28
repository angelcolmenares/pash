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
	public sealed class If : NativeActivity
	{
		public If ()
		{
			throw new NotImplementedException ();
		}
		public If (Activity<bool> condition)
		{
			throw new NotImplementedException ();
		}
		public If (InArgument<bool> condition)
		{
			throw new NotImplementedException ();
		}
		public If (Expression<Func<ActivityContext, bool>> condition)
		{
			throw new NotImplementedException ();
		}

		[RequiredArgumentAttribute]
		public InArgument<bool> Condition { get; set; }
		public Activity Else { get; set; }
		public Activity Then { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
