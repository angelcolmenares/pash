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
	[ContentProperty ("Value")]
	public sealed class Literal<T> : CodeActivity<T>, IValueSerializableExpression
	{
		public Literal ()
		{
			throw new NotImplementedException ();
		}
		public Literal (T value)
		{
			throw new NotImplementedException ();
		}

		public T Value { get; set; }

		public bool CanConvertToString (IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
		public string ConvertToString (IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
		public bool ShouldSerializeValue ()
		{
			throw new NotImplementedException ();
		}
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		protected override T Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
