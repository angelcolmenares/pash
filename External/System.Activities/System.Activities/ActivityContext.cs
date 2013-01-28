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
	[MonoTODO]
	public class ActivityContext
	{
		internal ActivityContext ()
		{
			throw new NotImplementedException ();
		}

		public string ActivityInstanceId { get; private set; }
		public WorkflowDataContext DataContext { get; private set; }
		public Guid WorkflowInstanceId { get; private set; }

		public T GetExtension<T> () where T : class
		{
			throw new NotImplementedException ();
		}

		public Location<T> GetLocation<T> (LocationReference locationReference)
		{
			throw new NotImplementedException ();
		}
		
		public object GetValue (Argument argument)
		{
			throw new NotImplementedException ();
		}
		
		public T GetValue<T> (InArgument<T> argument)
		{
			throw new NotImplementedException ();
		}
		
		public T GetValue<T> (InOutArgument<T> argument)
		{
			throw new NotImplementedException ();
		}
		
		public T GetValue<T> (LocationReference locationReference)
		{
			throw new NotImplementedException ();
		}
		
		public T GetValue<T> (OutArgument<T> argument)
		{
			throw new NotImplementedException ();
		}
		
		public object GetValue (RuntimeArgument runtimeArgument)
		{
			throw new NotImplementedException ();
		}
		
		public void SetValue (Argument argument, object value)
		{
			throw new NotImplementedException ();
		}
		
		public void SetValue<T> (InArgument<T> argument, T value)
		{
			throw new NotImplementedException ();
		}
		
		public void SetValue<T> (InOutArgument<T> argument, T value)
		{
			throw new NotImplementedException ();
		}
		
		public void SetValue<T> (LocationReference locationReference, T value)
		{
			throw new NotImplementedException ();
		}
		
		public void SetValue<T> (OutArgument<T> argument, T value)
		{
			throw new NotImplementedException ();
		}
	}
}
