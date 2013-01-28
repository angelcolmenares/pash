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
	public abstract class Variable : LocationReference
	{
		internal Variable ()
		{
		}

		[IgnoreDataMemberAttribute]
		public ActivityWithResult Default { get; set; }
		public VariableModifiers Modifiers { get; set; }
		public new string Name { get; set; }
		protected override string NameCore { get { throw new NotImplementedException (); } }

		public static Variable Create (string name, Type type, VariableModifiers modifiers)
		{
			throw new NotImplementedException ();
		}
	}
	
	public sealed class Variable<T> : Variable
	{
		public Variable (Expression<Func<ActivityContext, T>> defaultExpression)
		{
			throw new NotImplementedException ();
		}
		public Variable (string name)
		{
			throw new NotImplementedException ();
		}
		public Variable (string name, Expression<Func<ActivityContext, T>> defaultExpression)
		{
			throw new NotImplementedException ();
		}
		public Variable (string name, T defaultValue)
		{
			throw new NotImplementedException ();
		}

		public new Activity<T> Default { get; set; }

		protected override Type TypeCore {
			get { throw new NotImplementedException (); }
		}

		public T Get (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
		
		public override Location GetLocation (ActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
