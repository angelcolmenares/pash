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
	public sealed class AsyncCodeActivityContext : CodeActivityContext
	{
		internal AsyncCodeActivityContext ()
		{
			throw new NotImplementedException ();
		}
		
		public bool IsCancellationRequested { get; private set; }

		public object UserState { get; set; }
		
		public void MarkCanceled ()
		{
			throw new NotImplementedException ();
		}
	}
}
