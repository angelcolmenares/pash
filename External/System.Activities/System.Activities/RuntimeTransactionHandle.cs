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
	public sealed class RuntimeTransactionHandle : Handle, IExecutionProperty, IPropertyRegistrationCallback
	{
		public RuntimeTransactionHandle ()
		{
			throw new NotImplementedException ();
		}
		public RuntimeTransactionHandle (Transaction rootTransaction)
		{
			throw new NotImplementedException ();
		}
		
		public bool AbortInstanceOnTransactionFailure { get; set; }
		public bool SuppressTransaction { get; set; }
		
		public void CompleteTransaction (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public void CompleteTransaction (NativeActivityContext context, BookmarkCallback callback)
		{
			throw new NotImplementedException ();
		}
		public Transaction GetCurrentTransaction (AsyncCodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public Transaction GetCurrentTransaction (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public Transaction GetCurrentTransaction (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		void IExecutionProperty.CleanupWorkflowThread ()
		{
			throw new NotImplementedException ();
		}
		void IExecutionProperty.SetupWorkflowThread ()
		{
			throw new NotImplementedException ();
		}
		void IPropertyRegistrationCallback.Register (RegistrationContext context)
		{
			throw new NotImplementedException ();
		}
		void IPropertyRegistrationCallback.Unregister (RegistrationContext context)
		{
			throw new NotImplementedException ();
		}
		public void RequestTransactionContext (NativeActivityContext context, Action<NativeActivityTransactionContext, Object> callback, object state)
		{
			throw new NotImplementedException ();
		}
		public void RequireTransactionContext (NativeActivityContext context, Action<NativeActivityTransactionContext, Object> callback, object state)
		{
			throw new NotImplementedException ();
		}
	}
}
