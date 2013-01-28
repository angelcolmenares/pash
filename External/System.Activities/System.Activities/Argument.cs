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
	public abstract class Argument
	{
		public static Argument Create (Type type, ArgumentDirection direction)
		{
			throw new NotImplementedException ();
		}
		public static Argument CreateReference (Argument argumentToReference, string referencedArgumentName)
		{
			throw new NotImplementedException ();
		}

		public const string ResultValue = "Result";
		// FIXME: verify
		public static readonly int UnspecifiedEvaluationOrder = 0;
		
		public Type ArgumentType { get; internal set; }
		public ArgumentDirection Direction { get; internal set; }
		public int EvaluationOrder { get; set; }
		[IgnoreDataMemberAttribute]
		public ActivityWithResult Expression { get; set; }

		public object Get (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public T Get<T> (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public Location GetLocation (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public void Set (ActivityContext context, object value)
		{
			throw new NotImplementedException ();
		}
	}
}
