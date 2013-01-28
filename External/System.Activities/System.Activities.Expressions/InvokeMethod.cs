using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Windows.Markup;

namespace System.Activities.Expressions
{
	[ContentProperty ("Parameters")]
	public sealed class InvokeMethod<TResult> : AsyncCodeActivity<TResult>
	{
		public Collection<Type> GenericTypeArguments { get { throw new NotImplementedException (); } }
		public string MethodName { get; set; }
		public Collection<Argument> Parameters { get { throw new NotImplementedException (); } }
		public bool RunAsynchronously { get; set; }
		public InArgument Targetobject { get; set; }
		public Type TargetType { get; set; }

		protected override IAsyncResult BeginExecute (AsyncCodeActivityContext context, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		protected override TResult EndExecute (AsyncCodeActivityContext context, IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
