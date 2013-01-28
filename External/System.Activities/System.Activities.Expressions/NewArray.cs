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
	[ContentProperty ("Bounds")]
	public sealed class NewArray<TResult> : CodeActivity<TResult>
	{
		public Collection<Argument> Bounds { get { throw new NotImplementedException (); } }

		protected override TResult Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
