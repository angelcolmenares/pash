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
	[ContentProperty ("Parameters")]
	public sealed class InvokeMethod : AsyncCodeActivity
	{
		public Collection<Type> GenericTypeArguments { get { throw new NotImplementedException (); } }
		public string MethodName { get; set; }
		public Collection<Argument> Parameters { get { throw new NotImplementedException (); } }
		public OutArgument Result { get; set; }
		public bool RunAsynchronously { get; set; }
		public InArgument Targetobject { get; set; }
		public Type TargetType { get; set; }

		protected override IAsyncResult BeginExecute (AsyncCodeActivityContext context, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		protected override void EndExecute (AsyncCodeActivityContext context, IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
