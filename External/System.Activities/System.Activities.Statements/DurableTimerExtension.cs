using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Transactions;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Windows.Markup;

namespace System.Activities.Statements
{
	public class DurableTimerExtension : TimerExtension, IWorkflowInstanceExtension, IDisposable
	{
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		public virtual IEnumerable<Object> GetAdditionalExtensions ()
		{
			throw new NotImplementedException ();
		}
		protected override void OnCancelTimer (Bookmark bookmark)
		{
			throw new NotImplementedException ();
		}
		protected override void OnRegisterTimer (TimeSpan timeout, Bookmark bookmark)
		{
			throw new NotImplementedException ();
		}
		public virtual void SetInstance (WorkflowInstanceProxy instance)
		{
			throw new NotImplementedException ();
		}
	}
}
