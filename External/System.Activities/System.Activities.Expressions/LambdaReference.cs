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
	public sealed class LambdaReference<T> : CodeActivity<Location<T>>, IValueSerializableExpression
	{
		public LambdaReference (Expression<Func<ActivityContext, T>> locationExpression)
		{
			throw new NotImplementedException ();
		}
		public bool CanConvertToString (IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
		public string ConvertToString (IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}

		protected override Location<T> Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
