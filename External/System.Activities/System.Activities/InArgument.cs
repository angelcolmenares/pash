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
	public abstract class InArgument : Argument
	{
		public static InArgument CreateReference (InArgument argumentToReference, string referencedArgumentName)
		{
			throw new NotImplementedException ();
		}
		public static InArgument CreateReference (InOutArgument argumentToReference, string referencedArgumentName)
		{
			throw new NotImplementedException ();
		}
	}
	
	[ContentProperty ("Expression")]
	// FIXME: enable with valid type
	//[TypeConverter (typeof (InArgumentConverter))]
	[MonoTODO]
	public sealed class InArgument<T> : InArgument
	{
		public InArgument (T constValue)
		{
			throw new NotImplementedException ();
		}

		public InArgument (Activity<T> expression)
		{
			throw new NotImplementedException ();
		}

		public InArgument (DelegateArgument delegateArgument)
		{
			throw new NotImplementedException ();
		}

		public InArgument (Variable variable)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator InArgument<T> (T constValue)
		{
			throw new NotImplementedException ();
		}
		public static implicit operator InArgument<T> (Activity<T> expression)
		{
			throw new NotImplementedException ();
		}
		public static implicit operator InArgument<T> (DelegateArgument delegateArgument)
		{
			throw new NotImplementedException ();
		}
		public static implicit operator InArgument<T> (Variable variable)
		{
			throw new NotImplementedException ();
		}

		public static InArgument<T> FromDelegateArgument (DelegateArgument delegateArgument)
		{
			throw new NotImplementedException ();
		}

		public static InArgument<T> FromExpression (Activity<T> expression)
		{
			throw new NotImplementedException ();
		}

		public static InArgument<T> FromValue (T constValue)
		{
			throw new NotImplementedException ();
		}

		public static InArgument<T> FromVariable (Variable variable)
		{
			throw new NotImplementedException ();
		}
	}
}
