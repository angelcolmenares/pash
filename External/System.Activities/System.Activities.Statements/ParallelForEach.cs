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
	public sealed class ParallelForEach<T> : NativeActivity
	{
		public ActivityAction<T> Body { get; set; }
		public Activity<bool> CompletionCondition { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<IEnumerable<T>> Values { get; set; }

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
