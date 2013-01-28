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
	public abstract class DelegateArgument : LocationReference
	{
		internal DelegateArgument ()
		{
		}
		public ArgumentDirection Direction { get; internal set; }
		public new string Name { get; set; }
		protected override string NameCore { get { throw new NotImplementedException (); } }

		public object Get (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public override Location GetLocation (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
