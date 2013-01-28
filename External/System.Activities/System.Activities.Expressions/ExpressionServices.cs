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
	public static class ExpressionServices
	{
		public static Activity<TResult> Convert<TResult> (Expression<Func<ActivityContext, TResult>> expression)
		{
			throw new NotImplementedException ();
		}
		public static Activity<Location<TResult>> ConvertReference<TResult> (Expression<Func<ActivityContext, TResult>> expression)
		{
			throw new NotImplementedException ();
		}
		public static bool TryConvert<TResult> (Expression<Func<ActivityContext, TResult>> expression, out Activity<TResult> result)
		{
			throw new NotImplementedException ();
		}
		public static bool TryConvertReference<TResult> (Expression<Func<ActivityContext, TResult>> expression, out Activity<Location<TResult>> result)
		{
			throw new NotImplementedException ();
		}
	}
}
