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
	public abstract class InOutArgument : Argument
	{
		internal InOutArgument ()
		{
		}

		public static InOutArgument CreateReference (InOutArgument argumentToReference, string referencedArgumentName)
		{
			throw new NotImplementedException ();
		}
	}
	
	[ContentProperty ("Expression")]
	// FIXME: enable with valid type
	//[TypeConverter (typeof (InOutArgumentConverter))]
	[MonoTODO]
	public sealed class InOutArgument<T> : InOutArgument
	{
		public InOutArgument (Activity<Location<T>> expression)
		{
			throw new NotImplementedException ();
		}
		public InOutArgument (Expression<Func<ActivityContext, T>> expression)
		{
			throw new NotImplementedException ();
		}
		public InOutArgument (Variable variable)
		{
			throw new NotImplementedException ();
		}
		public InOutArgument (Variable<T> variable)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator InOutArgument<T> (Activity<Location<T>> expression)
		{
			throw new NotImplementedException ();
		}
		public static implicit operator InOutArgument<T> (Variable<T> variable)
		{
			throw new NotImplementedException ();
		}

		public static InOutArgument<T> FromExpression (Activity<Location<T>> expression)
		{
			throw new NotImplementedException ();
		}
		public static InOutArgument<T> FromVariable (Variable<T> variable)
		{
			throw new NotImplementedException ();
		}
	}
}
