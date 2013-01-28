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
	public sealed class RuntimeArgument : LocationReference
	{
		public RuntimeArgument (string name, Type argumentType, ArgumentDirection direction)
		{
			throw new NotImplementedException ();
		}

		public RuntimeArgument (string name, Type argumentType, ArgumentDirection direction, bool isRequired)
		{
			throw new NotImplementedException ();
		}

		public RuntimeArgument (string name, Type argumentType, ArgumentDirection direction, List<string> overloadGroupNames)
		{
			throw new NotImplementedException ();
		}

		public RuntimeArgument (string name, Type argumentType, ArgumentDirection direction, bool isRequired, List<string> overloadGroupNames)
		{
			throw new NotImplementedException ();
		}

		public ArgumentDirection Direction { get; private set; }
		public bool IsRequired { get; private set; }
		public ReadOnlyCollection<string> OverloadGroupNames { get { throw new NotImplementedException (); } }

		protected override string NameCore {
			get { throw new NotImplementedException (); }
		}

		protected override Type TypeCore {
			get { throw new NotImplementedException (); }
		}

		public object Get (ActivityContext context)
		{
			throw new NotImplementedException ();
		}

		public T Get<T> (ActivityContext context)
		{
			throw new NotImplementedException ();
		}

		public override Location GetLocation (ActivityContext context)
		{
			throw new NotImplementedException ();
		}

		public void Set (ActivityContext context, object value)
		{
			throw new NotImplementedException ();
		}
	}
}
