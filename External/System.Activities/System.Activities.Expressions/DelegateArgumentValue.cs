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
	[ContentProperty ("DelegateArgument")]
	public sealed class DelegateArgumentValue<T> : CodeActivity<T>
	{
		public DelegateArgumentValue ()
		{
			throw new NotImplementedException ();
		}
		public DelegateArgumentValue (DelegateArgument delegateArgument)
		{
			throw new NotImplementedException ();
		}

		public DelegateArgument DelegateArgument { get; set; }

		protected override T Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
