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
	[DataContract]
	public abstract class Handle
	{
		public string ExecutionPropertyName { get { throw new NotImplementedException (); } }
		public ActivityInstance Owner { get { throw new NotImplementedException (); } }

		protected virtual void OnInitialize (HandleInitializationContext context)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnUninitialize (HandleInitializationContext context)
		{
			throw new NotImplementedException ();
		}

		protected void ThrowIfUninitialized ()
		{
			throw new NotImplementedException ();
		}
	}
}
