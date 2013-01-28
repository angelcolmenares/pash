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
	public sealed class TransactionScope : NativeActivity
	{
		public bool AbortInstanceOnTransactionFailure { get; set; }
		public Activity Body { get; set; }
		public IsolationLevel IsolationLevel { get; set; }
		public InArgument<TimeSpan> Timeout { get; set; }

		public bool ShouldSerializeIsolationLevel ()
		{
			throw new NotImplementedException ();
		}
		public bool ShouldSerializeTimeout ()
		{
			throw new NotImplementedException ();
		}

		protected override void Execute (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
