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
	public abstract class OutArgument : Argument
	{
		internal OutArgument ()
		{
		}

		public static OutArgument CreateReference (InOutArgument argumentToReference, string referencedArgumentName)
		{
			throw new NotImplementedException ();
		}

		public static OutArgument CreateReference (OutArgument argumentToReference, string referencedArgumentName)
		{
			throw new NotImplementedException ();
		}
	}
	
	[ContentProperty ("Expression")]
	// FIXME: enable with valid type
	//[TypeConverter (typeof (OutArgumentConverter))]
	[MonoTODO]
	public sealed class OutArgument<T> : OutArgument
	{
		public OutArgument (Activity<Location<T>> expression)
		{
			throw new NotImplementedException ();
		}
		public OutArgument (DelegateArgument delegateArgument)
		{
			throw new NotImplementedException ();
		}
		public OutArgument (Expression<Func<ActivityContext, T>> expression)
		{
			throw new NotImplementedException ();
		}
		public OutArgument (Variable variable)
		{
			throw new NotImplementedException ();
		}
		public static implicit operator OutArgument<T>  (Activity<Location<T>> expression)
		{
			throw new NotImplementedException ();
		}
		public static implicit operator OutArgument<T>  (DelegateArgument delegateArgument)
		{
			throw new NotImplementedException ();
		}
		public static implicit operator OutArgument<T>  (Variable variable)
		{
			throw new NotImplementedException ();
		}

		public static OutArgument<T> FromDelegateArgument (DelegateArgument delegateArgument)
		{
			throw new NotImplementedException ();
		}
		public static OutArgument<T> FromExpression (Activity<Location<T>> expression)
		{
			throw new NotImplementedException ();
		}
		public static OutArgument<T> FromVariable (Variable variable)
		{
			throw new NotImplementedException ();
		}
	}
}
